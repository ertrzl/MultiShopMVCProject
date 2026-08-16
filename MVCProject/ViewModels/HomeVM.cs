using MVCProject.Models;

namespace MVCProject.ViewModels
{
    public class HomeVM
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
    }
}
