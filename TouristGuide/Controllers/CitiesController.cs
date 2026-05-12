using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TouristGuide.Data;
using TouristGuide.Models;
using TouristGuide.ViewModels;

namespace TouristGuide.Controllers
{
    public class CitiesController : Controller
    {
        private readonly TouristGuideContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CitiesController(TouristGuideContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Cities
        public async Task<IActionResult> Index(string searchString)
        {
            var all = await _context.Cities.ToListAsync();

            if (!string.IsNullOrEmpty(searchString))
                all = all.Where(c => c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewData["SearchString"] = searchString;
            return View(all);
        }

        // GET: Cities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.Attractions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        // GET: Cities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CityEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? uniqueFileName = model.PhotoFile != null ? await UploadedFile(model.PhotoFile) : null;
                if (!ModelState.IsValid) return View(model);

                City city = new City
                {
                    Name = model.Name,
                    Region = model.Region,
                    Population = model.Population,
                    History = model.History,
                    CoatOfArmsUrl = model.CoatOfArmsUrl,
                    PhotoUrl = uniqueFileName != null ? "/images/cities/" + uniqueFileName : null
                };

                _context.Add(city);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Cities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            var model = new CityEditViewModel
            {
                Id = city.Id,
                Name = city.Name,
                Region = city.Region,
                Population = city.Population,
                History = city.History,
                CoatOfArmsUrl = city.CoatOfArmsUrl,
                ExistingPhotoUrl = city.PhotoUrl
            };

            return View(model);
        }

        // POST: Cities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CityEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var city = await _context.Cities.FindAsync(id);
                    if (city == null) return NotFound();
                    city.Name = model.Name;
                    city.Region = model.Region;
                    city.Population = model.Population;
                    city.History = model.History;
                    city.CoatOfArmsUrl = model.CoatOfArmsUrl;

                    if (model.PhotoFile != null)
                    {
                        if (city.PhotoUrl != null && !city.PhotoUrl.StartsWith("http"))
                        {
                            string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, city.PhotoUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        string? uniqueFileName = await UploadedFile(model.PhotoFile);
                        if (!ModelState.IsValid) return View(model);
                        city.PhotoUrl = "/images/cities/" + uniqueFileName;
                    }

                    _context.Update(city);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CityExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Cities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.Attractions)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        // POST: Cities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var city = await _context.Cities
                .Include(c => c.Attractions)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (city == null) return NotFound();

            if (city.PhotoUrl != null && !city.PhotoUrl.StartsWith("http"))
            {
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, city.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            foreach (var attraction in city.Attractions)
            {
                if (attraction.PhotoUrl != null && !attraction.PhotoUrl.StartsWith("http"))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, attraction.PhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CityExists(int id)
        {
            return _context.Cities.Any(e => e.Id == id);
        }

        private async Task<string?> UploadedFile(IFormFile file)
        {
            if (file == null) return null;

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(allowedExtensions, e => e == ext))
            {
                ModelState.AddModelError("PhotoFile", "Допустимые форматы: JPG, PNG, GIF, WEBP.");
                return null;
            }

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "cities");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + ext;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            return uniqueFileName;
        }
    }
}
