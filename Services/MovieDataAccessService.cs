using AutoMapper;
using RealDealsAPI.Comparers;
using RealDealsAPI.Data;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;
using System.Text.Json;

namespace RealDealsAPI.Services
{

    public class MovieDataAccessService
    {
        private readonly IConfiguration _config;
        private readonly string _apiKey;
        private readonly ILogger<MovieDataAccessService> _logger;
        private readonly MovieDTOComparer _movieDTOComparer;
        private readonly IMapper _mapper;
        private readonly MovieContext _context;

        public MovieDataAccessService(IConfiguration config, ILogger<MovieDataAccessService> logger, MovieDTOComparer movieDTOComparer, IMapper mapper, MovieContext context)
        {
            _config = config;
            _apiKey = _config["MovieDataAccess:ApiKey"]!;
            _logger = logger;
            _movieDTOComparer = movieDTOComparer;
            _mapper = mapper;
            _context = context;
        }

        public async Task SaveMoviesToDBTask()
        {
            string CinemaWorldApiEndpoint = _config["MovieDataAccess:CinemaWorldApiEndpoint"]!;
            string FilmWorldApiEndpoint = _config["MovieDataAccess:FilmWorldApiEndpoint"]!;

            try
            {
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

                if (await AddOrUpdateRangeMovies(filmWorldMovies.Concat(cinemaWorldMovies.ToList())))
                {
                    _logger.LogInformation($"Movies DB update task was successful");
                }
                else
                {
                    _logger.LogInformation($"Movies DB update task was successful : No new entries were added");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Movies DB update task failed : {ex.Message}");
            }
        }

        private async Task<bool> AddOrUpdateRangeMovies(IEnumerable<Movie> movies)
        {
            foreach (var movie in movies)
            {
                var existingEntry = _context.Movies.Find(movie.ID);

                if (existingEntry == null)
                {
                    _context.Movies.Add(movie);
                    _logger.LogInformation($"Newly added {movie.Title}");
                }
                else
                {
                    _context.Entry(existingEntry).CurrentValues.SetValues(movie);
                    _logger.LogInformation($"Updated movie entry of {movie.Title}");
                }
            }
            return await _context.SaveChangesAsync() > 0;
        }


        private async Task<MoviesDto> GetMoviesFromEndPoint(string endpoint)
        {
            int maxRetries = 3;
            int retryDelayMilliseconds = 3000;
            MoviesDto movieList = new MoviesDto();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(endpoint);

                // Set the Authorization header with the bearer token
                client.DefaultRequestHeaders.Add("x-access-token", _apiKey);
                for (int retryCount = 0; retryCount < maxRetries; retryCount++)
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
                        if (retryCount < maxRetries - 1)
                        {
                            // If not the last retry, delay before retrying
                            _logger.LogInformation($"Retrying fetching movies from {endpoint} in {retryDelayMilliseconds / 1000} seconds...");
                            Thread.Sleep(retryDelayMilliseconds);
                        }
                    }
                }
                return movieList;
            }
        }
    }
}
