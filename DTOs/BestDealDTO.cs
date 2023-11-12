namespace RealDealsAPI.DTOs
{
    public class BestDealDTO
    {
        public required MovieDetailsDto MovieDetails { get; set; }
        public required bool IsRealTime { get; set; }
        public required string Provider { get; set;}

    }
}

