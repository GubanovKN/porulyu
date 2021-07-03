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
    public class OperationsMyCar
    {
        public Ad GetLastAd(Filter filter, long ChatId)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

                string BaseUrl = CombineBaseURL(filter);

                var client = new RestClient(BaseUrl);

                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    html.LoadHtml(response.Content);
                    var offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                    var ad = offers.Descendants("div").FirstOrDefault(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0);

                    for (int i = 0; ad == null && i < 5; i++)
                    {
                        Thread.Sleep(Constants.TimeoutLinks);

                        client = new RestClient(BaseUrl + $"&page={i + 2}");

                        client.Timeout = -1;
                        request = new RestRequest(Method.GET);
                        response = client.Execute(request);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            html.LoadHtml(response.Content);

                            offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                            ad = offers.Descendants("div").FirstOrDefault(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0);
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
        public List<Ad> GetNewAds(Filter filter, Ad lastAd, long ChatId)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

                string BaseUrl = CombineBaseURL(filter);

                var client = new RestClient(BaseUrl);

                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    html.LoadHtml(response.Content);
                    var offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();

                    var ads = offers.Descendants("div").Where(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0).Select(p => p.GetAttributeValue("data-id", "")).ToList();

                    for (int i = 0; i < 5; i++)
                    {
                        Thread.Sleep(Constants.TimeoutLinks);

                        client = new RestClient(BaseUrl + $"&page={i + 2}");

                        client.Timeout = -1;
                        request = new RestRequest(Method.GET);
                        response = client.Execute(request);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            html.LoadHtml(response.Content);

                            offers = html.DocumentNode.Descendants("div").Where(p => p.Id == "results").First();
                            ads.AddRange(offers.Descendants("div").Where(p => p.GetAttributeValue("data-id", "") != "" && p.Descendants("div").Where(w => w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item up") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item fast-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item turbo-sale") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item vip") != -1 || w.GetAttributeValue("class", "").IndexOf("paid-services-icons__item hot") != -1).Count() == 0).Select(p => p.GetAttributeValue("data-id", "")).ToList());
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
        public Ad GetDataAd(string link, long ChatId)
        {
            try
            {
                Thread.Sleep(Constants.TimeoutLinks);

                HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();

                var client = new RestClient(link);

                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.105 YaBrowser/21.3.3.230 Yowser/2.5 Safari/537.36";

                client.Timeout = -1;
                var request = new RestRequest(Method.GET);

                request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.AddHeader("Accept-Encoding", "gzip, deflate, br");
                request.AddHeader("Accept-Language", "ru,en;q=0.9");
                request.AddHeader("Cache-Control", "max-age=0");
                request.AddHeader("sec-fetch-dest", "document");
                request.AddHeader("sec-fetch-mode", "navigate");
                request.AddHeader("sec-fetch-site", "same-origin");
                request.AddHeader("sec-fetch-user", "?1");
                request.AddHeader("upgrade-insecure-requests", "1");

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
                    string State = "Нет";

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
                                State = Params[i].Descendants("dd").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
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
                Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}");
                List<string> FileNames = new List<string>();

                for (int i = 0; i < Links.Count && i < 3; i++)
                {
                    Thread.Sleep(Constants.TimeoutImage);

                    string link = Links[i];
                    var client = new RestClient(Links[i]);

                    client.Timeout = -1;
                    var request = new RestRequest(Method.GET);

                    byte[] response = client.DownloadData(request);

                    File.WriteAllBytes(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\{i}.jpg", response);

                    FileNames.Add(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\{i}.jpg");
                }

                return new OperationsPhoto().Combine(FileNames, Id, ChatId);
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        public int GetCountSearch(Filter filter)
        {
            try
            {
                var client = new RestClient(CombineCountSearchURL(filter));
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("x-requested-with", "XMLHttpRequest");
                IRestResponse response = client.Execute(request);

                dynamic content = JsonConvert.DeserializeObject(response.Content);

                int count = Convert.ToInt32(content.nbCnt);

                return count;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        private string CombineCountSearchURL(Filter filter)
        {
            string url = Constants.CountSearchURL;

            if (filter.MarkAlias != null && filter.MarkAlias != "All")
            {
                url += filter.MarkAlias + "/";

                if (filter.ModelAlias != null && filter.ModelAlias != "All")
                {
                    url += filter.ModelAlias + "/";
                }
            }

            if (filter.RegionAlias != null && filter.RegionAlias != "All")
            {
                if (filter.CityAlias != null && filter.CityAlias != "All")
                {
                    url += filter.CityAlias + "/";
                }
                else
                {
                    url += filter.RegionAlias + "/";
                }
            }

            url += "?";

            if (filter.FirstYear != 0)
            {
                url += $"year[from]=={filter.FirstYear}&";
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

            return url;
        }
        private string CombineBaseURL(Filter filter)
        {
            string url = Constants.BaseURL;

            if (filter.MarkAlias != "All")
            {
                url += filter.MarkAlias + "/";

                if (filter.ModelAlias != null && filter.ModelAlias != "All")
                {
                    url += filter.ModelAlias + "/";
                }
            }

            if (filter.RegionAlias != "All")
            {
                if (filter.CityAlias != null && filter.CityAlias != "All")
                {
                    url += filter.CityAlias + "/";
                }
                else
                {
                    url += filter.RegionAlias + "/";
                }
            }

            url += "?";

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

            return url;
        }
    }
}
