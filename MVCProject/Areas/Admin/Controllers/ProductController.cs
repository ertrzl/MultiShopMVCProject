using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCProject.DAL;
using MVCProject.Models;
using MVCProject.Utilities;
using MVCProject.Utilities.Enums;
using MVCProject.ViewModels;

namespace MVCProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product is null) return NotFound();
            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new ProductCreateVM
            {
                Categories = await GetCategorySelectListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVM vm)
        {
            bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == vm.CategoryId && !c.IsDeleted);
            if (!categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category");
            }

            if (vm.ImageFile is null)
            {
                ModelState.AddModelError("ImageFile", "Please select an image");
            }
            else if (!vm.ImageFile.IsValidType("image"))
            {
                ModelState.AddModelError("ImageFile", "File must be an image");
            }
            else if (!vm.ImageFile.IsValidSize(FileSize.MB, 5))
            {
                ModelState.AddModelError("ImageFile", "File size must be less than 5 MB");
            }

            bool duplicateSku = await _context.Products.AnyAsync(p => p.SKU.ToLower() == vm.SKU.ToLower() && !p.IsDeleted);
            if (duplicateSku)
            {
                ModelState.AddModelError("SKU", "This SKU already exists");
            }

            if (!ModelState.IsValid)
            {
                vm.Categories = await GetCategorySelectListAsync();
                return View(vm);
            }

            string fileName = await vm.ImageFile.CreateFileAsync(_env.WebRootPath, "img", "uploads");

            var product = new Product
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                SKU = vm.SKU,
                CategoryId = vm.CategoryId,
                ImageUrl = "/img/uploads/" + fileName
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (product is null) return NotFound();

            var vm = new ProductUpdateVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,
                Categories = await GetCategorySelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, ProductUpdateVM vm)
        {
            if (id is null || id < 1) return BadRequest();

            Product? existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (existingProduct is null) return NotFound();

            bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == vm.CategoryId && !c.IsDeleted);
            if (!categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category");
            }

            bool duplicateSku = await _context.Products.AnyAsync(p => p.SKU.ToLower() == vm.SKU.ToLower() && p.Id != id && !p.IsDeleted);
            if (duplicateSku)
            {
                ModelState.AddModelError("SKU", "This SKU already exists");
            }

            if (vm.ImageFile is not null)
            {
                if (!vm.ImageFile.IsValidType("image"))
                {
                    ModelState.AddModelError("ImageFile", "File must be an image");
                }
                else if (!vm.ImageFile.IsValidSize(FileSize.MB, 5))
                {
                    ModelState.AddModelError("ImageFile", "File size must be less than 5 MB");
                }
            }

            if (!ModelState.IsValid)
            {
                vm.ExistingImageUrl = existingProduct.ImageUrl;
                vm.Categories = await GetCategorySelectListAsync();
                return View(vm);
            }

            if (vm.ImageFile is not null)
            {
                string oldFileName = Path.GetFileName(existingProduct.ImageUrl);
                oldFileName.DeleteFile(_env.WebRootPath, "img", "uploads");

                string newFileName = await vm.ImageFile.CreateFileAsync(_env.WebRootPath, "img", "uploads");
                existingProduct.ImageUrl = "/img/uploads/" + newFileName;
            }

            existingProduct.Name = vm.Name;
            existingProduct.Description = vm.Description;
            existingProduct.Price = vm.Price;
            existingProduct.SKU = vm.SKU;
            existingProduct.CategoryId = vm.CategoryId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (product is null) return NotFound();

            product.IsDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
        }
    }
}
