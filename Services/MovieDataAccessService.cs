using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RealDealsAPI.Comparers;
using RealDealsAPI.Data;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;
using System.Linq;
using System.Net.Http.Headers;
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
                    _logger.LogError($"Movies DB update task was not successful : No entries were added");
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
            MoviesDto movieList = new MoviesDto();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(endpoint);

                    // Set the Authorization header with the bearer token
                    client.DefaultRequestHeaders.Add("x-access-token", _apiKey);

                    HttpResponseMessage response = await client.GetAsync("movies");

                    if (response.IsSuccessStatusCode)
                    {
                        string? responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent is not null)
                        {
                            movieList = JsonSerializer.Deserialize<MoviesDto>(responseContent)!;
                        }
                        else
                        {
                            _logger.LogError($"Movies Retrieval Failed from {endpoint}");
                            throw new Exception($"Movies Retrieval Failed from {endpoint}");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Movies Retrieval Failed from {endpoint}");
                        throw new Exception($"Movies Retrieval Failed from {endpoint}");
                    }
                    return movieList;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw new Exception($"Movies Retrieval Failed from {endpoint} : {ex.Message}");
                }
            }
        }
    }
}
