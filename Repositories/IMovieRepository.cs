using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;

namespace RealDealsAPI.Repositories
{
    public interface IMovieRepository
    {
        Task<List<Movie>> GetAllMovies();
        Task<Movie> GetMovieById(string id);
        Task<MovieDetails> GetMovieDetailsById(string id);
        Task<bool> AddOrUpdateRangeMovies(IEnumerable<Movie> movies);
    }
}
