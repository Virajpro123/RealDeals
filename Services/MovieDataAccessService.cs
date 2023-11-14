using AutoMapper;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;
using RealDealsAPI.Repositories;

namespace RealDealsAPI.Services
{

    public class MovieDataAccessService : IMovieDataAccessService
    {
        private readonly ILogger<MovieDataAccessService> _logger;
        private readonly IMapper _mapper;
        private readonly IExternalApiService _externalApiService;
        private readonly IMovieRepository _movieRepository;

        public MovieDataAccessService(ILogger<MovieDataAccessService> logger,
             IMapper mapper, IMovieRepository movieRepository
            , IExternalApiService externalApiService)
        {
            _logger = logger;
            _mapper = mapper;
            _movieRepository = movieRepository;
            _externalApiService = externalApiService;
        }

        public async Task<List<Movie>> GetMovies()
        {
            List<Movie> availableMovies = await _movieRepository.GetAllMovies();
            return availableMovies;
        }

        public async Task SaveMoviesToDBTask()
        {
            try
            {
                IEnumerable<Movie> moviesTobeSaved = await _externalApiService.GetMoviesFromAPI(_logger);
                await _movieRepository.AddOrUpdateRangeMovies(moviesTobeSaved, _logger);
                _logger.LogInformation($"Movies DB update task was successful");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Movies DB update task failed : {ex.Message}");
            }
        }

        public async Task<BestDealDTO> GetBestDealDTO(List<MovieDetails> realtimeRetrievedMovieDetails, bool isRetrievedRealTime)
        {
            var bestDealEntry = realtimeRetrievedMovieDetails.OrderBy(movie => float.Parse(movie.Price!)).FirstOrDefault();
            var bestDealEntryMovie = await _movieRepository.GetMovieById(bestDealEntry!.ID);

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
                var cachedMovieDetail = await _movieRepository.GetMovieDetailsById(relatedId);
                if (cachedMovieDetail != null) { cachedMovieDetails.Add(cachedMovieDetail); }
            }

            return cachedMovieDetails;
        }

        public async Task<List<MovieDetails>> GetMovieDetailsRealTime(string[] relatedIDsList)
        {
            return await GetMovieDetailsRealTimeFromApi(relatedIDsList);
        }

        public async Task<List<MovieDetails>> GetMovieDetailsRealTimeFromApi(string[] relatedIDsList)
        {
            List<MovieDetails> realtimeRetrievedMovieDetails = new List<MovieDetails>();
            foreach (var relatedId in relatedIDsList)
            {
                var movieDBEntry = await _movieRepository.GetMovieById(relatedId);
                if (movieDBEntry != null)
                {
                    MovieDetailsDto moviesDetails = await _externalApiService.GetMovieDetailsByID(relatedId, movieDBEntry);

                    realtimeRetrievedMovieDetails.Add(_mapper.Map<MovieDetails>(moviesDetails));
                }
            }
            return realtimeRetrievedMovieDetails;
        }
    }
}
