using NLog;
using porulyu.BotSender.Services.Sities;
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

        System.Timers.Timer TimerFilters;

        public OperationsTimers()
        {
            logger = LogManager.GetCurrentClassLogger();

            TaskersKolesa = new List<TaskerKolesa>();
            TaskersOLX = new List<TaskerOLX>();
            TaskersMyCar = new List<TaskerMyCar>();
            TaskersAster = new List<TaskerAster>();

            TimerFilters = new System.Timers.Timer(10000);
            TimerFilters.Elapsed += Timer_Elapsed;
        }

        public void Start()
        {
            TimerFilters.Start();
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
                        foreach (var filter in users[i].Filters)
                        {
                            if (filter.Work)
                            {
                                Region region = null;
                                City city = null;

                                if (filter.RegionId != 0)
                                {
                                    region = await new OperationsRegion().Get(filter.RegionId);

                                    if (filter.CityId != 0)
                                    {
                                        city = region.Cities.FirstOrDefault(p => p.Id == filter.CityId);
                                    }
                                }

                                Mark mark = null;
                                Model model = null;

                                if (filter.MarkId != 0)
                                {
                                    mark = await new OperationsMark().Get(filter.MarkId);

                                    if (filter.ModelId != 0)
                                    {
                                        model = mark.Models.FirstOrDefault(p => p.Id == filter.ModelId);
                                    }
                                }

                                await CreateKolesa(filter, region, city, mark, model, users[i]);
                                await CreateOLX(filter, region, city, mark, model, users[i]);
                                await CreateAster(filter, region, city, mark, model, users[i]);
                                await CreateMyCar(filter, region, city, mark, model, users[i]);
                            }
                        }
                    }
                }
                RemoveKolesa();
                RemoveOLX();
                RemoveAster();
                RemoveMyCar();

                TimerFilters.Start();
            }
            catch (Exception Ex)
            {
                TimerFilters.Start();
                logger.Error(Ex.Message);
            }
        }

        #region Создание задач
        private async Task CreateKolesa(Filter filter, Region region, City city, Mark mark, Model model, User user)
        {
            try
            {
                if ((region == null || region.KolesaId != String.Empty) && (city == null || city.KolesaId != String.Empty) && (mark == null || mark.KolesaId != String.Empty) && (model == null || model.KolesaId != String.Empty))
                {
                    TaskerKolesa temp = TaskersKolesa.FirstOrDefault(p => p.Filter.Id == filter.Id);

                    if (temp == null)
                    {
                        if (new OperationsKolesa().GetCountAds(filter, region, city, mark, model) != 0)
                        {
                            temp = new TaskerKolesa(filter, user.ChatId, region, city, mark, model);
                            await temp.Start();
                            TaskersKolesa.Add(temp);
                        }
                    }
                    else if (!temp.Status && !temp.CanRemove)
                    {
                        await temp.Start();
                    }
                }
            }
            catch(Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        private async Task CreateOLX(Filter filter, Region region, City city, Mark mark, Model model, User user)
        {
            try
            {
                if (filter.CustomsСleared == 0 && (region == null || region.OLXId != String.Empty) && (city == null || city.OLXId != String.Empty) && (mark == null || mark.OLXId != String.Empty) && (model == null || model.OLXId != String.Empty))
                {
                    TaskerOLX temp = TaskersOLX.FirstOrDefault(p => p.Filter.Id == filter.Id);

                    if (temp == null)
                    {
                        if (new OperationsOLX().GetCountAds(filter, region, city, mark, model) != 0)
                        {
                            temp = new TaskerOLX(filter, user.ChatId, region, city, mark, model);
                            await temp.Start();
                            TaskersOLX.Add(temp);
                        }
                    }
                    else if (!temp.Status && !temp.CanRemove)
                    {
                        await temp.Start();
                    }
                }
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        private async Task CreateAster(Filter filter, Region region, City city, Mark mark, Model model, User user)
        {
            try
            {
                if (filter.CustomsСleared == 0 && (region == null || region.AsterId != String.Empty) && (city == null || city.AsterId != String.Empty) && (mark == null || mark.AsterId != String.Empty) && (model == null || model.AsterId != String.Empty))
                {
                    TaskerAster temp = TaskersAster.FirstOrDefault(p => p.Filter.Id == filter.Id);

                    if (temp == null)
                    {
                        if (new OperationsAster().GetCountAds(filter, region, city, mark, model) != 0)
                        {
                            temp = new TaskerAster(filter, user.ChatId, region, city, mark, model);
                            await temp.Start();
                            TaskersAster.Add(temp);
                        }
                    }
                    else if (!temp.Status && !temp.CanRemove)
                    {
                        await temp.Start();
                    }
                }
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        private async Task CreateMyCar(Filter filter, Region region, City city, Mark mark, Model model, User user)
        {
            try
            {
                if (filter.CustomsСleared == 0 && filter.Transmission == 0 && filter.Actuator == 0 && filter.FirstEngineCapacity == 0 && filter.SecondEngineCapacity == 0 && (region == null || region.MyCarId != String.Empty) && (city == null || city.MyCarId != String.Empty) && (mark == null || mark.MyCarId != String.Empty) && (model == null || model.MyCarId != String.Empty))
                {
                    TaskerMyCar temp = TaskersMyCar.FirstOrDefault(p => p.Filter.Id == filter.Id);

                    if (temp == null)
                    {
                        if (new OperationsMyCar().GetCountAds(filter, region, city, mark, model) != 0)
                        {
                            temp = new TaskerMyCar(filter, user.ChatId, region, city, mark, model);
                            await temp.Start();
                            TaskersMyCar.Add(temp);
                        }
                    }
                    else if (!temp.Status && !temp.CanRemove)
                    {
                        await temp.Start();
                    }
                }
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        #endregion

        #region Удаление задач
        private void RemoveKolesa()
        {
            try
            {
                var RemoveTaskers = TaskersKolesa.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    TaskersKolesa.Remove(task);
                }
            }
            catch(Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        private void RemoveOLX()
        {
            try
            {
                var RemoveTaskers = TaskersOLX.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    TaskersOLX.Remove(task);
                }
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        private void RemoveAster()
        {
            try
            {
                var RemoveTaskers = TaskersAster.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    TaskersAster.Remove(task);
                }
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        private void RemoveMyCar()
        {
            try
            {
                var RemoveTaskers = TaskersMyCar.FindAll(p => p.CanRemove);

                foreach (var task in RemoveTaskers)
                {
                    if (task.Status)
                    {
                        task.Stop();
                    }

                    TaskersMyCar.Remove(task);
                }
            }
            catch (Exception Ex)
            {
                logger.Error(Ex.Message);
            }
        }
        #endregion
    }
}
