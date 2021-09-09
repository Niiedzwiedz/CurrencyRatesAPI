﻿// <auto-generated />
using System;
using CurrencyRatesAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CurrencyRatesAPI.Migrations
{
    [DbContext(typeof(RatesDbContext))]
    partial class RatesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.9")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CurrencyRatesAPI.Entities.DailyRate", b =>
                {
                    b.Property<string>("CuerrencyCode")
                        .HasMaxLength(3)
                        .HasColumnType("nvarchar(3)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<double>("Rate")
                        .HasColumnType("float");

                    b.HasKey("CuerrencyCode", "Date");

                    b.ToTable("DailyRates");

                    b.HasData(
                        new
                        {
                            CuerrencyCode = "PLN",
                            Date = new DateTime(2021, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Rate = 0.111
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
