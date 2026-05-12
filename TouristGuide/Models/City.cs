using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TouristGuide.Models
{
    public class City
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название города")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 100 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Укажите регион")]
        [StringLength(100, ErrorMessage = "Название региона слишком длинное")]
        [Display(Name = "Регион")]
        public string Region { get; set; } = default!;

        [Range(0, 50000000, ErrorMessage = "Население должно быть положительным числом")]
        [Display(Name = "Население")]
        public int Population { get; set; }

        [Display(Name = "История")]
        public string? History { get; set; }

        [Display(Name = "Герб (URL)")]
        public string? CoatOfArmsUrl { get; set; }

        [Display(Name = "Фотография")]
        public string? PhotoUrl { get; set; }

        public List<Attraction> Attractions { get; set; } = new List<Attraction>();
    }
}
