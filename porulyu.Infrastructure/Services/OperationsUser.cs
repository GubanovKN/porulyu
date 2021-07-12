using Microsoft.EntityFrameworkCore;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsUser
    {
        public async Task<List<Domain.Models.User>> GetUsers()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return await context.Users.Include(p => p.Filters).Include(p => p.Rate).Include(p => p.Reports).Include(p => p.Payments).ToListAsync();
            }
        }
        public async Task<Domain.Models.User> GetUser(long ChatId)
        {
            Domain.Models.User user;

            using (ApplicationContext context = new ApplicationContext())
            {
                user = await context.Users.Include(p => p.Filters).Include(p => p.Rate).Include(p => p.Reports).Include(p => p.Payments).FirstOrDefaultAsync(p => p.ChatId == ChatId);
            }

            return user;
        }
        public async Task CreateUser(long chatId)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                Rate rate = await new OperationsRate().Get("1");
                await context.Users.AddAsync(new Domain.Models.User { DateCreate = DateTime.Now, ChatId = chatId, RateId = 1, DateExpired = DateTime.Now.AddDays(rate.CountDays), CountReports = rate.CountReports });
                await context.SaveChangesAsync();
            }
        }
        public async Task DeleteUser(Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task SetUserAgreementUser(Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.UserAgreement = true;

                context.Update(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task SetRateIdUser(Domain.Models.User user, Rate rate)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.Rate = rate;
                user.DateExpired = DateTime.Now.AddDays(rate.CountDays);
                user.CountReports = rate.CountReports;

                context.Update(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task ExtendRateIdUser(Domain.Models.User user, Rate rate)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.Rate = rate;
                user.DateExpired = user.DateExpired.AddDays(rate.CountDays);
                user.CountReports += rate.CountReports;

                context.Update(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task SetCountReportsUser(int Count, Domain.Models.User user)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.CountReports = Count;

                context.Update(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task SetPositionViewUser(Domain.Models.User user, int Position)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.PositionView = Position;

                context.Update(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task SetPositionDialogUser(Domain.Models.User user, int Position)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.PositionDialog = Position;

                context.Update(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task SetLastMessageIdUser(Domain.Models.User user, int Id)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                user.LastMessageId = Id;

                context.Update(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task Activate(long ChatId, bool Activate)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                Domain.Models.User user = await GetUser(ChatId);
                user.Activate = Activate;

                context.Update(user);
                await context.SaveChangesAsync();
            }
        }
    }
}
