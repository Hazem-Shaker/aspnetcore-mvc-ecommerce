using e_commerce.Models;
using e_commerce.ViewModels.Common;

namespace e_commerce.ViewModels.Orders
{
    public class OrderListVM
    {
        public PaginatedList<Order> Orders { get; set; } = null!;
    }
}
