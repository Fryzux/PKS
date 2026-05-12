using Microsoft.EntityFrameworkCore;
using TouristGuide.Models;

namespace TouristGuide.Data
{
    public class TouristGuideContext : DbContext
    {
        public TouristGuideContext(DbContextOptions<TouristGuideContext> options)
            : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<Attraction> Attractions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>()
                .HasMany(c => c.Attractions)
                .WithOne(a => a.City)
                .HasForeignKey(a => a.CityId);
        }
    }
}
