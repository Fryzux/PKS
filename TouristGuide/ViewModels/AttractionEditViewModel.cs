using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TouristGuide.ViewModels
{
    public class AttractionEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название достопримечательности")]
        [Display(Name = "Название")]
        public string Name { get; set; } = default!;

        [Display(Name = "Краткое описание")]
        public string? BriefDescription { get; set; }

        [Display(Name = "История")]
        public string? History { get; set; }

        [Display(Name = "Фотография")]
        public IFormFile? PhotoFile { get; set; }

        public string? ExistingPhotoUrl { get; set; }

        [Display(Name = "Часы работы")]
        public string? OpeningHours { get; set; }

        [Display(Name = "Стоимость посещения")]
        public string? Cost { get; set; }

        public int CityId { get; set; }
    }
}
