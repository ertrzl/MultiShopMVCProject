using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCProject.DAL;
using MVCProject.Models;
using MVCProject.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace MVCProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
             HomeVM homeVM = new HomeVM
            {
                Products = await _context.Products.Include(p => p.Category).ToListAsync(),
                Categories = await _context.Categories.Include(c => c.Products).ToListAsync()
            };
            return View(homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Shop(int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).Where(p => !p.IsDeleted);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && !p.IsDeleted)
                .ToListAsync();

            var model = new DetailVM
            {
                Product = product,
                RelatedProducts = relatedProducts
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int productId, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
            if (product is null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = new Order
            {
                UserId = userId,
                TotalPrice = product.Price * quantity,
                Status = Utilities.Enums.OrderStatus.Pending,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = product.Price
                    }
                }
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Cart));
        }

        [Authorize]
        public IActionResult Checkout()
        {
            return RedirectToAction(nameof(Cart));
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
