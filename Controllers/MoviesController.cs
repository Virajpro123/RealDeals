using API.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealDealsAPI.Data;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;
using RealDealsAPI.Services;

namespace RealDealsAPI.Controllers
{
    public class MoviesController : BaseApiController
    {
        private readonly MovieDataAccessService _movieDataAccessService;
        private readonly MovieContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public MoviesController(MovieDataAccessService movieDataAccessService, MovieContext context, IMapper mapper, IConfiguration config)
        {
            _movieDataAccessService = movieDataAccessService;
            _context = context;
            _mapper = mapper;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<MovieCatalogDTO>>> GetMovies()
        {
            List<Movie> availableMovies = await _context.Movies.ToListAsync();

            var distinctMovies = availableMovies
           .GroupBy(movie => movie.Title)
           .Select(group => new MovieCatalogDTO
           {
               Title = group.First().Title,
               Year = group.First().Year,
               Providers = string.Join(" ", group.Select(movie => movie.ProviderInfo)),
               Type = group.First().Type,
               Poster = group.First().Poster,
               RelatedIDs = string.Join(" ", group.Select(movie => movie.ID)),
           })
           .ToList();

            return distinctMovies;
        }

        [HttpGet("GetBestDeal")]
        public async Task<ActionResult<BestDealDTO>> GetBestDeal(string relatedIds)
        {
            var relatedIDsList = relatedIds.Split(' ');
            bool isRetrievedRealTime = true;
            List<MovieDetails> realtimeRetrievedMovieDetails = new List<MovieDetails>();
            List<MovieDetails> cachedMovieDetails = new List<MovieDetails>();
            foreach (var relatedId in relatedIDsList)
            {
                var movies = await _context.Movies.FindAsync(relatedId);
                if (movies != null)
                {
                    var endpoint = _config[$"MovieDataAccess:{movies!.ProviderInfo}ApiEndpoint"]!;
                    MovieDetailsDto moviesDetails = new MovieDetailsDto();

                    try
                    {
                        moviesDetails = await _movieDataAccessService.GetMovieDetailsFromEndPoint(endpoint, relatedId);
                    }
                    catch
                    {
                        isRetrievedRealTime = false;
                    }
                    realtimeRetrievedMovieDetails.Add(_mapper.Map<MovieDetails>(moviesDetails));
                }
            }

            if (isRetrievedRealTime && realtimeRetrievedMovieDetails.Count > 0)
            {  
                var bestDealEntry = realtimeRetrievedMovieDetails.OrderBy(movie => float.Parse(movie.Price!)).FirstOrDefault();
                var bestDealEntryMovie = await _context.Movies.FindAsync(bestDealEntry!.ID);

                return new BestDealDTO()
                {
                    MovieDetails = _mapper.Map<MovieDetailsDto>(bestDealEntry),
                    IsRealTime = true,
                    Provider = bestDealEntryMovie!.ProviderInfo!,
                };
            }
            else
            {
                foreach (var relatedId in relatedIDsList)
                {
                    var cachedMovieDetail = await _context.MovieDetails.FindAsync(relatedId);
                    if (cachedMovieDetail != null) { cachedMovieDetails.Add(cachedMovieDetail); }
                }
                if (cachedMovieDetails.Count > 0)
                {
                    var bestDealEntryCached = cachedMovieDetails.OrderBy(movie => movie.Price).FirstOrDefault();
                    var bestDealEntryMovie = await _context.Movies.FindAsync(bestDealEntryCached!.ID);

                    return new BestDealDTO()
                    {
                        MovieDetails = _mapper.Map<MovieDetailsDto>(bestDealEntryCached),
                        IsRealTime = false,
                        Provider = bestDealEntryMovie!.ProviderInfo!,
                    };
                }
            }

            return BadRequest("Error in retrieving Movie Details");
        }
    }
}
