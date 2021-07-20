using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsAd
    {
        public async Task Create(Domain.Models.Ad Ad, Domain.Models.Filter Filter)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                Ad.FilterId = Filter.Id;

                await context.AddAsync(Ad);
                await context.SaveChangesAsync();
            }
        }

        public List<Domain.Models.Ad> GetByFilter(Domain.Models.Filter Filter, string Site)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return context.Ads.Where(p => p.FilterId == Filter.Id && p.Site == Site).ToList();
            }
        }
    }
}
