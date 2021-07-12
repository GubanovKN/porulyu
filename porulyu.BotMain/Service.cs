using NLog;
using porulyu.BotMain.Common;
using porulyu.BotMain.Services;
using porulyu.BotMain.Services.Main;
using porulyu.BotMain.Services.Payments;
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
                OperationsBot operationsBot = new OperationsBot();
                operationsBot.Load();
                OperationsCheckCar operationsCheckCar = new OperationsCheckCar();
                operationsCheckCar.Load();
                OperationsUnitpay operationsUnitpay = new OperationsUnitpay();
                operationsUnitpay.Load();
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
