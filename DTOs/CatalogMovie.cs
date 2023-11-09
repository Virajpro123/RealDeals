namespace RealDealsAPI.DTOs
{
    public class CatalogMovie
    {
        public required string Title { get; set; }
        public string? Year { get; set; }
        public required string RelatedIDs { get; set; }
        public string? Type { get; set; }
        public string? Poster { get; set; }
        public string? Providers { get; set; }
    }
}
