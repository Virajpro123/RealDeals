namespace RealDealsAPI.Entities
{
    public class Movie
    {
        public required string Title { get; set; }
        public string? Year { get; set; }
        public required string ID { get; set; }
        public string? Type { get; set; }
        public string? Poster { get; set; }
        public string? ProviderInfo { get; set; }

    }
}
