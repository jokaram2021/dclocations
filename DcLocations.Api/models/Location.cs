namespace DcLocations.Api.Models
{
    public class Location
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? AssociatedHero { get; set; }

        public string? UniverseRegion { get; set; }

        public string? FirstAppearance { get; set; }

        public string? ImageUrl { get; set; }
    }
}