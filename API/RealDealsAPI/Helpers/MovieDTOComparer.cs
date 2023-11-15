using RealDealsAPI.DTOs;

namespace RealDealsAPI.Helpers
{
    public class MovieDTOComparer : IEqualityComparer<MovieDTO>
    {
        public bool Equals(MovieDTO x, MovieDTO y)
        {
            return x.Title == y.Title;
        }
        public int GetHashCode(MovieDTO obj)
        {
            return obj.Title.GetHashCode();
        }
    }
}
