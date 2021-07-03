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
    public class OperationsReport
    {
        public async Task<List<Domain.Models.Report>> GetReportsUser(Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return await context.Reports.Where(p => p.UserId == user.Id).ToListAsync();
            }
        }
        public async Task<Report> Create(long chatId, string Name, string Link)
        {
            Report report = null;

            using (ApplicationContext context = new ApplicationContext())
            {
                User user = await new OperationsUser().GetUser(chatId);
                report = (await context.Reports.AddAsync(new Domain.Models.Report { DateCreate = DateTime.Now, UserId = user.Id, Name = Name, Link = Link })).Entity;
                await context.SaveChangesAsync();
            }

            return report;
        }
        public async Task<Report> Get(long Id)
        {
            Report report = null;

            using (ApplicationContext context = new ApplicationContext())
            {
                report = await context.Reports.FirstOrDefaultAsync(p => p.Id == Id);
            }

            return report;
        }
        public async Task SetReportPay(bool flag, Report report)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                report.Pay = flag;

                context.Update(report);
                await context.SaveChangesAsync();
            }
        }
    }
}
