using porulyu.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotMain.Common
{
    public static class Constants
    {
        public static string PathBots = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Settings\Bots.conf";

        public static string PathDataRegionsAndCities = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Data\RegionsAndCities.conf";
        public static string PathDataMarksAndModels = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Data\MarksAndModels.conf";

        public static string PathCheckCarAuth = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Settings\CheckCarAuth.conf";

        public static List<Region> Regions = new List<Region>();
        public static List<Mark> Marks = new List<Mark>();

        public static Domain.Models.Bot BotMain = new Domain.Models.Bot();
        public static Domain.Models.Bot BotSender = new Domain.Models.Bot();

        public static string CheckCarUserName = "";
        public static string CheckCarPassword = "";
    }
}
