using Microsoft.EntityFrameworkCore;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsModel
    {
        public async Task<Domain.Models.Model> Get(long ModelId)
        {
            Domain.Models.Model model;

            using (ApplicationContext context = new ApplicationContext())
            {
                model = await context.Models.FirstOrDefaultAsync(p => p.Id == ModelId);
            }

            return model;
        }
    }
}
