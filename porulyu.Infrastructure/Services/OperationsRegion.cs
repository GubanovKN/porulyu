using Microsoft.EntityFrameworkCore;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsRegion
    {
        public async Task<List<Domain.Models.Region>> GetAll()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return await context.Regions.ToListAsync();
            }
        }
        public async Task<Domain.Models.Region> Get(long RegionId)
        {
            Domain.Models.Region region;

            using (ApplicationContext context = new ApplicationContext())
            {
                region = await context.Regions.Include(p => p.Cities).FirstOrDefaultAsync(p => p.Id == RegionId);
            }

            return region;
        }
    }
}
