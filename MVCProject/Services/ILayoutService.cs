using MVCProject.Models;

namespace MVCProject.Services
{
    public interface ILayoutService
    {
        Task<List<Category>> GetCategoriesAsync();
    }
}
