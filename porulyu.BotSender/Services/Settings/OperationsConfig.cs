using porulyu.BotSender.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace porulyu.BotSender.Services.Settings
{
    public class OperationsConfig
    {
        public void Load()
        {
            try
            {
                XDocument doc = XDocument.Load(Constants.PathConfig);

                Constants.TimeoutUpdate = Convert.ToInt32(doc.Element("Config").Element("TimeoutUpdate").Value);
                Constants.TimeoutLinks = Convert.ToInt32(doc.Element("Config").Element("TimeoutLinks").Value);
                Constants.TimeoutPhones = Convert.ToInt32(doc.Element("Config").Element("TimeoutPhones").Value);
                Constants.TimeoutImage = Convert.ToInt32(doc.Element("Config").Element("TimeoutImage").Value);
                Constants.TimeoutBeforeSend = Convert.ToInt32(doc.Element("Config").Element("TimeoutBeforeSend").Value);
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
    }
}
