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

        public static string PathCheckCar = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Settings\CheckCar.conf";

        public static string PathUnitpay = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Settings\Unitpay.conf";

        public static string PathOLX = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Settings\OLX.conf";

        public static Domain.Models.Bot BotMain = new Domain.Models.Bot();
        public static Domain.Models.Bot BotSender = new Domain.Models.Bot();

        public static string CountSearchKolesaURL = "https://kolesa.kz/a/ajax-get-search-nb-results/cars/";
        public static string CountSearchOLXURL = "https://m.olx.kz/api/v1/offers/metadata/search/?";
        public static string CountSearchAsterURL = "https://aster.kz/rest/ads/countByFilter";
        public static string CountSearchMyCarURL = "https://cars.mycar.kz/api/publications/?";

        public static string OLXAccessToken = "";

        public static string CheckCarUserName = "";
        public static string CheckCarPassword = "";
        public static double CheckCarPrice = 0;

        public static string UnitpayProjectId = "";
        public static string UnitpaySecretKey = "";
        public static string UnitpayCurrency = "";

        public static string Currency = "";
    }
}
