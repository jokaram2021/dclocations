using System.ComponentModel.DataAnnotations;

namespace DcLocations.Api.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AssociatedHero { get; set; }

        [StringLength(100)]
        public string? UniverseRegion { get; set; }

        [StringLength(100)]
        public string? FirstAppearance { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        public string? WikiUrl { get; set; }
    }
}