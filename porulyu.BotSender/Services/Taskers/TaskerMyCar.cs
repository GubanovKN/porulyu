using NLog;
using porulyu.BotSender.Common;
using porulyu.BotSender.Services.Main;
using porulyu.BotSender.Services.Sities;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotSender.Services.Taskers
{
    public class TaskerMyCar
    {
        private readonly Logger logger;

        public readonly Filter Filter;
        private readonly long ChatId;

        private Region Region;
        private City City;
        private Mark Mark;
        private Model Model;

        private System.Timers.Timer Timer;

        private Ad LastAd;

        private OperationsBot OperationsBot;
        private OperationsUser OperationsUser;
        private OperationsFilter OperationsFilter;
        private OperationsMyCar OperationsMyCar;

        private bool CanStop;
        public bool Status;
        public bool CanRemove;

        public TaskerMyCar(Filter Filter, long ChatId, Region Region, City City, Mark Mark, Model Model)
        {
            this.Filter = Filter;
            this.ChatId = ChatId;
            this.Region = Region;
            this.City = City;
            this.Mark = Mark;
            this.Model = Model;
            logger = LogManager.GetCurrentClassLogger();
        }

        public async Task Start()
        {
            try
            {
                Status = true;

                OperationsBot = new OperationsBot();
                OperationsUser = new OperationsUser();
                OperationsFilter = new OperationsFilter();
                OperationsMyCar = new OperationsMyCar();

                LastAd = OperationsMyCar.GetLastAd(Filter, ChatId, Region, City, Mark, Model);

                if (LastAd != null && LastAd.Id != Filter.LastIdAdMyCar)
                {
                    await OperationsBot.SendNewAd(LastAd, ChatId, "MyCar");

                    Filter.LastIdAdMyCar = LastAd.Id;

                    await OperationsFilter.SaveLastIdAdFilter(Filter, LastAd.Id);
                }

                Timer = new System.Timers.Timer(Constants.TimeoutUpdate);
                Timer.Elapsed += Timer_Elapsed;
                Timer.Start();

                CanStop = true;
            }
            catch (Exception Ex)
            {
                if (Ex.Message == "Forbidden: bot was blocked by the user")
                {
                    await OperationsUser.Activate(ChatId, false);
                }
                else
                {
                    logger.Error($"Ошибка пользователя с ID {ChatId}");
                    logger.Error(Ex.ToString());
                }
                Status = false;
            }
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (await OperationsFilter.CheckFilter(Filter) && new OperationsMyCar().GetCountAds(Filter, Region, City, Mark, Model) != 0)
                {
                    CanStop = false;

                    Timer.Stop();

                    if (LastAd != null)
                    {
                        List<Ad> NewAds = OperationsMyCar.GetNewAds(Filter, LastAd, ChatId, Region, City, Mark, Model);

                        if (NewAds.Count > 0)
                        {
                            if (Directory.Exists($@"Temp\{ChatId}\MyCar\{LastAd.Id}"))
                            {
                                Directory.Delete($@"Temp\{ChatId}\MyCar\{LastAd.Id}", true);
                            }

                            LastAd = NewAds[0];

                            Filter.LastIdAdMyCar = LastAd.Id;

                            await OperationsFilter.SaveLastIdAdFilter(Filter, LastAd.Id);

                            for (int i = NewAds.Count - 1; -1 < i; i--)
                            {
                                await OperationsBot.SendNewAd(NewAds[i], ChatId, "MyCar");
                            }
                        }
                    }
                    else
                    {
                        LastAd = OperationsMyCar.GetLastAd(Filter, ChatId, Region, City, Mark, Model);

                        if (LastAd != null && LastAd.Id != Filter.LastIdAdMyCar)
                        {
                            await OperationsBot.SendNewAd(LastAd, ChatId, "MyCar");

                            Filter.LastIdAdMyCar = LastAd.Id;

                            await OperationsFilter.SaveLastIdAdFilter(Filter, LastAd.Id);
                        }
                    }

                    Timer.Start();

                    CanStop = true;
                }
                else
                {
                    CanStop = true;
                    CanRemove = true;
                    Stop();
                }
            }
            catch (Exception Ex)
            {
                CanStop = true;

                if (Ex.Message == "Forbidden: bot was blocked by the user")
                {
                    await OperationsUser.Activate(ChatId, false);
                    CanRemove = true;
                    Stop();
                }
                else
                {
                    logger.Error($"Ошибка пользователя с ID {ChatId}");
                    logger.Error(Ex.ToString());

                    Timer.Start();
                }
            }
        }

        public void Stop()
        {
            while (!CanStop)
            {

            }

            Timer.Stop();

            Status = false;
        }
    }
}
