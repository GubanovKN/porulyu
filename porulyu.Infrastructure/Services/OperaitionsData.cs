using porulyu.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace porulyu.Infrastructure.Services
{
    public class OperaitionsData
    {
        public List<Region> LoadRegions()
        {
            List<Region> Resultregions = new List<Region>();

            XDocument doc = XDocument.Load(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Data\RegionsAndCities.conf");

            var regions = doc.Element("Regions").Elements("Region").ToList();

            for (int i = 0; i < regions.Count; i++)
            {
                Region region = new Region();

                region.Id = Convert.ToInt64(regions[i].Element("Id").Value);
                region.Name = regions[i].Element("Name").Value;
                region.KolesaId = regions[i].Element("KolesaId").Value;
                region.OLXId = regions[i].Element("OLXId").Value;
                region.AsterId = regions[i].Element("AsterId").Value;
                region.MyCarId = regions[i].Element("MyCarId").Value;

                Resultregions.Add(region);
            }

            return Resultregions;
        }
        public List<City> LoadCities()
        {
            List<City> cities = new List<City>();

            XDocument doc = XDocument.Load(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Data\RegionsAndCities.conf");

            var regions = doc.Element("Regions").Elements("Region").ToList();

            for (int i = 0; i < regions.Count; i++)
            {
                var xcities = regions[i].Element("Cities").Elements("City").ToList();

                for (int j = 0; j < xcities.Count; j++)
                {
                    City city = new City();

                    city.Id = Convert.ToInt64(xcities[j].Element("Id").Value);
                    city.Name = xcities[j].Element("Name").Value;
                    city.KolesaId = xcities[j].Element("KolesaId").Value;
                    city.OLXId = xcities[j].Element("OLXId").Value;
                    city.AsterId = xcities[j].Element("AsterId").Value;
                    city.MyCarId = xcities[j].Element("MyCarId").Value;
                    city.RegionId = Convert.ToInt64(regions[i].Element("Id").Value);

                    cities.Add(city);
                }
            }

            return cities;
        }
        public List<Mark> LoadMarks()
        {
            List<Mark> Resultmarks = new List<Mark>();

            XDocument doc = XDocument.Load(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Data\MarksAndModels.conf");

            var marks = doc.Element("Marks").Elements("Mark").ToList();

            for (int i = 0; i < marks.Count; i++)
            {
                Mark mark = new Mark();

                mark.Id = Convert.ToInt64(marks[i].Element("Id").Value);
                mark.Name = marks[i].Element("Name").Value;
                mark.KolesaId = marks[i].Element("KolesaId").Value;
                mark.OLXId = marks[i].Element("OLXId").Value;
                mark.AsterId = marks[i].Element("AsterId").Value;
                mark.MyCarId = marks[i].Element("MyCarId").Value;

                Resultmarks.Add(mark);
            }

            return Resultmarks;
        }
        public List<Model> LoadModels()
        {
            List<Model> models = new List<Model>();

            XDocument doc = XDocument.Load(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + @"\Program\Data\MarksAndModels.conf");

            var marks = doc.Element("Marks").Elements("Mark").ToList();

            for (int i = 0; i < marks.Count; i++)
            {
                var xmodels = marks[i].Element("Models").Elements("Model").ToList();

                for (int j = 0; j < xmodels.Count; j++)
                {
                    Model model = new Model();

                    model.Id = Convert.ToInt64(xmodels[j].Element("Id").Value);
                    model.Name = xmodels[j].Element("Name").Value;
                    model.KolesaId = xmodels[j].Element("KolesaId").Value;
                    model.OLXId = xmodels[j].Element("OLXId").Value;
                    model.AsterId = xmodels[j].Element("AsterId").Value;
                    model.MyCarId = xmodels[j].Element("MyCarId").Value;
                    model.MarkId = Convert.ToInt64(marks[i].Element("Id").Value);

                    models.Add(model);
                }
            }

            return models;
        }
    }
}
