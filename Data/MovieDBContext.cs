using Microsoft.EntityFrameworkCore;
using MoviesAPIDemo.Entities;

namespace MoviesAPIDemo.Data
{
    public class MovieDBContext:DbContext
    {
        public MovieDBContext(DbContextOptions<MovieDBContext> options) : base(options)
        {
            
        }

        public DbSet<Movie> Movie {  get; set; }

        public DbSet<Person> Person { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
