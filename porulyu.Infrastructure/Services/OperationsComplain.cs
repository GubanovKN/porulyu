using Microsoft.EntityFrameworkCore;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsComplain
    {
        public async Task<List<Domain.Models.Complain>> GetComplains()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return await context.Complains.ToListAsync();
            }
        }
        public async Task Create(long chatId, string Type, string Link)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                User user = await new OperationsUser().GetUser(chatId);
                await context.Complains.AddAsync(new Domain.Models.Complain { DateCreate = DateTime.Now, UserId = user.Id, Type = Type, Link = Link });
                await context.SaveChangesAsync();
            }
        }
    }
}
