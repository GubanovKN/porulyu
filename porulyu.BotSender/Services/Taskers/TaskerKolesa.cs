using NLog;
using porulyu.BotSender.Common;
using porulyu.BotSender.Services.Main;
using porulyu.BotSender.Services.Sities;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Data;
using porulyu.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotSender.Services.Taskers
{
    public class TaskerKolesa
    {
        private readonly Logger logger;

        public readonly Filter Filter;
        private readonly long ChatId;

        private Region Region;
        private City City;
        private Mark Mark;
        private Model Model;

        private System.Timers.Timer Timer;

        private OperationsBot OperationsBot;
        private OperationsUser OperationsUser;
        private OperationsFilter OperationsFilter;
        private OperationsAd OperationsAd;
        private OperationsKolesa OperationsKolesa;

        private bool CanStop;
        public bool Status;
        public bool CanRemove;

        public TaskerKolesa(Filter Filter, long ChatId, Region Region, City City, Mark Mark, Model Model)
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
                OperationsAd = new OperationsAd();
                OperationsKolesa = new OperationsKolesa();

                Ad LastAd = OperationsKolesa.GetLastAd(Filter, ChatId, Region, City, Mark, Model);

                if (OperationsAd.GetByFilter(Filter, "Kolesa").FirstOrDefault(p => p.SiteId == LastAd.SiteId) == null)
                {
                    await OperationsBot.SendNewAd(LastAd, ChatId);

                    await OperationsAd.Create(LastAd, Filter);
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
                if (await OperationsFilter.CheckFilter(Filter) && new OperationsKolesa().GetCountAds(Filter, Region, City, Mark, Model) != 0)
                {
                    CanStop = false;

                    Timer.Stop();

                    await OperationsKolesa.GetNewAds(Filter, OperationsAd.GetByFilter(Filter, "Kolesa"), ChatId, Region, City, Mark, Model);

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
