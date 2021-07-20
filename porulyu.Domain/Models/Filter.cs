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

        public long RegionId { get; set; }
        public long CityId { get; set; }

        public long MarkId { get; set; }
        public long ModelId { get; set; }

        public int FirstYear { get; set; }
        public int SecondYear { get; set; }

        public int FirstPrice { get; set; }
        public int SecondPrice { get; set; }

        public int CustomsСleared { get; set; }
        public int Transmission { get; set; }
        public int Actuator { get; set; }

        public double FirstEngineCapacity { get; set; }
        public double SecondEngineCapacity { get; set; }

        public int Mileage { get; set; }

        public bool Work { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public ICollection<Ad> Ads { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
