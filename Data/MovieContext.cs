using Microsoft.EntityFrameworkCore;
using RealDealsAPI.Entities;

namespace RealDealsAPI.Data
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Movie> Movies { get; set; }
    }
}
