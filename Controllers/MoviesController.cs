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

        public MoviesController(MovieDataAccessService movieDataAccessService, MovieContext context, IMapper mapper)
        {
            _movieDataAccessService = movieDataAccessService;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CatalogMovie>>> GetMovies()
        {
            List<Movie> availableMovies = await _context.Movies.ToListAsync(); 

            var distinctMovies = availableMovies
           .GroupBy(movie => movie.Title)
           .Select(group => new CatalogMovie
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
    }
}
