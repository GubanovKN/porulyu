using Newtonsoft.Json;
using porulyu.BotMain.Common;
using porulyu.Domain.Models;
using porulyu.Infrastructure.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotMain.Services.Main
{
    public class OperationsAd
    {
        #region Общее количество объявлений
        public async Task<int> GetCountAds(Filter filter)
        {
            int Count = 0;

            Region region = null;
            City city = null;

            if (filter.RegionId != 0)
            {
                region = await new OperationsRegion().Get(filter.RegionId);

                if (filter.CityId != 0)
                {
                    city = region.Cities.FirstOrDefault(p => p.Id == filter.CityId);
                }
            }

            Mark mark = null;
            Model model = null;

            if (filter.MarkId != 0)
            {
                mark = await new OperationsMark().Get(filter.MarkId);

                if (filter.ModelId != 0)
                {
                    model = mark.Models.FirstOrDefault(p => p.Id == filter.ModelId);
                }
            }

            if (region == null || region.KolesaId != String.Empty)
            {
                if (city == null || city.KolesaId != String.Empty)
                {
                    if (mark == null || mark.KolesaId != String.Empty)
                    {
                        if (model == null || model.KolesaId != String.Empty)
                        {
                            Count += GetCountAdsKolesa(filter, region, city, mark, model);
                        }
                    }
                }
            }

            if (filter.CustomsСleared == 0)
            {
                if (region == null || region.OLXId != String.Empty)
                {
                    if (city == null || city.OLXId != String.Empty)
                    {
                        if (mark == null || mark.OLXId != String.Empty)
                        {
                            if (model == null || model.OLXId != String.Empty)
                            {
                                Count += GetCountAdsOLX(filter, region, city, mark, model);
                            }
                        }
                    }
                }

                if (region == null || region.AsterId != String.Empty)
                {
                    if (city == null || city.AsterId != String.Empty)
                    {
                        if (mark == null || mark.AsterId != String.Empty)
                        {
                            if (model == null || model.AsterId != String.Empty)
                            {
                                Count += GetCountAdsAster(filter, region, city, mark, model);
                            }
                        }
                    }
                }
                if (filter.Transmission == 0 && filter.Actuator == 0 && filter.FirstEngineCapacity == 0 && filter.SecondEngineCapacity == 0)
                {
                    if (region == null || region.MyCarId != String.Empty)
                    {
                        if (city == null || city.MyCarId != String.Empty)
                        {
                            if (mark == null || mark.MyCarId != String.Empty)
                            {
                                if (model == null || model.MyCarId != String.Empty)
                                {
                                    Count += GetCountAdsMyCar(filter, region, city, mark, model);
                                }
                            }
                        }
                    }
                }
            }

            return Count;
        }
        #endregion

        #region Количество объявлений на Kolesa.kz
        private int GetCountAdsKolesa(Filter filter, Region region, City city, Mark mark, Model model)
        {
            var client = new RestClient(CombineCountSearchKolesaURL(filter, region, city, mark, model));
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            IRestResponse response = client.Execute(request);

            dynamic content = JsonConvert.DeserializeObject(response.Content);

            return content.nbCnt;
        }
        private string CombineCountSearchKolesaURL(Filter filter, Region region, City city, Mark mark, Model model)
        {
            string url = Constants.CountSearchKolesaURL;

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

        #region Количество объявлений на OLX.kz
        private int GetCountAdsOLX(Filter filter, Region region, City city, Mark mark, Model model)
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
            string url = Constants.CountSearchOLXURL;

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

        #region Количество объявлений на Aster.kz
        private int GetCountAdsAster(Filter filter, Region region, City city, Mark mark, Model model)
        {
            var client = new RestClient(Constants.CountSearchAsterURL);
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

        #region Количество объявлений на MyCar.kz
        private int GetCountAdsMyCar(Filter filter, Region region, City city, Mark mark, Model model)
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
            string url = Constants.CountSearchMyCarURL;

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
