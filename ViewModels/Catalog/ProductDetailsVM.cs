using e_commerce.Models;

namespace e_commerce.ViewModels.Catalog
{
    public class ProductDetailsVM
    {
        public Product Product { get; set; } = null!;
        public List<FileEntity> Images { get; set; } = new();
    }
}
