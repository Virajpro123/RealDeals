﻿using Microsoft.EntityFrameworkCore;
using RealDealsAPI.Data;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;
using RealDealsAPI.Services;

namespace RealDealsAPI.Repositories
{
    /// <summary>
    /// Movie DB repo
    /// </summary>
    public class MovieRepository : IMovieRepository
    {
        private readonly ILogger<MovieRepository> _logger;
        private readonly MovieContext _context;
        public MovieRepository(MovieContext context, ILogger<MovieRepository> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<bool> AddOrUpdateRangeMovies(IEnumerable<Movie> movies)
        {
            foreach (var movie in movies)
            {
                var existingEntry = await GetMovieById(movie.ID);

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
