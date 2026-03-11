using Microsoft.EntityFrameworkCore;
using MyProject.Models.Entities;

namespace MyProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<EmailToken> EmailTokens { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
