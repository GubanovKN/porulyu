using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Newtonsoft.Json;
using porulyu.BotSender.Common;
using porulyu.BotSender.Services.Main;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Services;
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
    public class OperationsAster
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

                    var offers = html.DocumentNode.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "catalog__list").First();

                    var ad = offers.Descendants("a").FirstOrDefault(p => p.GetAttributeValue("class", "") == "car__link").GetAttributeValue("href", "");

                    if (ad != null)
                    {
                        string link = $"https://aster.kz{ad}";

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
        public async Task GetNewAds(Filter filter, List<Ad> Ads, long ChatId, Region region, City city, Mark mark, Model model)
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
                    var offers = html.DocumentNode.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "catalog__list").First();

                    var ads = offers.Descendants("a").Where(p => p.GetAttributeValue("class", "") == "car__link").Select(p => p.GetAttributeValue("href", "")).ToList();

                    for (int i = 0; i < 2; i++)
                    {
                        Thread.Sleep(Constants.TimeoutLinks);

                        if (BaseUrl[BaseUrl.Length - 1] == '/')
                        {
                            client = new RestClient(BaseUrl + $"?page={i + 2}");
                        }
                        else
                        {
                            client = new RestClient(BaseUrl + $"&page={i + 2}");
                        }

                        client.Timeout = -1;
                        request = new RestRequest(Method.GET);
                        response = client.Execute(request);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            html.LoadHtml(response.Content);

                            offers = html.DocumentNode.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "catalog__list").First();
                            ads.AddRange(offers.Descendants("a").Where(p => p.GetAttributeValue("class", "") == "car__link").Select(p => p.GetAttributeValue("href", "")).ToList());
                        }
                    }

                    List<string> links = new List<string>();

                    for (int i = 0; i < ads.Count; i++)
                    {
                        string link = $"https://aster.kz{ads[i]}";

                        if (Ads.FirstOrDefault(p => p.SiteId == ads[i].Split('/')[2] && p.Site == "Aster") == null)
                        {
                            links.Add(link);
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (int i = links.Count - 1; i >= 0; i--)
                    {
                        Ad NewAd = GetDataAd(links[i], ChatId);

                        await new OperationsBot().SendNewAd(NewAd, ChatId);

                        await new OperationsAd().Create(NewAd, filter);
                    }
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

                    string id = html.DocumentNode.Descendants("main").First().GetAttributeValue("itemprop", "");
                    List<string> LinksPhotos = html.DocumentNode.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "splide__list").First().Descendants("a").Select(p => p.GetAttributeValue("href", null)).ToList();

                    string Title = html.DocumentNode.Descendants("h1").First().GetAttributeValue("content", "") + "г.";

                    var Params = html.DocumentNode.Descendants("div").Where(p => p.GetAttributeValue("class", "") == "description").First().Descendants("div").ToList();

                    string temp = html.DocumentNode.Descendants("div").First(p => p.GetAttributeValue("class", "") == "icon-row__text").Descendants("p").First().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                    string City = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
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
                        switch (Params[i].Descendants("span").First().InnerText)
                        {
                            case "Город":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                City = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Кузов":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                Body = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Двигатель":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                EngineCapaticy = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Пробег, км":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                Mileage = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Коробка передач":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                Transmisson = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Руль":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                Wheel = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Цвет кузова":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                Color = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Привод":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                Actuator = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Растаможен":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                CustomsClearedKZ = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                            case "Состояние":
                                temp = Params[i].Descendants("span").Last().InnerText.Replace("\n", "").Replace("\r", "").Trim();
                                State = string.Join(" ", temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                break;
                        }
                    }

                    string Discription = html.DocumentNode.Descendants("p").First(p => p.GetAttributeValue("class", "") == "text-preline").InnerText.Replace("\n", "").Replace("\r", "").Trim();

                    string Price = html.DocumentNode.Descendants("meta").First(p => p.GetAttributeValue("itemprop", "") == "price").GetAttributeValue("content", "") + " ₸";

                    return new Ad
                    {
                        SiteId = id,
                        Site = "Aster",
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
                Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\Aster\{Id}");
                List<string> FileNames = new List<string>();

                for (int i = 0; i < Links.Count && i < 3; i++)
                {
                    Thread.Sleep(Constants.TimeoutImage);

                    string link = Links[i];
                    var client = new RestClient(Links[i]);

                    client.Timeout = -1;
                    var request = new RestRequest(Method.GET);

                    byte[] response = client.DownloadData(request);

                    File.WriteAllBytes(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\Aster\{Id}\{i}.webp", response);

                    ISupportedImageFormat jpgFormat = new JpegFormat { Quality = 100 };

                    byte[] imageBytes = File.ReadAllBytes(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\Aster\{Id}\{i}.webp");

                    using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        imageFactory.Load(memoryStream).Format(jpgFormat).Save(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\Aster\{Id}\{i}.jpg");
                    }

                    FileNames.Add(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\Aster\{Id}\{i}.jpg");
                }

                return new OperationsPhoto().Combine(FileNames, Id, ChatId, "Aster");
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
            string url = Constants.AsterBaseURL;

            if (region != null)
            {
                if (city != null)
                {
                    url += city.AsterId + "/";
                }
                else
                {
                    url += region.AsterId + "/";
                }
            }

            if (mark != null)
            {
                url += mark.AsterId + "/";

                if (model != null)
                {
                    url += model.AsterId + "/";
                }
            }

            url += "?";

            if (filter.Transmission != 0)
            {
                switch (filter.Transmission)
                {
                    case 1:
                        url += $"transmission=AT&";
                        break;
                    case 2:
                        url += $"transmission=MT&";
                        break;
                }
            }
            if (filter.Actuator != 0)
            {
                switch (filter.Actuator)
                {
                    case 1:
                        url += $"transmissionDriveType=_2WD&";
                        break;
                    case 2:
                        url += $"transmissionDriveType=_4WD&";
                        break;
                    case 3:
                        url += $"transmissionDriveType=_2WD&";
                        break;
                }
            }
            if (filter.FirstEngineCapacity != 0)
            {
                url += $"volumeFrom={filter.FirstEngineCapacity}&";
            }
            if (filter.SecondEngineCapacity != 0)
            {
                url += $"volumeTo={filter.SecondEngineCapacity}&";
            }
            if (filter.FirstYear != 0)
            {
                url += $"yearFrom={filter.FirstYear}&";
            }
            if (filter.SecondYear != 0)
            {
                url += $"yearTo={filter.SecondYear}&";
            }
            if (filter.Mileage != 0)
            {
                url += $"mileageTo={filter.Mileage}&";
            }
            if (filter.FirstPrice != 0)
            {
                url += $"priceFrom={filter.FirstPrice}&";
            }
            if (filter.SecondPrice != 0)
            {
                url += $"priceTo={filter.SecondPrice}&";
            }

            return url;
        }
        #endregion

        #region Получение количества объявлений
        public int GetCountAds(Filter filter, Region region, City city, Mark mark, Model model)
        {
            var client = new RestClient(Constants.AsterCountSearchURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var body = CombineCountSearchAsterURL(filter, region, city, mark, model);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            return Convert.ToInt32(response.Content);
        }
        private string CombineCountSearchAsterURL(Filter filter, Region region, City city, Mark mark, Model model)
        {
            string url = "{\n\"catalog_filter\": [\n";

            if (mark != null)
            {
                url += $"\"{mark.AsterId}";

                if (model != null)
                {
                    url += $";{model.AsterId}\"\n],\n";
                }
                else
                {
                    url += $";\"\n],\n";
                }
            }
            else
            {
                url += "\";\"\n],\n";
            }

            if (region != null)
            {
                if (city != null)
                {
                    url += $"\"city\": \"{city.AsterId}\",\n";
                }
                else
                {
                    url += $"\"city\": \"{region.AsterId}\",\n";
                }
            }
            else
            {
                url += "\"city\": null,\n";
            }

            if (filter.Mileage != 0)
            {
                url += $"\"mileageTo\": \"{filter.Mileage}\",\n";
            }
            else
            {
                url += $"\"mileageTo\": \"\",\n";
            }

            if (filter.FirstPrice != 0)
            {
                url += $"\"priceFrom\": \"{filter.FirstPrice}\",\n";
            }
            else
            {
                url += $"\"priceFrom\": \"\",\n";
            }

            if (filter.SecondPrice != 0)
            {
                url += $"\"priceTo\": \"{filter.SecondPrice}\",\n";
            }
            else
            {
                url += $"\"priceTo\": \"\",\n";
            }

            if (filter.Transmission != 0)
            {
                switch (filter.Transmission)
                {
                    case 1:
                        url += $"\"transmission\": \"AT\",\n";
                        break;
                    case 2:
                        url += $"\"transmission\": \"MT\",\n";
                        break;
                }
            }
            else
            {
                url += $"\"transmission\": null,\n";
            }

            if (filter.Actuator != 0)
            {
                switch (filter.Actuator)
                {
                    case 1:
                        url += $"\"transmissionDriveType\": \"_2WD\",\n";
                        break;
                    case 2:
                        url += $"\"transmissionDriveType\": \"_4WD\",\n";
                        break;
                    case 3:
                        url += $"\"transmissionDriveType\": \"_2WD\",\n";
                        break;
                }
            }
            else
            {
                url += $"\"transmissionDriveType\": null,\n";
            }

            if (filter.FirstEngineCapacity != 0)
            {
                url += $"\"volumeFrom\": \"{Math.Round(filter.FirstEngineCapacity, 0)}\",\n";
            }
            else
            {
                url += $"\"volumeFrom\": \"\",\n";
            }

            if (filter.SecondEngineCapacity != 0)
            {
                url += $"\"volumeTo\": \"{Math.Round(filter.SecondEngineCapacity, 0)}\",\n";
            }
            else
            {
                url += $"\"volumeTo\": \"\",\n";
            }

            if (filter.FirstYear != 0)
            {
                url += $"\"yearFrom\": \"{filter.FirstYear}\",\n";
            }
            else
            {
                url += $"\"yearFrom\": \"\",\n";
            }

            if (filter.SecondYear != 0)
            {
                url += $"\"yearTo\": \"{filter.SecondYear}\"\n";
            }
            else
            {
                url += $"\"yearTo\": \"\"\n";
            }

            url += "}";

            return url;
        }
        #endregion
    }
}
