using System;
using System.Collections.Generic;
using System.Text;

namespace porulyu.Domain.Models
{
    public class City
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public Region Region { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
