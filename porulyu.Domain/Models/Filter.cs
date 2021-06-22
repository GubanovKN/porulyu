using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace porulyu.Domain.Models
{
    public class Filter
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime DateCreate { get; set; }

        public string Name { get; set; }

        public string RegionAlias { get; set; }
        public string CityAlias { get; set; }

        public string MarkAlias { get; set; }
        public string ModelAlias { get; set; }

        public int FirstYear { get; set; }
        public int SecondYear { get; set; }

        public int FirstPrice { get; set; }
        public int SecondPrice { get; set; }

        public int CustomsСleared { get; set; }
        public int Transmission { get; set; }
        public int Actuator { get; set; }

        public double FirstEngineCapacity { get; set; }
        public double SecondEngineCapacity { get; set; }

        public int FirstMileage { get; set; }
        public int SecondMileage { get; set; }

        public bool Work { get; set; }

        public string LastIdAd { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
