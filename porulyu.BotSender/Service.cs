using NLog;
using porulyu.BotSender.Services;
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

namespace porulyu.BotSender
{
    partial class Service : ServiceBase
    {
        private readonly Logger logger;

        private List<Tasker> Taskers;

        System.Timers.Timer TimerFilters;

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
                OperationsConfig operationsConfig = new OperationsConfig();
                operationsConfig.Load();
                OperationsData operationsData = new OperationsData();
                operationsData.LoadRegions();
                operationsData.LoadMarks();
                OperationsBot operationsBot = new OperationsBot();
                operationsBot.Load();

                await operationsBot.Start();

                Taskers = new List<Tasker>();

                TimerFilters = new System.Timers.Timer(5000);
                TimerFilters.Elapsed += Timer_Elapsed;
                TimerFilters.Start();
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
                TimerFilters.Stop();

                List<User> users = await new OperationsUser().GetUsers();

                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].Activate)
                    {
                        if (users[i].DateExpired > DateTime.Now)
                        {
                            foreach (var filter in users[i].Filters)
                            {
                                if (filter.Work)
                                {
                                    Tasker temp = Taskers.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp == null)
                                    {
                                        temp = new Tasker(filter, users[i].ChatId);
                                        await temp.Start();
                                        Taskers.Add(temp);
                                    }
                                    else if (!temp.Status)
                                    {
                                        await temp.Start();
                                    }
                                }
                                else
                                {
                                    Tasker temp = Taskers.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp != null)
                                    {
                                        temp.Stop();
                                        Taskers.Remove(temp);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (users[i].RateId != 1)
                        {
                            users[i].RateId = 1;

                            if (users[i].Filters.Count > users[i].Rate.CountFilters)
                            {
                                for (int j = users[i].Filters.Count; j > users[i].Rate.CountFilters; j--)
                                {
                                    await new OperationsFilter().DeleteFilter(users[i].Filters.ToList()[j]);
                                }
                            }
                        }
                    }
                }

                var RemoveTaskers = Taskers.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    Taskers.Remove(task);
                }

                TimerFilters.Start();
            }
            catch (Exception Ex)
            {
                TimerFilters.Start();
                logger.Error(Ex.Message);
            }
        }
    }
}
