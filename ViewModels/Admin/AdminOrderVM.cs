using e_commerce.Models;
using e_commerce.ViewModels.Common;

namespace e_commerce.ViewModels.Admin
{
    public class AdminOrderVM
    {
        public PaginatedList<Order> Orders { get; set; } = null!;
        public string? StatusFilter { get; set; }

        public static readonly string[] Statuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
    }
}
