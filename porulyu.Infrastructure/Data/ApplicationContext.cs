using Microsoft.EntityFrameworkCore;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace porulyu.Infrastructure.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Filter> Filters { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<Complain> Complains { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Mark> Marks { get; set; }
        public DbSet<Model> Models { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Region>()
            .HasMany(t => t.Cities)
            .WithOne(p => p.Region)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Mark>()
            .HasMany(t => t.Models)
            .WithOne(p => p.Mark)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
            .HasMany(t => t.Reports)
            .WithOne(p => p.User)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
            .HasMany(t => t.Complains)
            .WithOne(p => p.User)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
            .HasMany(t => t.Filters)
            .WithOne(p => p.User)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
            .HasMany(t => t.Payments)
            .WithOne(p => p.User)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rate>()
            .HasMany(t => t.Users)
            .WithOne(p => p.Rate)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Filter>()
            .HasMany(t => t.Ads)
            .WithOne(p => p.Filter)
            .OnDelete(DeleteBehavior.Cascade);

            List<Region> regions = new OperaitionsData().LoadRegions();

            modelBuilder.Entity<Region>().HasData(regions);

            List<City> cities = new OperaitionsData().LoadCities();

            modelBuilder.Entity<City>().HasData(cities);

            List<Mark> marks = new OperaitionsData().LoadMarks();

            modelBuilder.Entity<Mark>().HasData(marks);

            List<Model> models = new OperaitionsData().LoadModels();

            modelBuilder.Entity<Model>().HasData(models);

            Rate[] Rates =
            {
                new Rate
                {
                    Id = 1,
                    DateCreate = DateTime.Now,
                    Name = "Бесплатный",
                    Price = 0,
                    CountFilters = 0,
                    CountReports = 0,
                    CountDays = 365,
                    CanBuy = false,
                    Demo = false
                },
                new Rate
                {
                    Id = 2,
                    DateCreate = DateTime.Now,
                    Name = "Standart",
                    Price = 0,
                    CountFilters = 2,
                    CountReports = 0,
                    CountDays = 7,
                    CanBuy = false,
                    Demo = true
                },
                new Rate
                {
                    Id = 3,
                    DateCreate = DateTime.Now,
                    Name = "Standart",
                    Price = 100,
                    CountFilters = 2,
                    CountReports = 0,
                    CountDays = 30,
                    CanBuy = true,
                    Demo = false
                },
                new Rate
                {
                    Id = 4,
                    DateCreate = DateTime.Now,
                    Name = "Standart+",
                    Price = 180,
                    CountFilters = 3,
                    CountReports = 1,
                    CountDays = 30,
                    CanBuy = true,
                    Demo = false
                },
                new Rate
                {
                    Id = 5,
                    DateCreate = DateTime.Now,
                    Name = "Premium",
                    Price = 200,
                    CountFilters = 5,
                    CountReports = 1,
                    CountDays = 30,
                    CanBuy = true,
                    Demo = false
                }
            };

            modelBuilder.Entity<Rate>().HasData(Rates);
        }
    }
}
