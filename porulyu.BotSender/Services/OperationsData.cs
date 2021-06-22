using porulyu.BotSender.Common;
using porulyu.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace porulyu.BotSender.Services
{
    public class OperationsData
    {
        public void LoadRegions()
        {
            XDocument doc = XDocument.Load(Constants.PathDataRegionsAndCities);

            var regions = doc.Element("Regions").Elements("Region").ToList();

            for (int i = 0; i < regions.Count; i++)
            {
                Region region = new Region();

                region.Name = regions[i].Element("Name").Value;
                region.Alias = regions[i].Element("Alias").Value;

                List<City> cities = new List<City>();

                var xcities = regions[i].Element("Cities").Elements("City").ToList();

                for (int j = 0; j < xcities.Count; j++)
                {
                    City city = new City();

                    city.Name = xcities[j].Element("Name").Value;
                    city.Alias = xcities[j].Element("Alias").Value;

                    cities.Add(city);
                }

                region.Cities = cities;

                Constants.Regions.Add(region);
            }
        }
        public void LoadMarks()
        {
            XDocument doc = XDocument.Load(Constants.PathDataMarksAndModels);

            var marks = doc.Element("Marks").Elements("Mark").ToList();

            for (int i = 0; i < marks.Count; i++)
            {
                Mark mark = new Mark();

                mark.Name = marks[i].Element("Name").Value;
                mark.Alias = marks[i].Element("Alias").Value;

                List<Model> models = new List<Model>();

                var xcities = marks[i].Element("Models").Elements("Model").ToList();

                for (int j = 0; j < xcities.Count; j++)
                {
                    Model model = new Model();

                    model.Name = xcities[j].Element("Name").Value;
                    model.Alias = xcities[j].Element("Alias").Value;

                    models.Add(model);
                }

                mark.Models = models;

                Constants.Marks.Add(mark);
            }
        }
    }
}
