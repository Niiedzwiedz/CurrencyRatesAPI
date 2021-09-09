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
            $"Server=DESKTOP-1J71C2D\\SQLEXPRESS;Database=CurrencyRatesDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        public DbSet<DailyRate> DailyRates { get; set; }
        public DbSet<User> AuthorizationTable { get; set; }

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

            modelBuilder.Entity<User>()
                .HasKey(v => v.UserName);

            modelBuilder.Entity<User>()
                .Property(v => v.UserName)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<User>()
                .Property(v => v.Password)
                .IsRequired();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
