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
                    Name = "Пробный",
                    Price = 0,
                    CountFilters = 1,
                    CountDays = 2
                },
                new Rate
                {
                    Id = 2,
                    DateCreate = DateTime.Now,
                    Name = "Стандартный",
                    Price = 100,
                    CountFilters = 1,
                    CountDays = 30
                },                
                new Rate
                {
                    Id = 3,
                    DateCreate = DateTime.Now,
                    Name = "VIP",
                    Price = 180,
                    CountFilters = 3,
                    CountDays = 30
                },
                new Rate
                {
                    Id = 4,
                    DateCreate = DateTime.Now,
                    Name = "VIP+",
                    Price = 200,
                    CountFilters = 5,
                    CountDays = 30
                }
            };

            modelBuilder.Entity<Rate>().HasData(Rates);
        }
    }
}
