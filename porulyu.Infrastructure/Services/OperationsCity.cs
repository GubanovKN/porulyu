using Microsoft.EntityFrameworkCore;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsCity
    {
        public async Task<Domain.Models.City> Get(long CityId)
        {
            Domain.Models.City city;

            using (ApplicationContext context = new ApplicationContext())
            {
                city = await context.Cities.FirstOrDefaultAsync(p => p.Id == CityId);
            }

            return city;
        }
    }
}
