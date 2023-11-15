using AutoMapper;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;
using System.Text.Json;

namespace RealDealsAPI.Services
{
    /// <summary>
    /// Service Connecting to external API
    /// </summary>
    public class ExternalApiService : IExternalApiService
    {
        private readonly Settings _settings;
        private readonly string _apiKey;
        private readonly IMapper _mapper;
        private readonly int _retryDelayMilliseconds;
        private readonly int _maxRetries;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExternalApiService> _logger;

        public ExternalApiService(Settings settings, IMapper mapper, IHttpClientFactory httpClientFactory, ILogger<ExternalApiService> logger)
        {
            _settings = settings;
            _apiKey = _settings.MovieDataAccess.ApiKey;
            _mapper = mapper;
            _maxRetries = _settings.MovieDataAccess.maxRetries;
            _retryDelayMilliseconds = _settings.MovieDataAccess.retryDelayMilliseconds;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        public async Task<IEnumerable<Movie>> GetMoviesFromAPI()
        {
            string CinemaWorldApiEndpoint = _settings.MovieDataAccess.CinemaWorldApiEndpoint;
            string FilmWorldApiEndpoint = _settings.MovieDataAccess.FilmWorldApiEndpoint;

            List<Movie> filmWorldMovies = (await GetMoviesFromEndPoint(FilmWorldApiEndpoint)).Movies.Select(movie =>
            {
                movie.ProviderInfo = "FilmWorld";
                var movieEntity = _mapper.Map<Movie>(movie);
                return movieEntity;
            }).ToList();

            List<Movie> cinemaWorldMovies = (await GetMoviesFromEndPoint(CinemaWorldApiEndpoint)).Movies.Select(movie =>
            {
                movie.ProviderInfo = "CinemaWorld";
                var movieEntity = _mapper.Map<Movie>(movie);
                return movieEntity;
            }).ToList();

            await AppendMovieDetails(CinemaWorldApiEndpoint, cinemaWorldMovies);
            await AppendMovieDetails(FilmWorldApiEndpoint, filmWorldMovies);

            var moviesTobeSaved = filmWorldMovies.Concat(cinemaWorldMovies.ToList());
            return moviesTobeSaved;
        }

        private async Task AppendMovieDetails(string endpoint, List<Movie> movies)
        {
            _logger.LogInformation($"Starting retrieving Movie Details for movies from {endpoint}");

            foreach (Movie movie in movies)
            {
                var movieDetails = await GetMovieDetailsFromEndPoint(endpoint, movie.ID);
                if (movieDetails != null)
                {
                    movie.movieDetails = _mapper.Map<MovieDetails>(movieDetails);
                    _logger.LogInformation($"Retrieved Movie Details for movie {movie.Title} from {endpoint}");
                }
            }
        }

        private async Task<MovieDetailsDto> GetMovieDetailsFromEndPoint(string endpoint, string movieID)
        {
            MovieDetailsDto movieDetails = new MovieDetailsDto();
            using (HttpClient client = _httpClientFactory.CreateClient())
            {
                client.BaseAddress = new Uri(endpoint);

                // Set the Authorization header with the bearer token
                client.DefaultRequestHeaders.Add("x-access-token", _apiKey);
                for (int retryCount = 0; retryCount < _maxRetries!; retryCount++)
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync($"movie/{movieID}");

                        if (response.IsSuccessStatusCode)
                        {
                            string? responseContent = await response.Content.ReadAsStringAsync();

                            if (responseContent is not null)
                            {
                                movieDetails = JsonSerializer.Deserialize<MovieDetailsDto>(responseContent)!;
                                break;
                            }
                            else
                            {
                                _logger.LogError($"No movie details were returned for movie {movieID} from {endpoint}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        if (retryCount < _maxRetries - 1)
                        {
                            // If not the last retry, delay before retrying
                            _logger.LogInformation($"Retrying fetching movie {movieID} from {endpoint} in {_retryDelayMilliseconds / 1000} seconds...");
                            Thread.Sleep(_retryDelayMilliseconds);
                        }
                    }
                }
            }
            return movieDetails;
        }

        private async Task<MovieDetailsDto> GetMovieDetailsRealtimeFromEndpoint(string endpoint, string movieID)
        {
            MovieDetailsDto movieDetails = new MovieDetailsDto();
            using (HttpClient client = _httpClientFactory.CreateClient())
            {
                client.BaseAddress = new Uri(endpoint);

                // Set the Authorization header with the bearer token
                client.DefaultRequestHeaders.Add("x-access-token", _apiKey);
                HttpResponseMessage response = await client.GetAsync($"movie/{movieID}");

                if (response.IsSuccessStatusCode)
                {
                    string? responseContent = await response.Content.ReadAsStringAsync();
                    movieDetails = JsonSerializer.Deserialize<MovieDetailsDto>(responseContent)!;
                }
            }
            return movieDetails;
        }

        public async Task<MoviesDto> GetMoviesFromEndPoint(string endpoint)
        {
            MoviesDto movieList = new MoviesDto();
            using (HttpClient client = _httpClientFactory.CreateClient())
            {
                client.BaseAddress = new Uri(endpoint);

                // Set the Authorization header with the bearer token
                client.DefaultRequestHeaders.Add("x-access-token", _apiKey);
                for (int retryCount = 0; retryCount < _maxRetries; retryCount++)
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync("movies");

                        if (response.IsSuccessStatusCode)
                        {
                            string? responseContent = await response.Content.ReadAsStringAsync();

                            if (responseContent is not null)
                            {
                                movieList = JsonSerializer.Deserialize<MoviesDto>(responseContent)!;
                                break;
                            }
                            else
                            {
                                _logger.LogError($"No movies returned from {endpoint}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        if (retryCount < _maxRetries - 1)
                        {
                            // If not the last retry, delay before retrying
                            _logger.LogInformation($"Retrying fetching movies from {endpoint} in {_retryDelayMilliseconds / 1000} seconds...");
                            Thread.Sleep(_retryDelayMilliseconds);
                        }
                    }
                }
            }

            return movieList;
        }

        public async Task<MovieDetailsDto> GetMovieDetailsByID(string relatedId, Movie movieDBEntry)
        {
            var endpoint = movieDBEntry.ProviderInfo == "CinemaWorld" ?
                                    _settings.MovieDataAccess.CinemaWorldApiEndpoint : _settings.MovieDataAccess.FilmWorldApiEndpoint;

            MovieDetailsDto moviesDetails = new MovieDetailsDto();

            moviesDetails = await GetMovieDetailsRealtimeFromEndpoint(endpoint, relatedId);
            return moviesDetails;
        }

    }
}
