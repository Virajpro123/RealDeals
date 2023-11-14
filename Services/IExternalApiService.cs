using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;

namespace RealDealsAPI.Services
{
    public interface IExternalApiService
    {
        Task<IEnumerable<Movie>> GetMoviesFromAPI(ILogger logger);
        Task<MovieDetailsDto> GetMovieDetailsByID(string relatedId, Movie movieDBEntry);
    }
}
