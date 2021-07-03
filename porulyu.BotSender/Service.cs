using NLog;
using porulyu.BotSender.Services;
using porulyu.BotSender.Services.Main;
using porulyu.BotSender.Services.Settings;
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
                OperationsTimers operationsTimers = new OperationsTimers();

                await operationsBot.Start();
                operationsTimers.Start();
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
    }
}
