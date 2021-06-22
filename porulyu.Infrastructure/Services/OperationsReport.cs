using Microsoft.EntityFrameworkCore;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsReport
    {
        public async Task<List<Domain.Models.Report>> GetReports()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return await context.Reports.ToListAsync();
            }
        }
        public async Task Create(long chatId, string Name, string Link)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                User user = await new OperationsUser().GetUser(chatId);
                await context.Reports.AddAsync(new Domain.Models.Report { DateCreate = DateTime.Now, UserId = user.Id, Name = Name, Link = Link });
                await context.SaveChangesAsync();
            }
        }
    }
}
