﻿using NLog;
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
        private System.Timers.Timer Timer;

        private Ad LastAd;

        private OperationsBot OperationsBot;
        private OperationsUser OperationsUser;
        private OperationsFilter OperationsFilter;
        private OperationsKolesa OperationsKolesa;

        private bool CanStop;
        public bool Status;
        public bool CanRemove;

        public TaskerKolesa(Filter Filter, long ChatId)
        {
            this.Filter = Filter;
            this.ChatId = ChatId;
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
                OperationsKolesa = new OperationsKolesa();

                LastAd = OperationsKolesa.GetLastAd(Filter, ChatId);

                if (LastAd != null && LastAd.Id != Filter.LastIdAdKolesa)
                {
                    await OperationsBot.SendNewAd(LastAd, ChatId, "Kolesa");

                    Filter.LastIdAdKolesa = LastAd.Id;

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
                if (await OperationsFilter.CheckFilter(Filter))
                {
                    CanStop = false;

                    Timer.Stop();

                    if (LastAd != null)
                    {
                        List<Ad> NewAds = OperationsKolesa.GetNewAds(Filter, LastAd, ChatId);

                        if (NewAds.Count > 0)
                        {
                            if (Directory.Exists($@"Temp\{ChatId}\Kolesa\{LastAd.Id}"))
                            {
                                Directory.Delete($@"Temp\{ChatId}\Kolesa\{LastAd.Id}", true);
                            }

                            LastAd = NewAds[0];

                            Filter.LastIdAdKolesa = LastAd.Id;

                            await OperationsFilter.SaveLastIdAdFilter(Filter, LastAd.Id);

                            for (int i = NewAds.Count - 1; -1 < i; i--)
                            {
                                await OperationsBot.SendNewAd(NewAds[i], ChatId, "Kolesa");
                            }
                        }
                    }
                    else
                    {
                        LastAd = OperationsKolesa.GetLastAd(Filter, ChatId);

                        if (LastAd != null && LastAd.Id != Filter.LastIdAdKolesa)
                        {
                            await OperationsBot.SendNewAd(LastAd, ChatId, "Kolesa");

                            Filter.LastIdAdKolesa = LastAd.Id;

                            await OperationsFilter.SaveLastIdAdFilter(Filter, LastAd.Id);
                        }
                    }

                    Timer.Start();

                    CanStop = true;
                }
                else
                {
                    Stop();
                    CanRemove = true;
                }
            }
            catch (Exception Ex)
            {
                if (Ex.Message == "Forbidden: bot was blocked by the user")
                {
                    await OperationsUser.Activate(ChatId, false);
                    Stop();
                }
                else
                {
                    logger.Error($"Ошибка пользователя с ID {ChatId}");
                    logger.Error(Ex.ToString());

                    Timer.Start();
                }

                CanStop = true;
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