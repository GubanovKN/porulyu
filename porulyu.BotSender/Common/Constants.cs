using porulyu.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotSender.Common
{
    public static class Constants
    {
        public static string PathBots = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Settings\Bots.conf";
        public static string PathConfig = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Settings\Config.conf";

        public static string PathDataRegionsAndCities = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Data\RegionsAndCities.conf";
        public static string PathDataMarksAndModels = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Data\MarksAndModels.conf";

        public static string KolesaBaseURL = "https://kolesa.kz/cars/";
        public static string KolesaCountSearchURL = "https://m.kolesa.kz/a/ajax-get-search-nb-results/cars/";
        public static string OLXBaseURL = "https://m.olx.kz/api/v1/offers/?offset=0&limit=50&";
        public static string OLXCountSearchURL = "https://m.kolesa.kz/a/ajax-get-search-nb-results/cars/";

        public static int TimeoutLinks = 0;
        public static int TimeoutPhones = 0;
        public static int TimeoutImage = 0;
        public static int TimeoutBeforeSend = 0;
        public static int TimeoutUpdate = 0;

        public static List<Region> Regions = new List<Region>();
        public static List<Mark> Marks = new List<Mark>();

        public static List<Region> OLXRegions = new List<Region>();
        public static List<Mark> OLXMarks = new List<Mark>();

        public static Domain.Models.Bot BotMain = new Domain.Models.Bot();
        public static Domain.Models.Bot BotSender = new Domain.Models.Bot();
    }
}
