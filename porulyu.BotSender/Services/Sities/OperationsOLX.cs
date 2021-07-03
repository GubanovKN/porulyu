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
    public class OperationsOLX
    {
        public Ad GetLastAd(Filter filter, long ChatId)
        {
            try
            {
                string BaseURL = CombineBaseURL(filter);

                var client = new RestClient(BaseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer 95582abdb4a193c0418fb2f13e1fb4c19c6e02d8");
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content);

                    if (content.data.Count != 0)
                    {
                        foreach (var ad in content.data)
                        {
                            if (!ad.promotion.b2c_ad_page && !ad.promotion.highlighted && !ad.promotion.premium_ad_page && !ad.promotion.top_ad && !ad.promotion.urgent)
                            {
                                List<string> Links = new List<string>();

                                foreach (var link in ad.photos)
                                {
                                    Links.Add(link.link.Replace("{width}", link.width).Replace("{height}", link.height));
                                }

                                string Title = ad.title;
                                string Body = "Не указано";
                                string EngineCapacity = "Не указано";
                                string Mileage = "Не указано";
                                string Transmission = "Не указано";
                                string Color = "Не указано";
                                string Actuator = "Не указано";
                                string State = "Не указано";
                                string Price = "Не указано";

                                foreach (var param in ad[12])
                                {
                                    switch (param.key)
                                    {
                                        case "price":
                                            Price = param.value.value + " ₸";
                                            break;
                                        case "car_body":
                                            Body = param.value.label;
                                            break;
                                        case "motor_year":
                                            Title += " " + param.value.label + "г.";
                                            break;
                                        case "transmission_type":
                                            Transmission = param.value.label;
                                            break;
                                        case "color":
                                            Color = param.value.label;
                                            break;
                                        case "condition":
                                            State = param.value.label;
                                            break;
                                        case "motor_mileage":
                                            Mileage = param.value.label;
                                            break;
                                        case "motor_engine_size":
                                            EngineCapacity = param.value.label;
                                            break;
                                    }
                                }

                                return new Ad
                                {
                                    Id = ad.id,
                                    PhotosFileName = GetPhotos(Links, ad.id, ChatId),
                                    Title = Title,
                                    City = ad.location.city.name,
                                    Body = Body,
                                    EngineCapacity = EngineCapacity,
                                    Mileage = Mileage,
                                    Transmission = Transmission,
                                    Color = Color,
                                    Actuator = Actuator,
                                    State = State,
                                    Discription = ad.description,
                                    Price = Price,
                                    URL = ad.url
                                };
                            }
                        }

                        throw new Exception("Не найдено не рекламных объявлений");
                    }
                    else
                    {
                        throw new Exception("По заданному фильтру ничего не найдено");
                    }
                }
                else
                {
                    throw new Exception("OLX Api не доступен");
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
                string BaseURL = CombineBaseURL(filter);

                var client = new RestClient(BaseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer 95582abdb4a193c0418fb2f13e1fb4c19c6e02d8");
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content);

                    if (content.data.Count != 0)
                    {
                        List<Ad> NewAds = new List<Ad>();
                        int CountAds = 0;
                        foreach (var ad in content.data)
                        {
                            if (!ad.promotion.b2c_ad_page && !ad.promotion.highlighted && !ad.promotion.premium_ad_page && !ad.promotion.top_ad && !ad.promotion.urgent)
                            {
                                if (ad.id != lastAd.Id)
                                {
                                    List<string> Links = new List<string>();

                                    foreach (var link in ad.photos)
                                    {
                                        Links.Add(link.link.Replace("{width}", link.width).Replace("{height}", link.height));
                                    }

                                    string Title = ad.title;
                                    string Body = "Не указано";
                                    string EngineCapacity = "Не указано";
                                    string Mileage = "Не указано";
                                    string Transmission = "Не указано";
                                    string Color = "Не указано";
                                    string Actuator = "Не указано";
                                    string State = "Не указано";
                                    string Price = "Не указано";

                                    foreach (var param in ad[12])
                                    {
                                        switch (param.key)
                                        {
                                            case "price":
                                                Price = param.value.value + " ₸";
                                                break;
                                            case "car_body":
                                                Body = param.value.label;
                                                break;
                                            case "motor_year":
                                                Title += " " + param.value.label + "г.";
                                                break;
                                            case "transmission_type":
                                                Transmission = param.value.label;
                                                break;
                                            case "color":
                                                Color = param.value.label;
                                                break;
                                            case "condition":
                                                State = param.value.label;
                                                break;
                                            case "motor_mileage":
                                                Mileage = param.value.label;
                                                break;
                                            case "motor_engine_size":
                                                EngineCapacity = param.value.label;
                                                break;
                                        }
                                    }

                                    NewAds.Add(new Ad
                                    {
                                        Id = ad.id,
                                        PhotosFileName = GetPhotos(Links, ad.id, ChatId),
                                        Title = Title,
                                        City = ad.location.city.name,
                                        Body = Body,
                                        EngineCapacity = EngineCapacity,
                                        Mileage = Mileage,
                                        Transmission = Transmission,
                                        Color = Color,
                                        Actuator = Actuator,
                                        State = State,
                                        Discription = ad.description,
                                        Price = Price,
                                        URL = ad.url
                                    });
                                }

                                CountAds++;
                            }
                        }

                        if (CountAds == NewAds.Count && NewAds.Count >= 3)
                        {
                            NewAds.RemoveRange(3, NewAds.Count - 3);
                        }

                        return NewAds;
                    }
                    else
                    {
                        throw new Exception("По заданному фильтру ничего не найдено");
                    }
                }
                else
                {
                    throw new Exception("OLX Api не доступен");
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
                Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\OLX\{Id}");
                List<string> FileNames = new List<string>();

                for (int i = 0; i < Links.Count && i < 3; i++)
                {
                    Thread.Sleep(Constants.TimeoutImage);

                    string link = Links[i];
                    var client = new RestClient(Links[i]);

                    client.Timeout = -1;
                    var request = new RestRequest(Method.GET);

                    byte[] response = client.DownloadData(request);

                    File.WriteAllBytes(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\OLX\{Id}\{i}.jpg", response);

                    FileNames.Add(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\OLX\{Id}\{i}.jpg");
                }

                return new OperationsPhoto().Combine(FileNames, Id, ChatId);
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
        private string CombineBaseURL(Filter filter)
        {
            string url = Constants.OLXBaseURL;

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

            if (filter.FirstYear != 0)
            {
                url += $"filter_float_motor_year:from={filter.FirstYear}&";
            }
            if (filter.SecondYear != 0)
            {
                url += $"filter_float_motor_year:to={filter.SecondYear}&";
            }

            if (filter.FirstPrice != 0)
            {
                url += $"filter_float_price:from={filter.FirstPrice}&";
            }
            if (filter.SecondPrice != 0)
            {
                url += $"filter_float_price:to={filter.SecondPrice}&";
            }

            if (filter.FirstMileage != 0)
            {
                url += $"filter_float_motor_mileage:from={filter.FirstMileage}&";
            }
            if (filter.SecondMileage != 0)
            {
                url += $"filter_float_motor_mileage:to={filter.SecondMileage}&";
            }

            if (filter.FirstEngineCapacity != 0)
            {
                url += $"filter_float_motor_engine_size:from={filter.FirstEngineCapacity}&";
            }
            if (filter.SecondEngineCapacity != 0)
            {
                url += $"filter_float_motor_engine_size:to={filter.SecondEngineCapacity}&";
            }

            return url;
        }
    }
}
