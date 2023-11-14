using Microsoft.AspNetCore.Mvc;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;

namespace RealDealsAPI.Services
{
    public interface IMovieDataAccessService
    {
        Task<List<Movie>> GetMovies();
        Task SaveMoviesToDBTask();
        Task<BestDealDTO> GetBestDealDTO(List<MovieDetails> realtimeRetrievedMovieDetails, bool isRetrievedRealTime);
        Task<List<MovieDetails>> GetMovieDetailsFromDatabase(string[] relatedIDsList);
        Task<List<MovieDetails>> GetMovieDetailsRealTime(string[] relatedIDsList);
    }
}
