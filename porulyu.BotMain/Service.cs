using NLog;
using porulyu.Bot.Services;
using porulyu.BotMain.Common;
using porulyu.BotMain.Services;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Data;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotMain
{
    partial class Service : ServiceBase
    {
        private readonly Logger logger;

        System.Timers.Timer TimerPayments;

        public Service()
        {
            try
            {
                logger = LogManager.GetCurrentClassLogger();

                InitializeComponent();
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }

        protected async override void OnStart(string[] args)
        {
            try
            {
                OperationsData operationsData = new OperationsData();
                operationsData.LoadRegions();
                operationsData.LoadMarks();
                OperationsBot operationsBot = new OperationsBot();
                operationsBot.Load();
                OperationsCheckCar operationsCheckCar = new OperationsCheckCar();
                operationsCheckCar.Load();
                OperationsUnitpay operationsUnitpay = new OperationsUnitpay();
                operationsUnitpay.Load();

                await operationsBot.Start();

                TimerPayments = new System.Timers.Timer(10000);
                TimerPayments.Elapsed += Timer_Elapsed;
                TimerPayments.Start();
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }

        protected override void OnStop()
        {
            // TODO: Добавьте код, выполняющий подготовку к остановке службы.
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
                                        case "rate":
                                            await new OperationsUser().SetRateIdUser(users[i], await new OperationsRate().Get(payments[j].ActionId.ToString()));
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
                        catch(Exception Ex)
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
    }
}
