using Microsoft.EntityFrameworkCore;
using RealDealsAPI.Data;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;

namespace RealDealsAPI.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly MovieContext _context;
        public MovieRepository(MovieContext context)
        {
            _context = context;
        }

        public async Task<List<Movie>> GetAllMovies()
        {
            List<Movie> availableMovies = await _context.Movies.ToListAsync();
            return availableMovies;
        }

        public async Task<Movie> GetMovieById(string id)
        {
            return await _context.Movies.FindAsync(id);
        }

        public async Task<MovieDetails> GetMovieDetailsById(string id)
        {
            return await _context.MovieDetails.FindAsync(id);
        }

        public async Task<bool> AddOrUpdateRangeMovies(IEnumerable<Movie> movies, ILogger logger)
        {
            foreach (var movie in movies)
            {
                var existingEntry = await GetMovieById(movie.ID);

                if (existingEntry == null)
                {
                    _context.Movies.Add(movie);
                    //_context.MovieDetails.Add(movie.movieDetails!);
                    logger.LogInformation($"Newly added {movie.Title}");
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
                    logger.LogInformation($"Updated movie entry of {movie.Title}");
                }
            }
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
