using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;

namespace RealDealsAPI.Services
{
    public interface IExternalApiService
    {
        Task<IEnumerable<Movie>> GetMoviesFromAPI();
        Task<MovieDetailsDto> GetMovieDetailsByID(string relatedId, Movie movieDBEntry);
    }
}
