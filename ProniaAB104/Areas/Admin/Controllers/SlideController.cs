using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB104.DAL;
using ProniaAB104.Models;

namespace ProniaAB104.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;

        public SlideController(AppDbContext context)
        {
            _context = context;
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
            if (!slide.Photo.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Photo", "File-in type sehvdir");
                return View();
            }
            if (slide.Photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Photo", "Olcusu 2MB dan cox olmamalidir");
                return View();
            }
            FileStream file = new FileStream(@"C:\Users\Xeyal\Desktop\ProniaAB104\ProniaAB104\wwwroot\assets\images\slider\" + slide.Photo.FileName, FileMode.Create);

            await slide.Photo.CopyToAsync(file);
            file.Close();
            slide.Image = slide.Photo.FileName;

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");


            //await _context.Slides.AddAsync(slide);
            //await _context.SaveChangesAsync();

            //return RedirectToAction(nameof(Index));
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
