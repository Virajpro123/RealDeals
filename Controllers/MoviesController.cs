using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RealDealsAPI.DTOs;
using RealDealsAPI.Services;

namespace RealDealsAPI.Controllers
{
    [EnableRateLimiting("fixed")]

    public class MoviesController : BaseApiController
    {
        private readonly IMovieDataAccessService _movieDataAccessService;

        public MoviesController(IMovieDataAccessService movieDataAccessService)
        {
            _movieDataAccessService = movieDataAccessService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MovieCatalogDTO>>> GetMovies()
        {
            var availableMovies = await _movieDataAccessService.GetMovies();
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

            return Ok(distinctMovies);
        }

        [HttpGet("GetBestDeal")]
        public async Task<ActionResult<BestDealDTO>> GetBestDeal(string relatedIds)
        {
            var relatedIDsList = relatedIds.Split(' ');
            bool isRetrievedRealTime = true;
            BestDealDTO realtimeRetrievedMovieDetailsDTO = new BestDealDTO();

            try
            {
                realtimeRetrievedMovieDetailsDTO = await _movieDataAccessService.GetBestDealDTO(await _movieDataAccessService.GetMovieDetailsRealTime(relatedIDsList), isRetrievedRealTime);
            }
            catch (Exception)
            {
                isRetrievedRealTime = false;
            }

            if (isRetrievedRealTime)
            {
                return Ok(realtimeRetrievedMovieDetailsDTO);
            }
            else
            {
                var cachedMovieDetails = await _movieDataAccessService.GetMovieDetailsFromDatabase(relatedIDsList);
                if (cachedMovieDetails.Count > 0)
                {
                    var result =  await _movieDataAccessService.GetBestDealDTO(cachedMovieDetails,isRetrievedRealTime);
                    return Ok(result);
                }
            }

            return BadRequest("Error in retrieving Movie Details");
        }
    }
}
