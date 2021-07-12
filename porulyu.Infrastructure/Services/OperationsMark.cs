using Microsoft.EntityFrameworkCore;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsMark
    {
        public async Task<List<Domain.Models.Mark>> GetAll()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return await context.Marks.ToListAsync();
            }
        }
        public async Task<Domain.Models.Mark> Get(long MarkId)
        {
            Domain.Models.Mark mark;

            using (ApplicationContext context = new ApplicationContext())
            {
                mark = await context.Marks.Include(p => p.Models).FirstOrDefaultAsync(p => p.Id == MarkId);
            }

            return mark;
        }
    }
}
