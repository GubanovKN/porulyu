using Microsoft.EntityFrameworkCore;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsRate
    {
        public async Task<List<Rate>> GetAllAsync()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return await context.Rates.ToListAsync();
            }
        }
        public List<Rate> GetAll()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return context.Rates.ToList();
            }
        }
        public async Task<Rate> Get(string Id)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return await context.Rates.Where(p => p.Id == Convert.ToInt64(Id)).FirstOrDefaultAsync();
            }
        }
        public async Task Add(Rate rate)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                await context.Rates.AddAsync(rate);
                await context.SaveChangesAsync();
            }
        }

        public async Task Delete(Rate rate)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                context.Rates.Remove(rate);
                await context.SaveChangesAsync();
            }
        }
    }
}
