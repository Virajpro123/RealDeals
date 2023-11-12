using Microsoft.EntityFrameworkCore;
using RealDealsAPI.DTOs;
using RealDealsAPI.Entities;
using System.Reflection.Metadata;

namespace RealDealsAPI.Data
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>()
                .HasOne(e => e.movieDetails)
                .WithOne(e => e.movie)
                .HasForeignKey<MovieDetails>();
            
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieDetails> MovieDetails { get; set; }
    }
}
