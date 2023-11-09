using AutoMapper;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;

namespace RealDealsAPI.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<MovieDTO, Movie>().ReverseMap();
        }
    }
}
