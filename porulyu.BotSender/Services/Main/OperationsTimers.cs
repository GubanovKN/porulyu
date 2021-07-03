using NLog;
using porulyu.BotSender.Services.Taskers;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotSender.Services.Main
{
    public class OperationsTimers
    {
        private readonly Logger logger;

        private List<TaskerKolesa> TaskersKolesa;
        private List<TaskerOLX> TaskersOLX;
        private List<TaskerMyCar> TaskersMyCar;
        private List<TaskerAster> TaskersAster;

        System.Timers.Timer KrishaTimerFilters;
        System.Timers.Timer OLXTimerFilters;
        System.Timers.Timer MyCarTimerFilters;
        System.Timers.Timer AsterTimerFilters;

        public OperationsTimers()
        {
            logger = LogManager.GetCurrentClassLogger();

            TaskersKolesa = new List<TaskerKolesa>();
            TaskersOLX = new List<TaskerOLX>();
            TaskersMyCar = new List<TaskerMyCar>();
            TaskersAster = new List<TaskerAster>();

            KrishaTimerFilters = new System.Timers.Timer(10000);
            KrishaTimerFilters.Elapsed += KrishaTimer_Elapsed;

            OLXTimerFilters = new System.Timers.Timer(10000);
            OLXTimerFilters.Elapsed += OLXTimer_Elapsed;

            //MyCarTimerFilters = new System.Timers.Timer(10000);
            //MyCarTimerFilters.Elapsed += OLXTimer_Elapsed;

            //AsterTimerFilters = new System.Timers.Timer(10000);
            //AsterTimerFilters.Elapsed += OLXTimer_Elapsed;
        }

        public void Start()
        {
            KrishaTimerFilters.Start();
            OLXTimerFilters.Start();
            //MyCarTimerFilters.Start();
            //AsterTimerFilters.Start();
        }

        private async void KrishaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                KrishaTimerFilters.Stop();

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
                                    TaskerKolesa temp = TaskersKolesa.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp == null)
                                    {
                                        temp = new TaskerKolesa(filter, users[i].ChatId);
                                        await temp.Start();
                                        TaskersKolesa.Add(temp);
                                    }
                                    else if (!temp.Status)
                                    {
                                        await temp.Start();
                                    }
                                }
                                else
                                {
                                    TaskerKolesa temp = TaskersKolesa.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp != null)
                                    {
                                        temp.Stop();
                                        TaskersKolesa.Remove(temp);
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

                var RemoveTaskers = TaskersKolesa.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    TaskersKolesa.Remove(task);
                }

                KrishaTimerFilters.Start();
            }
            catch (Exception Ex)
            {
                KrishaTimerFilters.Start();
                logger.Error(Ex.Message);
            }
        }
        private async void OLXTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                KrishaTimerFilters.Stop();

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
                                    TaskerOLX temp = TaskersOLX.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp == null)
                                    {
                                        temp = new TaskerOLX(filter, users[i].ChatId);
                                        await temp.Start();
                                        TaskersOLX.Add(temp);
                                    }
                                    else if (!temp.Status)
                                    {
                                        await temp.Start();
                                    }
                                }
                                else
                                {
                                    TaskerOLX temp = TaskersOLX.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp != null)
                                    {
                                        temp.Stop();
                                        TaskersOLX.Remove(temp);
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

                var RemoveTaskers = TaskersOLX.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    TaskersOLX.Remove(task);
                }

                KrishaTimerFilters.Start();
            }
            catch (Exception Ex)
            {
                KrishaTimerFilters.Start();
                logger.Error(Ex.Message);
            }
        }
        private async void MyCarTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                KrishaTimerFilters.Stop();

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
                                    TaskerMyCar temp = TaskersMyCar.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp == null)
                                    {
                                        temp = new TaskerMyCar(filter, users[i].ChatId);
                                        await temp.Start();
                                        TaskersMyCar.Add(temp);
                                    }
                                    else if (!temp.Status)
                                    {
                                        await temp.Start();
                                    }
                                }
                                else
                                {
                                    TaskerMyCar temp = TaskersMyCar.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp != null)
                                    {
                                        temp.Stop();
                                        TaskersMyCar.Remove(temp);
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

                var RemoveTaskers = TaskersMyCar.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    TaskersMyCar.Remove(task);
                }

                KrishaTimerFilters.Start();
            }
            catch (Exception Ex)
            {
                KrishaTimerFilters.Start();
                logger.Error(Ex.Message);
            }
        }
        private async void AsterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                KrishaTimerFilters.Stop();

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
                                    TaskerAster temp = TaskersAster.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp == null)
                                    {
                                        temp = new TaskerAster(filter, users[i].ChatId);
                                        await temp.Start();
                                        TaskersAster.Add(temp);
                                    }
                                    else if (!temp.Status)
                                    {
                                        await temp.Start();
                                    }
                                }
                                else
                                {
                                    TaskerAster temp = TaskersAster.FirstOrDefault(p => p.Filter.Id == filter.Id);

                                    if (temp != null)
                                    {
                                        temp.Stop();
                                        TaskersAster.Remove(temp);
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

                var RemoveTaskers = TaskersAster.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    TaskersAster.Remove(task);
                }

                KrishaTimerFilters.Start();
            }
            catch (Exception Ex)
            {
                KrishaTimerFilters.Start();
                logger.Error(Ex.Message);
            }
        }
    }
}
