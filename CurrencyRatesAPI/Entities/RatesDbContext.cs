using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyRatesAPI.Entities
{
    public class RatesDbContext : DbContext
    {
        private string _connectionString =
            $"Data Source=(localdb)\\mssqllocaldb;Initial Catalog=CurrencyRatesDb;Integrated Security=True;MultipleActiveResultSets=True";

        public DbSet<DailyRate> DailyRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailyRate>()
                .HasKey(v => new { v.CuerrencyCode, v.Date });

            modelBuilder.Entity<DailyRate>()
                .Property(v => v.CuerrencyCode)
                .HasMaxLength(3)
                .IsRequired();

            modelBuilder.Entity<DailyRate>()
                .Property(v => v.Date)
                .IsRequired();

            modelBuilder.Entity<DailyRate>()
                .Property(v => v.Rate)
                .IsRequired();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
