using Microsoft.EntityFrameworkCore;
using porulyu.Domain.Models;
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
        public DbSet<Complain> Complains { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Payment> Payments { get; set; }

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
