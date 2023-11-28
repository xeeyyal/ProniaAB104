using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB104.Areas.Admin.ViewModels;
using ProniaAB104.DAL;
using ProniaAB104.Models;

namespace ProniaAB104.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                .Include(pt => pt.ProductTags).ThenInclude(pt => pt.Tag)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            //ViewBag olmadan

            //CreateProductVM productVM = new CreateProductVM
            //{
            //    Categories = await _context.Categories.ToListAsync(),
            //    Tags = await _context.Tags.ToListAsync(),
            //    Colors = await _context.Colors.ToListAsync(),
            //    Sizes = await _context.Sizes.ToListAsync()
            //};

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Tags = await _context.Tags.ToListAsync();
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            ViewBag.Colors = await _context.Colors.ToListAsync();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                return View();
            }
            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                ModelState.AddModelError("CategoryId", "Bu adli category movcuddur");
                return View();
            }

            foreach (int tagId in productVM.TagIds)
            {
                bool tagResult = await _context.Tags.AllAsync(t => t.Id == tagId);

                if (tagResult)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
                    ViewBag.Colors = await _context.Colors.ToListAsync();
                    ViewBag.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("TagIds", "Yanlish id melumati gonderilib");
                    return View();
                }
            }

            foreach (int id in productVM.ColorIds)
            {
                bool colorResult = await _context.Colors.AnyAsync(t => t.Id == id);
                if (!colorResult)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
                    ViewBag.Colors = await _context.Colors.ToListAsync();
                    ViewBag.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("ColorIds", "Yanlish id melumati gonderilib");
                    return View(productVM);
                }
            }

            foreach (int id in productVM.SizeIds)
            {
                bool sizeResult = await _context.Sizes.AnyAsync(t => t.Id == id);
                if (!sizeResult)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
                    ViewBag.Colors = await _context.Colors.ToListAsync();
                    ViewBag.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("SizeIds", "There is no such size");
                    return View(productVM);
                }
            }

            Product product = new Product
            {
                Name = productVM.Name,
                SKU = productVM.SKU,
                Description = productVM.Description,
                Price = productVM.Price,
                CategoryId = (int)productVM.CategoryId,
                ProductTags = new List<ProductTag>(),
                ProductColors = new List<ProductColor>(),
                ProductSizes = new List<ProductSize>()
            };

            foreach (int tagId in productVM.TagIds)
            {
                ProductTag productTag = new ProductTag
                {
                    TagId = tagId,
                };
                product.ProductTags.Add(productTag);
            }

            foreach (int colorId in productVM.ColorIds)
            {
                ProductColor productColor = new ProductColor
                {
                    ColorId = colorId,
                };
                product.ProductColors.Add(productColor);
            }

            foreach (int sizeID in productVM.SizeIds)
            {
                ProductSize productSize = new ProductSize
                {
                    SizeId = sizeID,
                };
                product.ProductSizes.Add(productSize);
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products
                .Include(pt=>pt.ProductTags)
                .Include(s=>s.ProductSizes)
                .Include(c=>c.ProductColors)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (product is null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                Description= product.Description,
                SKU= product.SKU,
                Price = product.Price,
                CategoryId=(int)product.CategoryId,
                TagIds=product.ProductTags.Select(t => t.TagId).ToList(),
                SizeIds=product.ProductSizes.Select(s => s.SizeId).ToList(),
                ColorIds=product.ProductColors.Select(c => c.ColorId).ToList(),
                Categories=await _context.Categories.ToListAsync(),
                Tags=await _context.Tags.ToListAsync(),
                Sizes=await _context.Sizes.ToListAsync(),
                Colors=await _context.Colors.ToListAsync()
            };

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id,UpdateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags= await _context.Tags.ToListAsync();
                productVM.Colors= await _context.Colors.ToListAsync();
                productVM.Sizes= await _context.Sizes.ToListAsync();
                return View();
            }

            Product existed = await _context.Products
                .Include(pt=>pt.ProductTags)
                .Include(pc=>pc.ProductColors)
                .Include(ps=>ps.ProductSizes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existed is null) return NotFound();

            bool result=await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);

            if (!result)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags = await _context.Tags.ToListAsync();
                productVM.Colors = await _context.Colors.ToListAsync();
                productVM.Sizes = await _context.Sizes.ToListAsync();
                return View();
            }

            //TAG
            foreach (ProductTag pTag in existed.ProductTags)
            {
                if (!productVM.TagIds.Exists(tId=>tId==pTag.TagId))
                {
                    _context.ProductTags.Remove(pTag);
                }
            }

            foreach (int tagId in productVM.TagIds)
            {
                if (!existed.ProductTags.Any(pt=>pt.TagId==tagId))
                {
                    existed.ProductTags.Add(new ProductTag 
                    { 
                        TagId = tagId 
                    });
                }
            }

            //COLOR
            foreach (ProductColor pColor in existed.ProductColors)
            {
                if (!productVM.ColorIds.Exists(cId => cId == pColor.ColorId))
                {
                    _context.ProductColors.Remove(pColor);
                }
            }

            foreach (int colorId in productVM.ColorIds)
            {
                if (!existed.ProductColors.Any(pc => pc.ColorId == colorId))
                {
                    existed.ProductColors.Add(new ProductColor
                    {
                        ColorId = colorId
                    });
                }
            }
            
            //SIZE
            foreach (ProductSize pSize in existed.ProductSizes)
            {
                if (!productVM.SizeIds.Exists(sId => sId == pSize.SizeId))
                {
                    _context.ProductSizes.Remove(pSize);
                }
            }

            foreach (int sizeId in productVM.SizeIds)
            {
                if (!existed.ProductSizes.Any(ps => ps.SizeId == sizeId))
                {
                    existed.ProductSizes.Add(new ProductSize
                    {
                        SizeId = sizeId
                    });
                }
            }


            existed.Name= productVM.Name;
            existed.Description= productVM.Description;
            existed.Price= productVM.Price;
            existed.SKU= productVM.SKU;
            existed.CategoryId=(int)productVM.CategoryId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            var existed = await _context.Products.FirstOrDefaultAsync(c => c.Id == id);

            if (existed is null) return NotFound();

            _context.Products.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductColors).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            return View(product);
        }
    }
}
