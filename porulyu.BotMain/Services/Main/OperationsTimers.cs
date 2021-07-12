using NLog;
using porulyu.BotMain.Common;
using porulyu.BotMain.Services.Main;
using porulyu.BotMain.Services.Payments;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotMain.Services.Main
{
    public class OperationsTimers
    {
        private readonly Logger logger;

        System.Timers.Timer TimerPayments;
        System.Timers.Timer TimerRates;

        public OperationsTimers()
        {
            logger = LogManager.GetCurrentClassLogger();

            TimerPayments = new System.Timers.Timer(10000);
            TimerPayments.Elapsed += TimerPayments_Elapsed;

            TimerRates = new System.Timers.Timer(10000);
            TimerRates.Elapsed += TimerRates_Elapsed;
        }

        public void Start()
        {
            TimerPayments.Start();
            TimerRates.Start();
        }

        private async void TimerPayments_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                TimerPayments.Stop();

                List<User> users = (await new OperationsUser().GetUsers()).FindAll(p => p.Payments.Count() != 0);

                for (int i = 0; i < users.Count; i++)
                {
                    List<Payment> payments = users[i].Payments.ToList().FindAll(p => p.Status == 0);

                    for (int j = 0; j < payments.Count; j++)
                    {
                        try
                        {
                            int Status = new OperationsUnitpay().CheckPay(payments[j]);

                            switch (Status)
                            {
                                case 1:
                                    switch (payments[j].Action)
                                    {
                                        case "report":
                                            Report report = users[i].Reports.FirstOrDefault(p => p.Id == payments[j].ActionId);
                                            if (report != null)
                                            {
                                                if (new OperationsCheckCar().PayReport(new OperationsCheckCar().Auth(report.Link), Constants.CheckCarUserName, report.Link))
                                                {
                                                    await new OperationsReport().SetReportPay(true, report);
                                                }
                                            }
                                            break;
                                        case "rateSet":
                                            await new OperationsUser().SetRateIdUser(users[i], await new OperationsRate().Get(payments[j].ActionId.ToString()));
                                            break;
                                        case "rateExtend":
                                            await new OperationsUser().ExtendRateIdUser(users[i], await new OperationsRate().Get(payments[j].ActionId.ToString()));
                                            break;
                                    }
                                    await new OperationsPayment().SetStatus(payments[j], 1);
                                    break;
                                case 0:
                                    if (DateTime.Now > payments[j].DateCreate.AddDays(1))
                                    {
                                        await new OperationsPayment().Delete(payments[j]);
                                    }
                                    break;
                                case -1:
                                    if (DateTime.Now > payments[j].DateCreate.AddMonths(1))
                                    {
                                        await new OperationsPayment().Delete(payments[j]);
                                    }
                                    break;
                                case -2:
                                    await new OperationsPayment().Delete(payments[j]);
                                    break;
                            }
                        }
                        catch (Exception Ex)
                        {
                            logger.Error(Ex.Message);
                        }
                    }
                }

                TimerPayments.Start();
            }
            catch (Exception Ex)
            {
                TimerPayments.Start();
                logger.Error(Ex.Message);
            }
        }
        private async void TimerRates_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                TimerRates.Stop();

                List<User> users = (await new OperationsUser().GetUsers()).FindAll(p => p.Rate.Id != 1);

                for (int i = 0; i < users.Count; i++)
                {
                    if (DateTime.Now > users[i].DateExpired)
                    {
                        await new OperationsUser().SetRateIdUser(users[i], await new OperationsRate().Get("1"));
                        await new OperationsUser().SetCountReportsUser(0, users[i]);

                        foreach (var filter in users[i].Filters)
                        {
                            await new OperationsFilter().DeleteFilter(filter);
                        }
                    }
                }

                TimerRates.Start();
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
    }
}
