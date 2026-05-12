using System;
using System.IO;
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
    public class AttractionsController : Controller
    {
        private readonly TouristGuideContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AttractionsController(TouristGuideContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Attractions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var attraction = await _context.Attractions
                .Include(a => a.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (attraction == null) return NotFound();

            return View(attraction);
        }

        // GET: Attractions/Create?cityId=5
        public async Task<IActionResult> Create(int cityId)
        {
            var city = await _context.Cities.FindAsync(cityId);
            if (city == null) return NotFound();

            return View(new AttractionEditViewModel { CityId = cityId });
        }

        // POST: Attractions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AttractionEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? uniqueFileName = model.PhotoFile != null ? await UploadedFile(model.PhotoFile) : null;

                if (!ModelState.IsValid) return View(model);

                var attraction = new Attraction
                {
                    Name = model.Name,
                    BriefDescription = model.BriefDescription,
                    History = model.History,
                    PhotoUrl = uniqueFileName != null ? "/images/attractions/" + uniqueFileName : null,
                    OpeningHours = model.OpeningHours,
                    Cost = model.Cost,
                    CityId = model.CityId
                };

                _context.Add(attraction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Cities", new { id = model.CityId });
            }
            return View(model);
        }

        // GET: Attractions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var attraction = await _context.Attractions.FindAsync(id);
            if (attraction == null) return NotFound();

            var model = new AttractionEditViewModel
            {
                Id = attraction.Id,
                Name = attraction.Name,
                BriefDescription = attraction.BriefDescription,
                History = attraction.History,
                ExistingPhotoUrl = attraction.PhotoUrl,
                OpeningHours = attraction.OpeningHours,
                Cost = attraction.Cost,
                CityId = attraction.CityId
            };

            return View(model);
        }

        // POST: Attractions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AttractionEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var attraction = await _context.Attractions.FindAsync(id);
                    if (attraction == null) return NotFound();

                    attraction.Name = model.Name;
                    attraction.BriefDescription = model.BriefDescription;
                    attraction.History = model.History;
                    attraction.OpeningHours = model.OpeningHours;
                    attraction.Cost = model.Cost;

                    if (model.PhotoFile != null)
                    {
                        string? uniqueFileName = await UploadedFile(model.PhotoFile);
                        if (!ModelState.IsValid) return View(model);

                        if (attraction.PhotoUrl != null && !attraction.PhotoUrl.StartsWith("http"))
                        {
                            string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, attraction.PhotoUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                                System.IO.File.Delete(oldFilePath);
                        }
                        attraction.PhotoUrl = "/images/attractions/" + uniqueFileName;
                    }

                    _context.Update(attraction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttractionExists(model.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Details", "Cities", new { id = model.CityId });
            }
            return View(model);
        }

        // GET: Attractions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var attraction = await _context.Attractions
                .Include(a => a.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (attraction == null) return NotFound();

            return View(attraction);
        }

        // POST: Attractions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attraction = await _context.Attractions.FindAsync(id);
            if (attraction == null) return NotFound();

            int cityId = attraction.CityId;

            if (attraction.PhotoUrl != null && !attraction.PhotoUrl.StartsWith("http"))
            {
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, attraction.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Attractions.Remove(attraction);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Cities", new { id = cityId });
        }

        private bool AttractionExists(int id) => _context.Attractions.Any(e => e.Id == id);

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

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "attractions");
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
