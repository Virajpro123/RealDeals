using Newtonsoft.Json;

namespace RealDealsAPI.Entities
{
    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
    }

    public class Logging
    {
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }

        [JsonProperty("Microsoft.AspNetCore")]
        public string MicrosoftAspNetCore { get; set; }
        public string Hangfire { get; set; }
    }

    public class MovieDataAccess
    {
        public string FilmWorldApiEndpoint { get; set; }
        public string CinemaWorldApiEndpoint { get; set; }
        public string ApiKey { get; set; }
        public int retryDelayMilliseconds { get; set; }
        public int maxRetries { get; set; }
    }

    public class Settings
    {
        public Logging Logging { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public MovieDataAccess MovieDataAccess { get; set; }
        public string AllowedHosts { get; set; }
    }
}
