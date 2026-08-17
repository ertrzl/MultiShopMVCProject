using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCProject.DAL;
using MVCProject.Models;

namespace MVCProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {

        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async  Task<IActionResult> Index()
        {
            List<Category> categories = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .ToListAsync();

            return View(categories);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            bool result = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower() && !c.IsDeleted);
            if (result)
            {
                ModelState.AddModelError("Name", "This category already exists");
                return View(category);
            }

            category.CreatedAt = DateTime.Now;

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if(id is null || id < 1) return BadRequest();

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (category is  null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Category category)
        {
            if (id is null || id < 1) return BadRequest();

            if (!ModelState.IsValid) return View(category);

            Category? existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (existingCategory is null) return NotFound();

            bool result = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != id && !c.IsDeleted);
            if (result)
            {
                ModelState.AddModelError("Name", "This category already exists");
                return View(category);
            }

            existingCategory.Name = category.Name;
            existingCategory.ImageUrl = category.ImageUrl;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category is null) return NotFound();
            category.IsDeleted = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
