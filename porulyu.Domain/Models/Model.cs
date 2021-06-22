using System;
using System.Collections.Generic;
using System.Text;

namespace porulyu.Domain.Models
{
    public class Model
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public Mark Mark { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
