using porulyu.BotMain.Common;
using porulyu.Infrastructure.Cryptography;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace porulyu.BotMain.Services
{
    public class OperationsCheckCar
    {
        public string GetLinkReport(string Data)
        {
            var client = new RestClient($"https://vinkod.kz/t.php?d={Data}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            return $"https://vinkod.kz/{response.Content.Remove(0, response.Content.IndexOf('/') + 1)}";
        }

        public string GetFreeReport(string Link)
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

            return Result;
        }
        public List<RestResponseCookie> Auth(string UserName, string Password)
        {
            HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

            var client = new RestClient("https://vinkod.kz/login");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            html.LoadHtml(response.Content);

            var hervam = html.DocumentNode.Descendants("input").Where(p => p.GetAttributeValue("name", "") == "hervam").FirstOrDefault();

            if (hervam != null)
            {
                client = new RestClient("https://vinkod.kz/login");
                client.Timeout = -1;
                request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("login", "1");
                request.AddParameter("hervam", hervam.GetAttributeValue("value", ""));
                request.AddParameter("email", Constants.CheckCarUserName);
                request.AddParameter("pass", new Encryption().DecryptRSA(Constants.CheckCarPassword));
                response = client.Execute(request);

                return response.Cookies.ToList();
            }
            else
            {
                return null;
            }
        }
        public bool PayReport(List<RestResponseCookie> Cookies, string UserName, string Link)
        {
            string Id = Link.Remove(0, Link.IndexOf("gci") + 4);
            Id = Id.Remove(Id.IndexOf('\''), Id.Length - Id.IndexOf('\''));

            var client = new RestClient($"https://vinkod.kz/get_pack.php?e={UserName}&i={Id}&u=2302");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
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
                            if (data.Descendants("p").FirstOrDefault() != null)
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
                XDocument doc = XDocument.Load(Constants.PathCheckCarAuth);

                Constants.CheckCarUserName = doc.Element("Settings").Element("UserName").Value;
                Constants.CheckCarPassword = doc.Element("Settings").Element("Password").Value;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
    }
}
