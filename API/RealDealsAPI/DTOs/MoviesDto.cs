using System.Text.Json.Serialization;

namespace RealDealsAPI.DTOs
{
    public class MovieDTO
    {
        public required string Title { get; set; }
        public string? Year { get; set; }
        public required string ID { get; set; }
        public string? Type { get; set; }
        public string? Poster { get; set; }
        public string? ProviderInfo { get; set; }
    }

    public class MoviesDto
    {
        [JsonPropertyName("Movies")]
        public List<MovieDTO> Movies { get; set; }
    }
}
