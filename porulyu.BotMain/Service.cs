using NLog;
using porulyu.BotMain.Services;
using porulyu.Infrastructure.Data;
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
                OperationsData operationsData = new OperationsData();
                operationsData.LoadRegions();
                operationsData.LoadMarks();
                OperationsBot operationsBot = new OperationsBot();
                operationsBot.Load();

                await operationsBot.Start();
            }
            catch(Exception Ex)
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
