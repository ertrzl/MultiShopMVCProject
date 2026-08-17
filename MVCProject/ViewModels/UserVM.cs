using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCProject.ViewModels
{
    public class UserVM
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string CurrentRole { get; set; }
        public List<SelectListItem> Roles { get; set; } = new();
    }
}
