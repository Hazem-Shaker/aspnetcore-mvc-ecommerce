# E-Shop — ASP.NET Core MVC E-Commerce Platform

A full-featured e-commerce web application built with ASP.NET Core MVC, Entity Framework Core, and ASP.NET Core Identity. It supports a public product catalog, a database-backed shopping cart, atomic checkout, file uploads, and a complete admin panel.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Architecture & Patterns](#architecture--patterns)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running Migrations](#running-migrations)
- [Roles & Authorization](#roles--authorization)
- [File Uploads](#file-uploads)
- [Admin Panel](#admin-panel)

---

## Features

### Customer
- Browse, search, and filter the product catalog with pagination (9 per page)
- View product details with an image/video carousel
- Database-backed shopping cart (add, update quantity, remove)
- Atomic checkout: validates stock → creates order → decreases stock → clears cart in a single transaction
- View personal order history and order details
- Role-based redirection after login (Customer → Catalog, Admin → Dashboard)

### Admin
- Dashboard with stats: total products, categories, orders, pending orders, revenue, recent orders
- Full CRUD for **Categories** (blocked if products exist)
- Full CRUD for **Products** with file upload (blocked if orders exist)
- List and update **Order statuses**

### Security
- Cookie-based authentication via ASP.NET Core Identity
- Password hashing by Identity (bcrypt)
- Role-based authorization — admins cannot access customer endpoints and vice versa
- IDOR protection — customers can only access their own cart and orders
- CSRF protection on all POST forms (AntiForgeryToken)

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 (Code First) |
| Database | SQL Server |
| Auth | ASP.NET Core Identity |
| Frontend | Bootstrap 5 (local, via `wwwroot/lib`) |
| Icons | Bootstrap Icons (inline SVG) |

### NuGet Packages
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.3
- `Microsoft.EntityFrameworkCore.SqlServer` 10.0.3
- `Microsoft.EntityFrameworkCore.Tools` 10.0.3

---

## Project Structure

```
e-commerce/
├── e-commerce/                        # Main project
│   ├── Controllers/
│   │   ├── AuthController.cs          # Login, Register, Logout
│   │   ├── CatalogController.cs       # Product listing & details
│   │   ├── CartController.cs          # Cart (Customer only)
│   │   ├── OrdersController.cs        # Checkout & order history (Customer only)
│   │   └── Admin/
│   │       ├── DashboardController.cs
│   │       ├── CategoriesController.cs
│   │       ├── ProductsController.cs
│   │       └── AdminOrdersController.cs
│   ├── Data/
│   │   ├── AppDbContext.cs            # EF Core DbContext + Identity
│   │   ├── DbInitializer.cs           # Seeds Admin & Customer roles
│   │   └── CustomClaimsPrincipalFactory.cs  # Adds FirstName claim
│   ├── Models/
│   │   ├── BaseEntity.cs              # Id, CreatedAt, UpdatedAt
│   │   ├── User.cs                    # Extends IdentityUser<int>
│   │   ├── Role.cs                    # Extends IdentityRole<int>
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Cart.cs / CartItem.cs
│   │   ├── Order.cs / OrderItem.cs
│   │   ├── Address.cs
│   │   └── FileEntity.cs             # Uploaded file metadata
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs         # Generic CRUD interface
│   │   │   ├── IUnitOfWork.cs         # SaveChanges + transactions
│   │   │   ├── IAuthRepository.cs
│   │   │   ├── ICartRepository.cs
│   │   │   ├── IOrderRepository.cs
│   │   │   └── IFileRepository.cs
│   │   ├── Repository.cs
│   │   ├── UnitOfWork.cs
│   │   ├── AuthRepository.cs
│   │   ├── CartRepository.cs
│   │   ├── OrderRepository.cs
│   │   ├── FileRepository.cs
│   │   └── Helpers/
│   │       └── FileValidator.cs       # Type & size validation
│   ├── ViewModels/
│   │   ├── Auth/                      # LoginVM, RegisterVM
│   │   ├── Catalog/                   # ProductListVM, ProductDetailsVM
│   │   ├── Cart/                      # CartVM, CartItemVM
│   │   ├── Checkout/                  # CheckoutVM
│   │   ├── Orders/                    # OrderListVM, OrderDetailsVM
│   │   ├── Admin/                     # CategoryFormVM, ProductFormVM, AdminOrderVM
│   │   └── Common/
│   │       └── PaginatedList.cs       # Generic pagination helper
│   ├── Views/
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml         # Navbar + footer (role-aware)
│   │   │   └── _Pagination.cshtml     # Reusable pagination partial
│   │   ├── Auth/                      # Login, Register, AccessDenied
│   │   ├── Catalog/                   # Index, Details
│   │   ├── Cart/                      # Index
│   │   ├── Checkout/                  # Index
│   │   ├── Orders/                    # Index, Details
│   │   └── Admin/
│   │       ├── Dashboard/
│   │       ├── Categories/            # Index, Create, Edit, Delete
│   │       ├── Products/              # Index, Create, Edit, Delete
│   │       └── Orders/                # Index, Details
│   ├── Migrations/                    # EF Core migrations
│   ├── wwwroot/
│   │   ├── css/site.css
│   │   ├── js/site.js
│   │   ├── lib/bootstrap/             # Bootstrap 5 (local)
│   │   ├── lib/jquery/
│   │   └── uploads/                   # Product images/videos (gitignored)
│   ├── appsettings.json
│   └── Program.cs
└── .gitignore
```

---

## Architecture & Patterns

### Generic Repository
`IRepository<T>` provides `GetAll()`, `GetByIdAsync()`, `AddAsync()`, `Update()`, `Delete()` for any `BaseEntity`. `SaveChangesAsync` is intentionally excluded from the repository — saving is the Unit of Work's responsibility.

### Unit of Work
`IUnitOfWork` wraps the `DbContext` and exposes:
- `SaveChangesAsync()` — persists all pending changes
- `BeginTransactionAsync()` / `CommitTransactionAsync()` / `RollbackTransactionAsync()` — used for atomic operations like checkout

### Checkout Atomicity
The checkout flow runs inside a single database transaction:
1. Validate stock for every cart item
2. Create the `Address` record
3. Create the `Order` record
4. Create `OrderItem` records from cart items
5. Decrement `Product.Stock` for each item
6. Delete all `CartItem` records for the user's cart

If any step fails, the entire transaction rolls back.

### Custom Claims
`CustomClaimsPrincipalFactory` adds `User.FirstName` as a `ClaimTypes.GivenName` claim to the authentication cookie so the navbar can display the user's first name without a database round-trip.

---

## Database Schema

| Table | Key Relationships |
|---|---|
| `AspNetUsers` | Extended with `FirstName`, `LastName`, `CreatedAt`, `UpdatedAt` |
| `AspNetRoles` | Extended with `CreatedAt`, `UpdatedAt` |
| `AspNetUserRoles` | Many-to-many join (Identity managed) |
| `Categories` | One-to-many → `Products` |
| `Products` | FK → `Categories`; one-to-many → `Files`, `OrderItems`, `CartItems` |
| `Carts` | One-to-one with `User`; one-to-many → `CartItems` |
| `CartItems` | FK → `Cart`, `Product` |
| `Orders` | FK → `User`, `Address`; one-to-many → `OrderItems` |
| `OrderItems` | FK → `Order`, `Product` |
| `Addresses` | FK → `User`; one-to-many → `Orders` |
| `Files` | FK → `Products` via `OwnerId`; stores path, type, size |

All custom tables include `CreatedAt` and `UpdatedAt` timestamps, automatically managed by the overridden `SaveChangesAsync` in `AppDbContext`.

**Indexes:** `Products.CategoryId`, `Products.Name`, `Orders.UserId`, `Orders.Status`, `OrderItems.OrderId`, `Files.(OwnerType, OwnerId)`, `Carts.UserId` (unique), `CartItems.(CartId, ProductId)` (unique).

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express or full edition)
- Visual Studio 2022+ or VS Code

### 1. Clone the repository
```bash
git clone <your-repo-url>
cd e-commerce
```

### 2. Configure the connection string
Open `e-commerce/appsettings.json` and update `DefaultConnection`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Run the application
Open the solution in Visual Studio and press **F5**, or from the terminal:
```bash
cd e-commerce
dotnet run
```

The application will automatically:
- Apply all pending EF Core migrations
- Seed the **Admin** and **Customer** roles

### 4. Create your first admin user
1. Register a new account through `/Auth/Register`
2. In SQL Server Management Studio (SSMS), run:
```sql
-- Find the role ID for Admin
SELECT Id FROM AspNetRoles WHERE Name = 'Admin';

-- Find your user ID
SELECT Id FROM AspNetUsers WHERE Email = 'your@email.com';

-- Assign the Admin role
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (<UserId>, <RoleId>);
```
3. Log out and log back in — you will be redirected to the admin dashboard.

---

## Configuration

| Setting | Location | Description |
|---|---|---|
| Connection string | `appsettings.json` | SQL Server connection |
| Password policy | `Program.cs` | Min 8 chars, uppercase + lowercase + digit required |
| Cookie expiry | `Program.cs` | 7 days (`ExpireTimeSpan`) |
| Login path | `Program.cs` | `/Auth/Login` |
| Access denied path | `Program.cs` | `/Auth/AccessDenied` |
| Page size | `CatalogController.cs` | 9 products per page |

---

## Running Migrations

Migrations run automatically on startup. To manage them manually:

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Apply pending migrations
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

---

## Roles & Authorization

| Role | Accessible Areas |
|---|---|
| **Guest** (unauthenticated) | Catalog (browse only), Login, Register |
| **Customer** | Catalog, Product Details, Cart, Checkout, My Orders |
| **Admin** | Admin Dashboard, Categories CRUD, Products CRUD, All Orders |

- Admins are **blocked** from customer endpoints (`[Authorize(Roles = "Customer")]`)
- Customers are **blocked** from admin endpoints (`[Authorize(Roles = "Admin")]`)
- When an admin opens the app, they are redirected to the dashboard — the catalog is never shown
- Customers can only view/modify their own cart and orders (IDOR protection via `UserId` filtering)

---

## File Uploads

Product images and videos are uploaded through the admin product create/edit forms.

**Allowed types:**
- Images: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp` — max **5 MB** each
- Videos: `.mp4`, `.webm` — max **50 MB** each
- Maximum **10 files** per upload request

**Storage:**
- Files are saved to `wwwroot/uploads/product/{productId}/`
- Each file is renamed to a GUID to prevent overwrites
- Metadata (type, original name, size, path) is stored in the `Files` table
- When a product's files are updated, all old files are **deleted** from disk and the database before the new ones are saved
- When a product is deleted, all associated files are also deleted

---

## Admin Panel

All admin routes use **attribute routing** under the `/Admin/` prefix:

| Route | Description |
|---|---|
| `GET /Admin/Dashboard/Index` | Stats overview |
| `GET /Admin/Categories/Index` | List all categories |
| `GET /Admin/Categories/Create` | New category form |
| `GET /Admin/Categories/Edit/{id}` | Edit category |
| `GET /Admin/Categories/Delete/{id}` | Delete confirmation (blocked if has products) |
| `GET /Admin/Products/Index` | List all products |
| `GET /Admin/Products/Create` | New product form with file upload |
| `GET /Admin/Products/Edit/{id}` | Edit product + replace files |
| `GET /Admin/Products/Delete/{id}` | Delete confirmation (blocked if has orders) |
| `GET /Admin/Orders/Index` | All orders with status filter |
| `GET /Admin/Orders/Details/{id}` | Order details + status update |
#   a s p n e t c o r e - m v c - e c o m m e r c e  
 #   a s p n e t c o r e - m v c - e c o m m e r c e  
 #   a s p n e t c o r e - m v c - e c o m m e r c e  
 #   a s p n e t c o r e - m v c - e c o m m e r c e  
 #   a s p n e t c o r e - m v c - e c o m m e r c e  
 #   a s p n e t c o r e - m v c - e c o m m e r c e  
 