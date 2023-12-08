using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaAB104.DAL;
using ProniaAB104.Models;
using ProniaAB104.ViewModels;
using System.Security.Claims;

namespace ProniaAB104.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();

            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .ThenInclude(bi => bi.Product)
                    .ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (BasketItem item in user.BasketItems)
                {
                    basketVM.Add(new BasketItemVM()
                    {
                        Name = item.Product.Name,
                        Price = item.Product.Price,
                        Count = item.Count,
                        SubTotal = item.Count * item.Product.Price,
                        Image = item.Product.ProductImages.FirstOrDefault().Url,
                        Id = item.Product.Id
                    });
                }
            }
            else
            {
                if (Request.Cookies["Basket"] is not null)
                {
                    List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    foreach (BasketCookieItemVM basketCookieItem in basket)
                    {
                        Product product = await _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);

                        if (product is not null)
                        {
                            BasketItemVM basketItemVM = new BasketItemVM
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Image = product.ProductImages.FirstOrDefault().Url,
                                Price = product.Price,
                                Count = basketCookieItem.Count,
                                SubTotal = product.Price * basketCookieItem.Count,
                            };

                            basketVM.Add(basketItemVM);

                        }
                    }
                }
            }

            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id, string plus)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {  //Database ile ishleyen

                //AppUser user=await _userManager.FindByNameAsync(User.Identity.Name);
                //AppUser user = await _userManager.Users.Include(u=>u.BasketItems).FirstOrDefaultAsync(u=>u.UserName==User.Identity.Name);

                AppUser user = await _userManager.Users.Include(u => u.BasketItems).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user is null) return NotFound();

                BasketItem item = user.BasketItems.FirstOrDefault(b => b.ProductId == id);

                if (item is null)
                {
                    item = new BasketItem
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Price = product.Price,
                        Count = 1,
                    };
                    user.BasketItems.Add(item);
                }
                else
                {
                    item.Count++;

                    //_context.BasketItems.Update(item); ishlemese isdtifade edirik
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                //Cookies ile ishleyen

                List<BasketCookieItemVM> basket;

                if (Request.Cookies["Basket"] is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    BasketCookieItemVM itemVM = basket.FirstOrDefault(b => b.Id == id);
                    if (itemVM is null)
                    {
                        BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                        {
                            Id = id,
                            Count = 1
                        };

                        basket.Add(basketCookieItemVM);
                    }
                    else
                    {
                        itemVM.Count++;
                    }
                }
                else
                {
                    basket = new List<BasketCookieItemVM>();

                    BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                    {
                        Id = id,
                        Count = 1
                    };

                    basket.Add(basketCookieItemVM);
                }

                string json = JsonConvert.SerializeObject(basket);

                Response.Cookies.Append("Basket", json);
            }



            return RedirectToAction(nameof(Index), "plus");
        }
        public async Task<IActionResult> MinusBasket(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            List<BasketCookieItemVM> basket;
            if (Request.Cookies["Basket"] is not null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);
                if (item is not null)
                {
                    item.Count--;
                    if (item.Count == 0)
                    {
                        basket.Remove(item);
                    }
                    string json = JsonConvert.SerializeObject(basket);
                    Response.Cookies.Append("Basket", json);
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RemoveBasket(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            List<BasketCookieItemVM> basket;
            if (Request.Cookies["Basket"] is not null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                var item = basket.FirstOrDefault(b => b.Id == id);
                if (item is not null)
                {
                    basket.Remove(item);

                    string json = JsonConvert.SerializeObject(basket);
                    Response.Cookies.Append("Basket", json);
                }
            }

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Checkout()
        {
            return View();
        }
    }
}
