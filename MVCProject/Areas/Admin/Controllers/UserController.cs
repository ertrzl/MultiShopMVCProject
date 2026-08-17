using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCProject.Models;
using MVCProject.Utilities.Enums;
using MVCProject.ViewModels;

namespace MVCProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = nameof(AppRole.Admin))]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roleNames = Enum.GetValues<AppRole>().Select(r => r.ToString()).ToList();
            var users = _userManager.Users.ToList();

            var model = new List<UserVM>();

            foreach (var user in users)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);

                model.Add(new UserVM
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CurrentRole = currentRoles.FirstOrDefault() ?? "-",
                    Roles = roleNames.Select(r => new SelectListItem(r, r)).ToList()
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role)) return BadRequest();

            if (userId == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            if (!await _roleManager.RoleExistsAsync(role)) return BadRequest();

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction(nameof(Index));
        }
    }
}
