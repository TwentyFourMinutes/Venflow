using Microsoft.EntityFrameworkCore;
using Venflow.Shared;

namespace Venflow.Benchmarks.Benchmarks.Models.Configurations
{
    public class BenchmarkDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailContent> EmailContents { get; set; }

        public BenchmarkDbContext() : base(new DbContextOptionsBuilder().UseNpgsql(SecretsHandler.GetConnectionString<Startup>()).Options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .HasMany(x => x.Emails)
                .WithOne(x => x.Person)
                .HasForeignKey(x => x.PersonId);

            modelBuilder.Entity<Person>()
                        .ToTable("People");

            modelBuilder.Entity<Email>()
                .HasMany(x => x.Contents)
                .WithOne(x => x.Email)
                .HasForeignKey(x => x.EmailId);

            modelBuilder.Entity<Email>()
                       .ToTable("Emails");

            modelBuilder.Entity<EmailContent>()
                       .ToTable("EmailContents");
        }
    }
}
