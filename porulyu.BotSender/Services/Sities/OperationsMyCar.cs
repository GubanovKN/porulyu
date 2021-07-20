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
    public class OperationsMyCar
    {
        #region Получение последнего объявления
        public Ad GetLastAd(Filter filter, long ChatId, Region region, City city, Mark mark, Model model)
        {
            try
            {
                string BaseUrl = CombineBaseURL(filter, region, city, mark, model);

                var client = new RestClient(BaseUrl);

                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content);

                    if (content.count != 0)
                    {
                        var ad = content.results.First;

                        List<string> Links = new List<string>();

                        foreach (var link in ad.photos)
                        {
                            Links.Add(link.image.ToString());
                        }

                        string Title = $"{ad.car_mark_name.ToString()} {ad.car_model_name.ToString()} {ad.year_manufactured.ToString()}г";
                        string Body = "Не указано";
                        string Mileage = ad.mileage.ToString() + " км";
                        string Transmission = "Не указано";
                        string Actuator = "Не указано";
                        string Price = ad.price.ToString() + " ₸";

                        foreach (var param in ad.car_modification)
                        {
                            switch (param.Name.ToString())
                            {
                                case "coupe_type":
                                    Body = param.Value.ToString();
                                    break;
                                case "drive_type":
                                    Actuator = param.Value.ToString();
                                    break;
                                case "gearbox_type":
                                    Transmission = param.Value.ToString();
                                    break;
                            }
                        }

                        return new Ad
                        {
                            Site = "MyCar",
                            SiteId = ad.id.ToString(),
                            PhotosFileName = GetPhotos(Links, ad.id.ToString(), ChatId),
                            Title = Title,
                            City = ad.city_info.name.ToString(),
                            Body = Body,
                            Mileage = Mileage,
                            Transmission = Transmission,
                            Actuator = Actuator,
                            Price = Price,
                            URL = $"https://mycar.kz/announcement/{ad.id.ToString()}"
                        };
                    }
                    else
                    {
                        throw new Exception("По заданному фильтру ничего не найдено");
                    }
                }
                else
                {
                    throw new Exception("MyCar API не доступен");
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
                string BaseUrl = CombineBaseURL(filter, region, city, mark, model);

                var client = new RestClient(BaseUrl);

                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content);

                    if (content.count != 0)
                    {
                        List<Ad> NewAds = new List<Ad>();

                        foreach (var ad in content.results)
                        {
                            if (Ads.FirstOrDefault(p => p.SiteId == ad.id.ToString() && p.Site == "MyCar") == null)
                            {
                                List<string> Links = new List<string>();

                                foreach (var link in ad.photos)
                                {
                                    Links.Add(link.image.ToString());
                                }

                                string Title = $"{ad.car_mark_name.ToString()} {ad.car_model_name.ToString()} {ad.year_manufactured.ToString()}г";
                                string Body = "Не указано";
                                string Mileage = ad.mileage.ToString() + " км";
                                string Transmission = "Не указано";
                                string Actuator = "Не указано";
                                string Price = ad.price.ToString() + " ₸";

                                foreach (var param in ad.car_modification)
                                {
                                    switch (param.Name.ToString())
                                    {
                                        case "coupe_type":
                                            Body = param.Value.ToString();
                                            break;
                                        case "drive_type":
                                            Actuator = param.Value.ToString();
                                            break;
                                        case "gearbox_type":
                                            Transmission = param.Value.ToString();
                                            break;
                                    }
                                }

                                Ad NewAd = new Ad
                                {
                                    Site = "MyCar",
                                    SiteId = ad.id.ToString(),
                                    PhotosFileName = GetPhotos(Links, ad.id.ToString(), ChatId),
                                    Title = Title,
                                    City = ad.city_info.name.ToString(),
                                    Body = Body,
                                    Mileage = Mileage,
                                    Transmission = Transmission,
                                    Actuator = Actuator,
                                    Price = Price,
                                    URL = $"https://mycar.kz/announcement/{ad.id.ToString()}"
                                };

                                await new OperationsBot().SendNewAd(NewAd, ChatId);

                                await new OperationsAd().Create(NewAd, filter);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("По заданному фильтру ничего не найдено");
                    }
                }
                else
                {
                    throw new Exception("MyCar API не доступен");
                }
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        #endregion

        #region Работа с данными объявления

        public string GetPhotos(List<string> Links, string Id, long ChatId)
        {
            try
            {
                Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\MyCar\{Id}");
                List<string> FileNames = new List<string>();

                for (int i = 0; i < Links.Count && i < 3; i++)
                {
                    Thread.Sleep(Constants.TimeoutImage);

                    string link = Links[i];
                    var client = new RestClient(Links[i]);

                    client.Timeout = -1;
                    var request = new RestRequest(Method.GET);

                    byte[] response = client.DownloadData(request);

                    File.WriteAllBytes(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\MyCar\{Id}\{i}.jpg", response);

                    FileNames.Add(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\MyCar\{Id}\{i}.jpg");
                }

                return new OperationsPhoto().Combine(FileNames, Id, ChatId, "MyCar");
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        #endregion

        #region Получения ссылки для поиска
        private string CombineBaseURL(Filter filter, Region region, City city, Mark mark, Model model)
        {
            string url = Constants.MyCarBaseURL;

            if (mark != null)
            {
                url += $"mark_in={mark.MyCarId}&";

                if (model != null)
                {
                    url += $"model_in={model.MyCarId}&";
                }
            }

            if (region != null)
            {
                if (city != null)
                {
                    url += $"city_id_in={city.MyCarId}&";
                }
                else
                {
                    string cities = "";
                    foreach (var item in region.Cities)
                    {
                        if (item.MyCarId != String.Empty)
                        {
                            cities += $"{item.MyCarId},";
                        }
                    }
                    cities = cities.TrimEnd(',');
                    url += $"city_id_in={cities}&";
                }
            }

            if (filter.FirstYear != 0)
            {
                url += $"min_year={filter.FirstYear}&";
            }
            if (filter.SecondYear != 0)
            {
                url += $"max_year={filter.SecondYear}&";
            }
            if (filter.FirstPrice != 0)
            {
                url += $"min_price={filter.FirstPrice}&";
            }
            if (filter.SecondPrice != 0)
            {
                url += $"max_price={filter.SecondPrice}&";
            }
            if (filter.Mileage != 0)
            {
                url += $"max_mileage={filter.Mileage}&";
            }

            return url;
        }
        #endregion

        #region Получение количества объявлений
        public int GetCountAds(Filter filter, Region region, City city, Mark mark, Model model)
        {
            var client = new RestClient(CombineCountSearchMyCarURL(filter, region, city, mark, model));
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            dynamic content = JsonConvert.DeserializeObject(response.Content);

            return content.count;
        }
        private string CombineCountSearchMyCarURL(Filter filter, Region region, City city, Mark mark, Model model)
        {
            string url = Constants.MyCarCountSearchURL;

            if (mark != null)
            {
                url += $"mark_in={mark.MyCarId}&";

                if (model != null)
                {
                    url += $"model_in={model.MyCarId}&";
                }
            }

            if (region != null)
            {
                if (city != null)
                {
                    url += $"city_id_in={city.MyCarId}&";
                }
                else
                {
                    string cities = "";
                    foreach (var item in region.Cities)
                    {
                        if (item.MyCarId != String.Empty)
                        {
                            cities += $"{item.MyCarId},";
                        }
                    }
                    cities = cities.TrimEnd(',');
                    url += $"city_id_in={cities}&";
                }
            }

            if (filter.FirstYear != 0)
            {
                url += $"min_year={filter.FirstYear}&";
            }
            if (filter.SecondYear != 0)
            {
                url += $"max_year={filter.SecondYear}&";
            }
            if (filter.FirstPrice != 0)
            {
                url += $"min_price={filter.FirstPrice}&";
            }
            if (filter.SecondPrice != 0)
            {
                url += $"max_price={filter.SecondPrice}&";
            }
            if (filter.Mileage != 0)
            {
                url += $"max_mileage={filter.Mileage}&";
            }

            return url;
        }
        #endregion
    }
}
