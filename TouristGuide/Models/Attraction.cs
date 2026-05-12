using System.ComponentModel.DataAnnotations;

namespace TouristGuide.Models
{
    public class Attraction
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
        public string? PhotoUrl { get; set; }

        [Display(Name = "Часы работы")]
        public string? OpeningHours { get; set; }

        [Display(Name = "Стоимость посещения")]
        public string? Cost { get; set; }

        public int CityId { get; set; }
        public City City { get; set; } = default!;
    }
}
