using porulyu.BotMain.Common;
using porulyu.Infrastructure.Cryptography;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace porulyu.BotMain.Services.Main
{
    public class OperationsCheckCar
    {
        public string GetLinkReport(string Data)
        {
            var client = new RestClient($"https://vinkod.kz/t.php?d={Data}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (response.Content.IndexOf("Ошибка!") == -1)
                {
                    return $"https://vinkod.kz/{response.Content.Remove(0, response.Content.IndexOf('/') + 1)}";
                }
                else
                {
                    throw new Exception("Автомобиль не найден");
                }
            }
            else
            {
                throw new Exception("Сервис не доступен");
            }
        }

        public string GetFreeReport(string Link)
        {
            HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

            var client = new RestClient(Link);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            string Result = "";

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                html.LoadHtml(response.Content);

                if (html.DocumentNode.Descendants("h3").Where(p => p.ChildNodes.Where(x => x.Id == "marka").Count() != 0).FirstOrDefault() != null)
                {
                    Result = "<b>" + html.DocumentNode.Descendants("h3").Where(p => p.ChildNodes.Where(x => x.Id == "marka").Count() != 0).FirstOrDefault().InnerText + "</b>\n\r├";

                    if (html.DocumentNode.Descendants("address").FirstOrDefault() != null)
                    {
                        Result += html.DocumentNode.Descendants("address").First().InnerText.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                    }

                    string[] dataAdd = new string[] { "Гос-Номер", "Регион регистрации", "Цвет", "Тип транспортного средства", "Возраст автомобиля", "Объем двигателя", "Налог на транспорт", "Мощность двигателя", "Страна сборки" };

                    for (int i = 0; i < dataAdd.Length; i++)
                    {
                        if (Result.IndexOf(dataAdd[i]) != -1)
                        {
                            Result = Result.Insert(Result.IndexOf(dataAdd[i]), "\n\r├");
                        }
                    }

                    string[] dataRemove = new string[] { "Масса без груза", "Максимальная разрешенная масса", "Кол-во посадочных мест", "Завод изготовитель", "Владелец марки", "История" };

                    for (int i = 0; i < dataRemove.Length; i++)
                    {
                        if (i != dataRemove.Length - 1)
                        {
                            if (Result.IndexOf(dataRemove[i]) != -1)
                            {
                                Result = Result.Remove(Result.IndexOf(dataRemove[i]), Result.IndexOf("\n\r", Result.IndexOf(dataRemove[i])) - Result.IndexOf(dataRemove[i]));
                            }
                        }
                        else
                        {
                            if (Result.IndexOf(dataRemove[i]) != -1)
                            {
                                Result = Result.Remove(Result.IndexOf(dataRemove[i]), Result.Length - Result.IndexOf(dataRemove[i]));
                            }
                        }
                    }

                    bool Find = true;

                    while (Find)
                    {
                        if (Result.IndexOf("не соответствует") != -1)
                        {
                            Result = Result.Remove(Result.IndexOf("не соответствует"), Result.IndexOf("\n\r", Result.IndexOf("не соответствует")) - Result.IndexOf("не соответствует"));
                        }
                        else
                        {
                            Find = false;
                        }
                    }

                    Result += "\n\r\n\r<b>Дополнительная информация</b>";

                    string[] dataOptions = new string[] { "p2", "p3", "p85", "p5", "p6", "p4" };

                    for (int i = 0; i < dataOptions.Length; i++)
                    {
                        var Options = html.DocumentNode.Descendants("div").Where(p => p.GetAttributeValue("id", "") == dataOptions[i]).FirstOrDefault();

                        if (Options != null)
                        {
                            var header = Options.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "card-header").FirstOrDefault();
                            var data = Options.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "card-body").FirstOrDefault();

                            if (header != null && data != null)
                            {
                                Result += $"\n\r{header.InnerText.Replace("\n", "").Replace("\t", "").Replace("\r", "")}: {data.Descendants("h5").First().InnerText.Replace("\n", "").Replace("\t", "").Replace("\r", "")}";
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Автомобиль не найден");
                }
            }
            else
            {
                throw new Exception("Сервис не доступен");
            }

            return Result;
        }
        public List<RestResponseCookie> Auth(string Link)
        {
            HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

            var client = new RestClient(Link);
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.216 YaBrowser/21.5.4.607 Yowser/2.5 Safari/537.36";
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            html.LoadHtml(response.Content);

            var hervam = html.DocumentNode.Descendants("input").Where(p => p.GetAttributeValue("name", "") == "hervam").FirstOrDefault();

            if (hervam != null)
            {
                var LoginClient = new RestClient("https://vinkod.kz/login");
                LoginClient.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.216 YaBrowser/21.5.4.607 Yowser/2.5 Safari/537.36";
                LoginClient.Timeout = -1;
                LoginClient.FollowRedirects = false;
                var LoginRequest = new RestRequest(Method.POST);
                LoginRequest.AddHeader("Referer", Link);
                LoginRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                LoginRequest.AddHeader("Origin", "https://vinkod.kz");
                LoginRequest.AddParameter("login", "1");
                LoginRequest.AddParameter("hervam", hervam.GetAttributeValue("value", ""));
                LoginRequest.AddParameter("email", Constants.CheckCarUserName);
                LoginRequest.AddParameter("pass", new Encryption().DecryptRSA(Constants.CheckCarPassword));

                for (int i = 0; i < response.Cookies.Count; i++)
                {
                    LoginRequest.AddCookie(response.Cookies[i].Name, response.Cookies[i].Value);
                }

                IRestResponse LoginResponse = LoginClient.Execute(LoginRequest);

                if (LoginResponse.StatusCode == System.Net.HttpStatusCode.Redirect)
                {
                    return LoginResponse.Cookies.ToList();
                }
                else
                {
                    throw new Exception("Не удается войти в аккаунт vinkod");
                }
            }
            else
            {
                return null;
            }
        }
        public bool PayReport(List<RestResponseCookie> Cookies, string UserName, string Link)
        {
            var Data = GetIdUserAndReport(Link, Cookies);

            var client = new RestClient($"https://vinkod.kz/get_pack.php?e={UserName}&i={Data.Item1}&u={Data.Item2}");
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.216 YaBrowser/21.5.4.607 Yowser/2.5 Safari/537.36";
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);

            for (int i = 0; i < Cookies.Count; i++)
            {
                request.AddCookie(Cookies[i].Name, Cookies[i].Value);
            }

            IRestResponse response = client.Execute(request);

            if (response.Content != "Ваш баланс равен нулю! Обновите страницу.")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private (string, string) GetIdUserAndReport(string Link, List<RestResponseCookie> Cookies)
        {
            HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

            var client = new RestClient(Link);
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.216 YaBrowser/21.5.4.607 Yowser/2.5 Safari/537.36";
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);

            for (int i = 0; i < Cookies.Count; i++)
            {
                request.AddCookie(Cookies[i].Name, Cookies[i].Value);
            }

            IRestResponse response = client.Execute(request);

            html.LoadHtml(response.Content);

            var reportId = html.DocumentNode.Descendants("input").FirstOrDefault(p => p.GetAttributeValue("id", "") == "idd2");
            var userId = html.DocumentNode.Descendants("input").FirstOrDefault(p => p.GetAttributeValue("id", "") == "iddu2");

            if (reportId != null && userId != null)
            {
                return (reportId.GetAttributeValue("value", ""), userId.GetAttributeValue("value", ""));
            }
            else
            {
                throw new Exception("Ошибка получения ID пользователя и отчета");
            }
        }
        public string GetPayReport(string Link)
        {
            HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

            var client = new RestClient(Link);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            html.LoadHtml(response.Content);

            string Result = "";

            if (html.DocumentNode.Descendants("h3").Where(p => p.ChildNodes.Where(x => x.Id == "marka").Count() != 0).FirstOrDefault() != null)
            {
                Result = "<b>" + html.DocumentNode.Descendants("h3").Where(p => p.ChildNodes.Where(x => x.Id == "marka").Count() != 0).FirstOrDefault().InnerText + "</b>\n\r├";

                if (html.DocumentNode.Descendants("address").FirstOrDefault() != null)
                {
                    Result += html.DocumentNode.Descendants("address").First().InnerText.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                }

                string[] dataAdd = new string[] { "Гос-Номер", "Регион регистрации", "Цвет", "Тип транспортного средства", "Возраст автомобиля", "Объем двигателя", "Налог на транспорт", "Мощность двигателя", "Страна сборки" };

                for (int i = 0; i < dataAdd.Length; i++)
                {
                    if (Result.IndexOf(dataAdd[i]) != -1)
                    {
                        Result = Result.Insert(Result.IndexOf(dataAdd[i]), "\n\r├");
                    }
                }

                string[] dataRemove = new string[] { "Масса без груза", "Максимальная разрешенная масса", "Кол-во посадочных мест", "Завод изготовитель", "Владелец марки", "История", "Проверяем в других странах" };

                for (int i = 0; i < dataRemove.Length; i++)
                {
                    if (i != dataRemove.Length - 1)
                    {
                        if (Result.IndexOf(dataRemove[i]) != -1)
                        {
                            Result = Result.Remove(Result.IndexOf(dataRemove[i]), Result.IndexOf("\n\r", Result.IndexOf(dataRemove[i])) - Result.IndexOf(dataRemove[i]));
                        }
                    }
                    else
                    {
                        if (Result.IndexOf(dataRemove[i]) != -1)
                        {
                            Result = Result.Remove(Result.IndexOf(dataRemove[i]), Result.Length - Result.IndexOf(dataRemove[i]));
                        }
                    }
                }

                bool Find = true;

                while (Find)
                {
                    if (Result.IndexOf("не соответствует") != -1)
                    {
                        Result = Result.Remove(Result.IndexOf("не соответствует"), Result.IndexOf("\n\r", Result.IndexOf("не соответствует")) - Result.IndexOf("не соответствует"));
                    }
                    else
                    {
                        Find = false;
                    }
                }

                Result += "\n\r\n\r<b>Дополнительная информация</b>";

                string[] dataOptions = new string[] { "p2", "p3", "p85", "p5", "p6", "p4" };

                for (int i = 0; i < dataOptions.Length; i++)
                {
                    var Options = html.DocumentNode.Descendants("div").Where(p => p.GetAttributeValue("id", "") == dataOptions[i]).FirstOrDefault();

                    if (Options != null)
                    {
                        var header = Options.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "card-header").FirstOrDefault();
                        var data = Options.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "card-body").FirstOrDefault();

                        if (header != null && data != null)
                        {
                            if (data.Descendants("p").FirstOrDefault() != null && i == 0)
                            {
                                Result += $"\n\r{data.Descendants("p").First().InnerText.Replace("\n", "").Replace("\t", "").Replace("\r", "")}";
                            }
                            else
                            {
                                Result += $"\n\r{header.InnerText.Replace("\n", "").Replace("\t", "").Replace("\r", "")}: {data.Descendants("h5").First().InnerText.Replace("\n", "").Replace("\t", "").Replace("\r", "")}";
                            }
                        }
                    }
                }
            }
            else
            {
                Result = "Автомобиль не найден";
            }

            return Result;
        }

        public void Load()
        {
            try
            {
                XDocument doc = XDocument.Load(Constants.PathCheckCar);

                Constants.CheckCarUserName = doc.Element("Settings").Element("UserName").Value;
                Constants.CheckCarPassword = doc.Element("Settings").Element("Password").Value;
                Constants.CheckCarPrice = Convert.ToDouble(doc.Element("Settings").Element("Price").Value);
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
    }
}
