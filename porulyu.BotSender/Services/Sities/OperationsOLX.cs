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
        #region Получение последнего объявления
        public Ad GetLastAd(Filter filter, long ChatId, Region region, City city, Mark mark, Model model)
        {
            try
            {
                string BaseURL = CombineBaseURL(filter, region, city, mark, model);

                var client = new RestClient(BaseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer 08b6305556c81a96245743e4b749c60d5e172686");
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content);

                    if (content.data.Count != 0)
                    {
                        foreach (var ad in content.data)
                        {
                            if (!Convert.ToBoolean(ad.promotion.b2c_ad_page) && !Convert.ToBoolean(ad.promotion.highlighted) && !Convert.ToBoolean(ad.promotion.premium_ad_page) && !Convert.ToBoolean(ad.promotion.top_ad) && !Convert.ToBoolean(ad.promotion.urgent))
                            {
                                List<string> Links = new List<string>();

                                foreach (var link in ad.photos)
                                {
                                    Links.Add(link.link.ToString().Replace("{width}", link.width.ToString()).Replace("{height}", link.height.ToString()));
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

                                foreach (var param in ad["params"])
                                {
                                    switch (param.key.ToString())
                                    {
                                        case "price":
                                            Price = param.value.value.ToString() + " ₸";
                                            break;
                                        case "car_body":
                                            Body = param.value.label.ToString();
                                            break;
                                        case "motor_year":
                                            Title += " " + param.value.label.ToString() + "г.";
                                            break;
                                        case "transmission_type":
                                            Transmission = param.value.label.ToString();
                                            break;
                                        case "color":
                                            Color = param.value.label.ToString();
                                            break;
                                        case "condition":
                                            State = param.value.label.ToString();
                                            break;
                                        case "motor_mileage":
                                            Mileage = param.value.label.ToString();
                                            break;
                                        case "motor_engine_size":
                                            EngineCapacity = param.value.label.ToString();
                                            break;
                                    }
                                }

                                return new Ad
                                {
                                    Id = ad.id.ToString(),
                                    PhotosFileName = GetPhotos(Links, ad.id.ToString(), ChatId),
                                    Title = Title,
                                    City = ad.location.city.name.ToString(),
                                    Body = Body,
                                    EngineCapacity = EngineCapacity,
                                    Mileage = Mileage,
                                    Transmission = Transmission,
                                    Color = Color,
                                    Actuator = Actuator,
                                    State = State,
                                    Discription = ad.description.ToString(),
                                    Price = Price,
                                    URL = ad.url.ToString()
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
        #endregion

        #region Получение новых объявлений
        public List<Ad> GetNewAds(Filter filter, Ad lastAd, long ChatId, Region region, City city, Mark mark, Model model)
        {
            try
            {
                string BaseURL = CombineBaseURL(filter, region, city, mark, model);

                var client = new RestClient(BaseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer 08b6305556c81a96245743e4b749c60d5e172686");
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic content = JsonConvert.DeserializeObject(response.Content);

                    if (content.data.Count != 0)
                    {
                        List<Ad> NewAds = new List<Ad>();
                        foreach (var ad in content.data)
                        {
                            if (!Convert.ToBoolean(ad.promotion.b2c_ad_page) && !Convert.ToBoolean(ad.promotion.highlighted) && !Convert.ToBoolean(ad.promotion.premium_ad_page) && !Convert.ToBoolean(ad.promotion.top_ad) && !Convert.ToBoolean(ad.promotion.urgent))
                            {
                                if (ad.id.ToString() != lastAd.Id)
                                {
                                    List<string> Links = new List<string>();

                                    foreach (var link in ad.photos)
                                    {
                                        Links.Add(link.link.ToString().Replace("{width}", link.width.ToString()).Replace("{height}", link.height.ToString()));
                                    }

                                    string Title = ad.title.ToString();
                                    string Body = "Не указано";
                                    string EngineCapacity = "Не указано";
                                    string Mileage = "Не указано";
                                    string Transmission = "Не указано";
                                    string Color = "Не указано";
                                    string Actuator = "Не указано";
                                    string State = "Не указано";
                                    string Price = "Не указано";

                                    foreach (var param in ad["params"])
                                    {
                                        switch (param.key.ToString())
                                        {
                                            case "price":
                                                Price = param.value.value.ToString() + " ₸";
                                                break;
                                            case "car_body":
                                                Body = param.value.label.ToString();
                                                break;
                                            case "motor_year":
                                                Title += " " + param.value.label.ToString() + "г.";
                                                break;
                                            case "transmission_type":
                                                Transmission = param.value.label.ToString();
                                                break;
                                            case "color":
                                                Color = param.value.label.ToString();
                                                break;
                                            case "condition":
                                                State = param.value.label.ToString();
                                                break;
                                            case "motor_mileage":
                                                Mileage = param.value.label.ToString();
                                                break;
                                            case "motor_engine_size":
                                                EngineCapacity = param.value.label.ToString();
                                                break;
                                        }
                                    }

                                    NewAds.Add(new Ad
                                    {
                                        Id = ad.id,
                                        PhotosFileName = GetPhotos(Links, ad.id.ToString(), ChatId),
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
                                else
                                {
                                    break;
                                }
                            }
                        }

                        //if (CountAds == NewAds.Count && NewAds.Count >= 3)
                        //{
                        //    NewAds.RemoveRange(3, NewAds.Count - 3);
                        //}

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
        #endregion

        #region Работа с данными объявления
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

                return new OperationsPhoto().Combine(FileNames, Id, ChatId, "OLX");
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
            string url = Constants.OLXBaseURL;

            if (mark != null)
            {
                url += $"category_id={mark.OLXId}&";

                if (model != null)
                {
                    url += $"filter_enum_model[0]={model.OLXId}&";
                }
            }
            else
            {
                url += $"category_id=108&";
            }

            if (region != null)
            {
                url += $"region_id={region.OLXId}&";

                if (city != null)
                {
                    url += $"city_id={city.OLXId}&";
                }
            }

            url += "sort_by=created_at:desc&";

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

            if (filter.Mileage != 0)
            {
                url += $"filter_float_motor_mileage:to={filter.Mileage}&";
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
        #endregion

        #region Получение количества объявлений
        public int GetCountAds(Filter filter, Region region, City city, Mark mark, Model model)
        {
            var client = new RestClient(CombineCountSearchOLXURL(filter, region, city, mark, model));
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer 2ea3256734b100b12a7076ab7aa7edc48e1aa993");
            IRestResponse response = client.Execute(request);

            dynamic content = JsonConvert.DeserializeObject(response.Content);

            return content.data.total_count;
        }
        private string CombineCountSearchOLXURL(Filter filter, Region region, City city, Mark mark, Model model)
        {
            string url = Constants.OLXCountSearchURL;

            if (mark != null)
            {
                url += $"category_id={mark.OLXId}&";

                if (model != null)
                {
                    url += $"filter_enum_model[0]={model.OLXId}&";
                }
            }
            else
            {
                url += $"category_id=108&";
            }

            if (region != null)
            {
                url += $"region_id={region.OLXId}&";

                if (city != null)
                {
                    url += $"city_id={city.OLXId}&";
                }
            }

            url += "sort_by=created_at:desc&";

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

            if (filter.Mileage != 0)
            {
                url += $"filter_float_motor_mileage:to={filter.Mileage}&";
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
        #endregion
    }
}
