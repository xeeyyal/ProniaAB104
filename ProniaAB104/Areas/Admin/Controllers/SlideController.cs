using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB104.DAL;
using ProniaAB104.Models;
using ProniaAB104.Utilities.Extensions;

namespace ProniaAB104.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides=await _context.Slides.ToListAsync();

            return View(slides);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Slide slide)
        {
            if (slide.Photo is null)
            {
                ModelState.AddModelError("Photo", "Shekil mutleq secilmelidir");
                return View();
            }
            if (!slide.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "File-in type sehvdir");
                return View();
            }
            if (slide.Photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Photo", "Olcusu 2MB dan cox olmamalidir");
                return View();
            }

            string fileExtension = Path.GetExtension(slide.Photo.FileName);

            if (string.IsNullOrEmpty(fileExtension) || !IsImageFileExtension(fileExtension))
            {
                ModelState.AddModelError("Photo", "Fayl tipi sehvdir");
                return View();
            }

            string fileName = Guid.NewGuid().ToString() + fileExtension;

            string path = Path.Combine(_env.WebRootPath, @"assets\images\slider\", fileName);
            using (FileStream file = new FileStream(path, FileMode.Create))
            {
                await slide.Photo.CopyToAsync(file);
            }

            slide.Image = fileName;

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool IsImageFileExtension(string fileExtension)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            return allowedExtensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide == null) return NotFound();

            return View(slide);
        }
        
    }
}
