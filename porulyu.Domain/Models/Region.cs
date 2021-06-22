using System;
using System.Collections.Generic;
using System.Text;

namespace porulyu.Domain.Models
{
    public class Region
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public List<City> Cities { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
