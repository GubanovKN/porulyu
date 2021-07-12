using Newtonsoft.Json;
using porulyu.BotSender.Common;
using porulyu.BotSender.Services.Main;
using porulyu.Domain.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace porulyu.BotSender.Services.Sities
{
    public class OperationsKolesa
    {
        #region Получение последнего объявления
        public Ad GetLastAd(Filter filter, long ChatId, Region region, City city, Mark mark, Model model)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

                string BaseUrl = CombineBaseURL(filter, region, city, mark, model);

                var client = new RestClient(BaseUrl);

                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    html.LoadHtml(response.Content);

                    int LastPage = 0;

                    var offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                    var ad = offers.Descendants("div").FirstOrDefault(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0);

                    if (ad == null)
                    {
                        Thread.Sleep(Constants.TimeoutLinks);

                        client = new RestClient(BaseUrl + $"&page={LastPage}");

                        client.Timeout = -1;
                        request = new RestRequest(Method.GET);
                        response = client.Execute(request);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            html.LoadHtml(response.Content);

                            offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                            ad = offers.Descendants("div").FirstOrDefault(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0);

                            if (ad != null)
                            {
                                int checkPage = 0;

                                int spliter = 1;
                                if (LastPage > 100)
                                {
                                    spliter = 100;
                                }
                                else if (LastPage > 10)
                                {
                                    spliter = 10;
                                }

                                int untopPage = 0;

                                bool Find = true;
                                while (Find)
                                {
                                    Thread.Sleep(Constants.TimeoutLinks);

                                    client = new RestClient(BaseUrl + $"&page={checkPage}");

                                    client.Timeout = -1;
                                    request = new RestRequest(Method.GET);
                                    response = client.Execute(request);

                                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        html.LoadHtml(response.Content);

                                        offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                                        ad = offers.Descendants("div").FirstOrDefault(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0);

                                        if (ad != null)
                                        {
                                            untopPage = spliter;
                                            checkPage -= spliter;

                                            switch (spliter)
                                            {
                                                case 100:
                                                    spliter = 10;
                                                    break;
                                                case 10:
                                                    spliter = 1;
                                                    break;
                                                case 1:
                                                    Find = false;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            if (checkPage + spliter < LastPage)
                                            {
                                                checkPage += spliter;
                                            }
                                            else
                                            {
                                                checkPage = LastPage;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Thread.Sleep(Constants.TimeoutLinks);

                                client = new RestClient(BaseUrl);

                                client.Timeout = -1;
                                request = new RestRequest(Method.GET);
                                response = client.Execute(request);

                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    html.LoadHtml(response.Content);

                                    offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                                    ad = offers.Descendants("div").FirstOrDefault(p => p.GetAttributeValue("data-id", "") != "");
                                }
                            }
                        }
                    }

                    if (ad != null)
                    {
                        string link = $"https://kolesa.kz/a/show/{ad.GetAttributeValue("data-id", "")}";

                        return GetDataAd(link, ChatId);
                    }

                    return null;
                }
                else
                {
                    throw new Exception("Страница поиска не найдена");
                }
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        #endregion

        #region Получение новых объявлений
        public List<Ad> GetNewAds(Filter filter, Ad lastAd, long ChatId, Region region, City city, Mark mark, Model model)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

                string BaseUrl = CombineBaseURL(filter, region, city, mark, model);

                var client = new RestClient(BaseUrl);

                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    html.LoadHtml(response.Content);

                    int LastPage = 0;

                    var offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                    List<string> ads = offers.Descendants("div").Where(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0).Select(p => p.GetAttributeValue("data-id", "")).ToList();

                    if (ads.Count == 0)
                    {
                        Thread.Sleep(Constants.TimeoutLinks);

                        client = new RestClient(BaseUrl + $"&page={LastPage}");

                        client.Timeout = -1;
                        request = new RestRequest(Method.GET);
                        response = client.Execute(request);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            html.LoadHtml(response.Content);

                            offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                            ads.AddRange(offers.Descendants("div").Where(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0).Select(p => p.GetAttributeValue("data-id", "")).ToList());

                            if (ads.Count != 0)
                            {
                                ads.Clear();

                                int checkPage = 0;

                                int spliter = 1;
                                if (LastPage > 100)
                                {
                                    spliter = 100;
                                }
                                else if (LastPage > 10)
                                {
                                    spliter = 10;
                                }

                                int untopPage = 0;

                                bool Find = true;
                                while (Find)
                                {
                                    Thread.Sleep(Constants.TimeoutLinks);

                                    client = new RestClient(BaseUrl + $"&page={checkPage}");

                                    client.Timeout = -1;
                                    request = new RestRequest(Method.GET);
                                    response = client.Execute(request);

                                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        html.LoadHtml(response.Content);

                                        offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                                        List<string> temp = offers.Descendants("div").Where(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0).Select(p => p.GetAttributeValue("data-id", "")).ToList();

                                        if (temp.Count != 0)
                                        {
                                            untopPage = spliter;
                                            checkPage -= spliter;

                                            switch (spliter)
                                            {
                                                case 100:
                                                    spliter = 10;
                                                    break;
                                                case 10:
                                                    spliter = 1;
                                                    break;
                                                case 1:
                                                    Find = false;
                                                    ads.AddRange(temp);

                                                    Thread.Sleep(Constants.TimeoutLinks);

                                                    client = new RestClient(BaseUrl + $"&page={checkPage}");

                                                    client.Timeout = -1;
                                                    request = new RestRequest(Method.GET);
                                                    response = client.Execute(request);

                                                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                                    {
                                                        html.LoadHtml(response.Content);

                                                        offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                                                        ads.AddRange(offers.Descendants("div").Where(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0).Select(p => p.GetAttributeValue("data-id", "")).ToList());
                                                    }

                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            if (checkPage + spliter < LastPage)
                                            {
                                                checkPage += spliter;
                                            }
                                            else
                                            {
                                                checkPage = LastPage;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Thread.Sleep(Constants.TimeoutLinks);

                                client = new RestClient(BaseUrl);

                                client.Timeout = -1;
                                request = new RestRequest(Method.GET);
                                response = client.Execute(request);

                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    html.LoadHtml(response.Content);

                                    offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                                    ads = offers.Descendants("div").Where(p => p.GetAttributeValue("data-id", "") != "").Select(p => p.GetAttributeValue("data-id", "")).ToList();
                                }
                            }
                        }
                    }


                    List<string> links = new List<string>();

                    for (int i = 0; i < ads.Count; i++)
                    {
                        string link = $"https://kolesa.kz/a/show/{ads[i]}";

                        if (ads[i] != lastAd.Id)
                        {
                            links.Add(link);
                        }
                        else
                        {
                            break;
                        }
                    }

                    List<Ad> NewAds = new List<Ad>();

                    if (links.Count == ads.Count && ads.Count >= 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            NewAds.Add(GetDataAd(links[i], ChatId));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < links.Count; i++)
                        {
                            NewAds.Add(GetDataAd(links[i], ChatId));
                        }
                    }

                    return NewAds;
                }
                else
                {
                    throw new Exception("Страница поиска не найдена");
                }
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        #endregion

        #region Работа с данными объявления
        public Ad GetDataAd(string link, long ChatId)
        {
            try
            {
                Thread.Sleep(Constants.TimeoutLinks);

                HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

                var client = new RestClient(link);

                client.Timeout = -1;
                var request = new RestRequest(Method.GET);

                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    html.LoadHtml(response.Content);

                    string id = html.DocumentNode.Descendants("div").Where(p => p.Attributes.Where(j => j.Name == "data-id").Count() != 0).First().GetAttributeValue("data-id", null);

                    List<string> LinksPhotos = html.DocumentNode.Descendants("li").Where(p => p.GetAttributeValue("class", "") == "gallery__thumb").Select(p => p.Descendants("button").First().GetAttributeValue("data-href", null)).ToList();

                    if (LinksPhotos.Count == 0)
                    {
                        if (html.DocumentNode.Descendants("button").Where(p => p.Attributes.Where(j => j.Value == "gallery__main js__gallery-main").Count() != 0).FirstOrDefault() != null)
                        {
                            if (html.DocumentNode.Descendants("button").Where(p => p.Attributes.Where(j => j.Value == "gallery__main js__gallery-main").Count() != 0).FirstOrDefault().GetAttributeValue("data-href", null) != null)
                            {
                                LinksPhotos.Add(html.DocumentNode.Descendants("button").Where(p => p.Attributes.Where(j => j.Value == "gallery__main js__gallery-main").Count() != 0).FirstOrDefault().GetAttributeValue("data-href", null));
                            }
                        }
                    }

                    string Title = html.DocumentNode.Descendants("h1").FirstOrDefault().InnerText.Trim('\n').Trim().Replace("  ", " ");

                    var Params = html.DocumentNode.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "offer__parameters").First().Descendants("dl").ToList();

                    string City = "";
                    string Body = "";
                    string EngineCapaticy = "";
                    string Mileage = "";
                    string Transmisson = "";
                    string Wheel = "";
                    string Color = "";
                    string Actuator = "";
                    string CustomsClearedKZ = "";
                    string State = "На ходу";

                    for (int i = 0; i < Params.Count; i++)
                    {
                        switch (Params[i].Descendants("dt").First().GetAttributeValue("title", ""))
                        {
                            case "Город":
                                City = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Кузов":
                                Body = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Объем двигателя, л":
                                EngineCapaticy = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Пробег":
                                Mileage = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Коробка передач":
                                Transmisson = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Руль":
                                Wheel = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Цвет":
                                Color = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Привод":
                                Actuator = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Растаможен в Казахстане":
                                CustomsClearedKZ = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                break;
                            case "Аварийная/Не на ходу":
                                State = "Аварийная/Не на ходу";
                                break;
                        }
                    }

                    string Discription = null;

                    if (html.DocumentNode.Descendants("div").Where(p => p.Attributes.Where(j => j.Value == "offer__description").Count() != 0).Count() != 0)
                    {
                        var _descriptions = html.DocumentNode.Descendants("div").Where(p => p.Attributes.Where(j => j.Value == "offer__description").Count() != 0).FirstOrDefault().Descendants("div").Where(p => p.GetAttributeValue("class", "") == "text").ToList();

                        if (_descriptions.Count == 1)
                        {
                            Discription = _descriptions[0].InnerText.Trim('\n').Trim().Replace("&quot;", "\"");
                        }
                        else
                        {
                            Discription = _descriptions[1].InnerText.Trim('\n').Trim().Replace("&quot;", "\"");
                        }
                    }

                    string Price = html.DocumentNode.Descendants("div").Where(p => p.Attributes.Where(j => j.Value == "offer__price").Count() != 0).FirstOrDefault().InnerText.Trim('\n').Trim().Replace("&nbsp;", " ").Replace("  ", " ").Replace("\n", "");

                    return new Ad
                    {
                        Id = id,
                        PhotosFileName = GetPhotos(LinksPhotos, id, ChatId),
                        Title = Title,
                        City = City,
                        Body = Body,
                        EngineCapacity = EngineCapaticy,
                        Mileage = Mileage,
                        Transmission = Transmisson,
                        Wheel = Wheel,
                        Color = Color,
                        Actuator = Actuator,
                        CustomsClearedKZ = CustomsClearedKZ,
                        State = State,
                        Discription = Discription,
                        Price = Price,
                        URL = link
                    };
                }
                else
                {
                    throw new Exception("Страница объявления не найдена");
                }
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }

        public string GetPhotos(List<string> Links, string Id, long ChatId)
        {
            try
            {
                Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\Kolesa\{Id}");
                List<string> FileNames = new List<string>();

                for (int i = 0; i < Links.Count && i < 3; i++)
                {
                    if (Links[i] != null)
                    {
                        Thread.Sleep(Constants.TimeoutImage);

                        string link = Links[i];
                        var client = new RestClient(Links[i]);

                        client.Timeout = -1;
                        var request = new RestRequest(Method.GET);

                        byte[] response = client.DownloadData(request);

                        File.WriteAllBytes(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\Kolesa\{Id}\{i}.jpg", response);

                        FileNames.Add(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\Kolesa\{Id}\{i}.jpg");
                    }
                }

                return new OperationsPhoto().Combine(FileNames, Id, ChatId, "Kolesa");
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        #endregion

        #region Получение ссылки для поиска
        private string CombineBaseURL(Filter filter, Region region, City city, Mark mark, Model model)
        {
            string url = Constants.KolesaBaseURL;

            if (mark != null)
            {
                url += mark.KolesaId + "/";

                if (model != null)
                {
                    url += model.KolesaId + "/";
                }
            }

            if (region != null)
            {
                if (city != null)
                {
                    url += city.KolesaId + "/";
                }
                else
                {
                    url += region.KolesaId + "/";
                }
            }

            url += "?";

            if (filter.CustomsСleared == 1)
            {
                url += $"auto-custom=2&";
            }
            if (filter.Transmission != 0)
            {
                switch (filter.Transmission)
                {
                    case 1:
                        url += $"auto-car-transm=2345&";
                        break;
                    case 2:
                        url += $"auto-car-transm=1&";
                        break;
                }
            }
            if (filter.Actuator != 0)
            {
                switch (filter.Actuator)
                {
                    case 1:
                        url += $"car-dwheel=1&";
                        break;
                    case 2:
                        url += $"car-dwheel=2&";
                        break;
                    case 3:
                        url += $"car-dwheel=3&";
                        break;
                }
            }
            if (filter.FirstYear != 0)
            {
                url += $"year[from]={filter.FirstYear}&";
            }
            if (filter.SecondYear != 0)
            {
                url += $"year[to]={filter.SecondYear}&";
            }
            if (filter.FirstPrice != 0)
            {
                url += $"price[from]={filter.FirstPrice}&";
            }
            if (filter.SecondPrice != 0)
            {
                url += $"price[to]={filter.SecondPrice}&";
            }
            if (filter.FirstEngineCapacity != 0)
            {
                url += $"auto-car-volume[from]={filter.FirstEngineCapacity}&";
            }
            if (filter.SecondEngineCapacity != 0)
            {
                url += $"auto-car-volume[to]={filter.SecondEngineCapacity}&";
            }
            if (filter.Mileage != 0)
            {
                url += $"auto-run[to]={filter.Mileage}&";
            }

            return url;
        }
        #endregion

        #region Получение количества объявлений
        public int GetCountAds(Filter filter, Region region, City city, Mark mark, Model model)
        {
            var client = new RestClient(CombineCountSearchURL(filter, region, city, mark, model));
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            IRestResponse response = client.Execute(request);

            dynamic content = JsonConvert.DeserializeObject(response.Content);

            return content.nbCnt;
        }
        private string CombineCountSearchURL(Filter filter, Region region, City city, Mark mark, Model model)
        {
            string url = Constants.KolesaCountSearchURL;

            if (mark != null)
            {
                url += mark.KolesaId + "/";

                if (model != null)
                {
                    url += model.KolesaId + "/";
                }
            }

            if (region != null)
            {
                if (city != null)
                {
                    url += city.KolesaId + "/";
                }
                else
                {
                    url += region.KolesaId + "/";
                }
            }

            url += "?";

            if (filter.CustomsСleared == 1)
            {
                url += $"auto-custom=2&";
            }
            if (filter.Transmission != 0)
            {
                switch (filter.Transmission)
                {
                    case 1:
                        url += $"auto-car-transm=2345&";
                        break;
                    case 2:
                        url += $"auto-car-transm=1&";
                        break;
                }
            }
            if (filter.Actuator != 0)
            {
                switch (filter.Actuator)
                {
                    case 1:
                        url += $"car-dwheel=1&";
                        break;
                    case 2:
                        url += $"car-dwheel=2&";
                        break;
                    case 3:
                        url += $"car-dwheel=3&";
                        break;
                }
            }
            if (filter.FirstYear != 0)
            {
                url += $"year[from]={filter.FirstYear}&";
            }
            if (filter.SecondYear != 0)
            {
                url += $"year[to]={filter.SecondYear}&";
            }
            if (filter.FirstPrice != 0)
            {
                url += $"price[from]={filter.FirstPrice}&";
            }
            if (filter.SecondPrice != 0)
            {
                url += $"price[to]={filter.SecondPrice}&";
            }
            if (filter.FirstEngineCapacity != 0)
            {
                url += $"auto-car-volume[from]={filter.FirstEngineCapacity}&";
            }
            if (filter.SecondEngineCapacity != 0)
            {
                url += $"auto-car-volume[to]={filter.SecondEngineCapacity}&";
            }
            if (filter.Mileage != 0)
            {
                url += $"auto-run[to]={filter.Mileage}&";
            }

            return url;
        }
        #endregion
    }
}
