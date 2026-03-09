# E-Shop — ASP.NET Core MVC E-Commerce Platform

A full-featured e-commerce web application built with **ASP.NET Core MVC**, **Entity Framework Core**, and **ASP.NET Core Identity**.
It supports a public product catalog, a database-backed shopping cart, atomic checkout, file uploads, and a complete admin panel.

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

# Features

## Customer

- Browse, search, and filter the product catalog with pagination (9 per page)
- View product details with an image/video carousel
- Database-backed shopping cart (add, update quantity, remove)
- Atomic checkout:
  - validates stock
  - creates order
  - decreases stock
  - clears cart in a single transaction

- View personal order history and order details
- Role-based redirection after login
  - Customer → Catalog
  - Admin → Dashboard

## Admin

- Dashboard with statistics:
  - total products
  - categories
  - orders
  - pending orders
  - revenue
  - recent orders

- Full CRUD for **Categories** (blocked if products exist)
- Full CRUD for **Products** with file upload (blocked if orders exist)
- List and update **Order statuses**

## Security

- Cookie-based authentication via **ASP.NET Core Identity**
- Password hashing by Identity (bcrypt)
- Role-based authorization
- IDOR protection — customers can only access their own cart and orders
- CSRF protection on all POST forms (`AntiForgeryToken`)

---

# Tech Stack

| Layer     | Technology                            |
| --------- | ------------------------------------- |
| Framework | ASP.NET Core MVC (.NET 10)            |
| ORM       | Entity Framework Core 10 (Code First) |
| Database  | SQL Server                            |
| Auth      | ASP.NET Core Identity                 |
| Frontend  | Bootstrap 5 (local via `wwwroot/lib`) |
| Icons     | Bootstrap Icons (inline SVG)          |

### NuGet Packages

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.3
- `Microsoft.EntityFrameworkCore.SqlServer` 10.0.3
- `Microsoft.EntityFrameworkCore.Tools` 10.0.3

---

# Project Structure

```
e-commerce/
├── e-commerce/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── CatalogController.cs
│   │   ├── CartController.cs
│   │   ├── OrdersController.cs
│   │   └── Admin/
│   │       ├── DashboardController.cs
│   │       ├── CategoriesController.cs
│   │       ├── ProductsController.cs
│   │       └── AdminOrdersController.cs
│
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── DbInitializer.cs
│   │   └── CustomClaimsPrincipalFactory.cs
│
│   ├── Models/
│   │   ├── BaseEntity.cs
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Cart.cs
│   │   ├── CartItem.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── Address.cs
│   │   └── FileEntity.cs
│
│   ├── Repositories/
│   ├── ViewModels/
│   ├── Views/
│   ├── Migrations/
│   ├── wwwroot/
│   ├── appsettings.json
│   └── Program.cs
│
└── .gitignore
```

---

# Architecture & Patterns

## Generic Repository

`IRepository<T>` provides:

- `GetAll()`
- `GetByIdAsync()`
- `AddAsync()`
- `Update()`
- `Delete()`

Saving changes is **not** handled here.

## Unit of Work

`IUnitOfWork` handles:

- `SaveChangesAsync()`
- `BeginTransactionAsync()`
- `CommitTransactionAsync()`
- `RollbackTransactionAsync()`

This ensures operations like **checkout** remain atomic.

---

# Checkout Atomicity

Checkout runs inside a **single database transaction**:

1. Validate stock
2. Create Address
3. Create Order
4. Create OrderItems
5. Decrease product stock
6. Clear Cart

If any step fails → **transaction rollback**.

---

# Database Schema

| Table       | Relationship                    |
| ----------- | ------------------------------- |
| AspNetUsers | Extended Identity user          |
| AspNetRoles | Extended Identity role          |
| Categories  | 1 → many Products               |
| Products    | 1 → many OrderItems / CartItems |
| Carts       | 1 → 1 User                      |
| CartItems   | Cart → Product                  |
| Orders      | User → Address                  |
| OrderItems  | Order → Product                 |
| Addresses   | User → Orders                   |
| Files       | Product media storage           |

All custom tables include:

- `CreatedAt`
- `UpdatedAt`

---

# Getting Started

## Prerequisites

- .NET SDK
- SQL Server
- Visual Studio 2022 or VS Code

---

## Clone the repository

```bash
git clone <your-repo-url>
cd e-commerce
```

---

## Configure connection string

Edit `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Run the application

```bash
dotnet run
```

The application will automatically:

- Apply EF migrations
- Seed roles (Admin & Customer)

---

# Running Migrations

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef migrations remove
```

---

# Roles & Authorization

| Role     | Access                                  |
| -------- | --------------------------------------- |
| Guest    | Catalog browsing                        |
| Customer | Cart, Checkout, Orders                  |
| Admin    | Dashboard, Products, Categories, Orders |

Security rules:

- Admin cannot access customer endpoints
- Customers cannot access admin endpoints
- Users can only access their own orders and cart

---

# File Uploads

Allowed files:

Images

- jpg
- jpeg
- png
- gif
- webp

Videos

- mp4
- webm

Limits:

- Images → 5MB
- Videos → 50MB
- Max 10 files per request

Files are stored in:

```
wwwroot/uploads/product/{productId}/
```

Metadata stored in **Files table**.

---

# Admin Panel

| Route               | Description         |
| ------------------- | ------------------- |
| `/Admin/Dashboard`  | Statistics overview |
| `/Admin/Categories` | Categories CRUD     |
| `/Admin/Products`   | Products CRUD       |
| `/Admin/Orders`     | Orders management   |

Admins can:

- manage products
- manage categories
- update order statuses
- monitor store statistics

---
