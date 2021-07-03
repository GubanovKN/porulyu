using porulyu.Domain.Models;
using porulyu.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.Infrastructure.Services
{
    public class OperationsPayment
    {
        public async Task<Payment> Create(Payment payment)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                payment = (await context.Payments.AddAsync(payment)).Entity;
                await context.SaveChangesAsync();
            }

            return payment;
        }
        public async Task SetStatus(Payment payment, int status)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                payment.Status = status;

                context.Update(payment);
                await context.SaveChangesAsync();
            }
        }
        public async Task Delete(Payment payment)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                context.Payments.Remove(payment);
                await context.SaveChangesAsync();
            }
        }
    }
}
