using Labb3CVApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Labb3CVApi.Data
{
    public class CvDbContext : DbContext
    {
        public CvDbContext(DbContextOptions<CvDbContext> options) : base(options) { }

        public DbSet<Skill> Skills { get; set; }
        public DbSet<Project> Projects { get; set; }
    }
}
