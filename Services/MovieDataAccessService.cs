using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealDealsAPI.Comparers;
using RealDealsAPI.Data;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;
using System.Text.Json;

namespace RealDealsAPI.Services
{

    public class MovieDataAccessService : IMovieDataAccessService
    {
        private readonly Settings _settings;
        private readonly string _apiKey;
        private readonly ILogger<MovieDataAccessService> _logger;
        private readonly MovieDTOComparer _movieDTOComparer;
        private readonly IMapper _mapper;
        private readonly MovieContext _context;
        private readonly int _retryDelayMilliseconds;
        private readonly int _maxRetries;
        private readonly IHttpClientFactory _httpClientFactory;

        public MovieDataAccessService(Settings settings, ILogger<MovieDataAccessService> logger
            , MovieDTOComparer movieDTOComparer, IMapper mapper, MovieContext context
            , IHttpClientFactory httpClientFactory)
        {
            _settings = settings;
            _apiKey = settings.MovieDataAccess.ApiKey;
            _logger = logger;
            _movieDTOComparer = movieDTOComparer;
            _mapper = mapper;
            _context = context;
            _maxRetries = settings.MovieDataAccess.maxRetries;
            _retryDelayMilliseconds = settings.MovieDataAccess.retryDelayMilliseconds;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Movie>> GetMovies()
        {
            List<Movie> availableMovies = await _context.Movies.ToListAsync();
            return availableMovies;
        }

        public async Task SaveMoviesToDBTask()
        {
            string CinemaWorldApiEndpoint = _settings.MovieDataAccess.CinemaWorldApiEndpoint;
            string FilmWorldApiEndpoint = _settings.MovieDataAccess.FilmWorldApiEndpoint;

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

                await AppendMovieDetails(CinemaWorldApiEndpoint, cinemaWorldMovies);
                await AppendMovieDetails(FilmWorldApiEndpoint, filmWorldMovies);

                await AddOrUpdateRangeMovies(filmWorldMovies.Concat(cinemaWorldMovies.ToList()));
                _logger.LogInformation($"Movies DB update task was successful");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Movies DB update task failed : {ex.Message}");
            }
        }

        private async Task AppendMovieDetails(string endpoint, List<Movie> movies)
        {
            _logger.LogInformation($"Starting retrieving Movie Details for movies from {endpoint}");

            foreach(Movie movie in movies)
            {
                var movieDetails = await GetMovieDetailsFromEndPoint(endpoint, movie.ID);
                if (movieDetails != null)
                {
                    movie.movieDetails = _mapper.Map<MovieDetails>(movieDetails);
                    _logger.LogInformation($"Retrieved Movie Details for movie {movie.Title} from {endpoint}");
                }
            }
        }

        public async Task<BestDealDTO> GetBestDealDTO(List<MovieDetails> realtimeRetrievedMovieDetails, bool isRetrievedRealTime)
        {
            var bestDealEntry = realtimeRetrievedMovieDetails.OrderBy(movie => float.Parse(movie.Price!)).FirstOrDefault();
            var bestDealEntryMovie = await _context.Movies.FindAsync(bestDealEntry!.ID);

            return new BestDealDTO()
            {
                MovieDetails = _mapper.Map<MovieDetailsDto>(bestDealEntry),
                IsRealTime = isRetrievedRealTime,
                Provider = bestDealEntryMovie!.ProviderInfo!,
            };
        }

        public async Task<List<MovieDetails>> GetMovieDetailsFromDatabase(string[] relatedIDsList)
        {
            List<MovieDetails> cachedMovieDetails = new List<MovieDetails>();

            foreach (var relatedId in relatedIDsList)
            {
                var cachedMovieDetail = await _context.MovieDetails.FindAsync(relatedId);
                if (cachedMovieDetail != null) { cachedMovieDetails.Add(cachedMovieDetail); }
            }

            return cachedMovieDetails;
        }

        public async Task<List<MovieDetails>> GetMovieDetailsRealTime(string[] relatedIDsList)
        {
            List<MovieDetails> realtimeRetrievedMovieDetails = new List<MovieDetails>();
            foreach (var relatedId in relatedIDsList)
            {
                var movies = await _context.Movies.FindAsync(relatedId);
                if (movies != null)
                {
                    var endpoint = movies.ProviderInfo == "CinemaWorld" ?
                        _settings.MovieDataAccess.CinemaWorldApiEndpoint : _settings.MovieDataAccess.FilmWorldApiEndpoint;

                    MovieDetailsDto moviesDetails = new MovieDetailsDto();

                    moviesDetails = await GetMovieDetailsRealtimeFromEndpoint(endpoint, relatedId);

                    realtimeRetrievedMovieDetails.Add(_mapper.Map<MovieDetails>(moviesDetails));
                }
            }
            return realtimeRetrievedMovieDetails;
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

        private async Task<MoviesDto> GetMoviesFromEndPoint(string endpoint)
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

        private async Task<bool> AddOrUpdateRangeMovies(IEnumerable<Movie> movies)
        {
            foreach (var movie in movies)
            {
                var existingEntry = _context.Movies.Find(movie.ID);

                if (existingEntry == null)
                {
                    _context.Movies.Add(movie);
                    //_context.MovieDetails.Add(movie.movieDetails!);
                    _logger.LogInformation($"Newly added {movie.Title}");
                }
                else
                {
                    _context.Entry(existingEntry).CurrentValues.SetValues(movie);
                    var existingDetailsEntry = _context.MovieDetails.Find(movie.ID);
                    if (existingDetailsEntry == null)
                    {
                        _context.MovieDetails.Add(movie.movieDetails!);
                    }
                    else
                    {
                        _context.Entry(existingDetailsEntry).CurrentValues.SetValues(movie.movieDetails!);
                    }
                        _logger.LogInformation($"Updated movie entry of {movie.Title}");
                }
            }
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
