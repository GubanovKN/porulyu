using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace porulyu.Domain.Models
{
    public class Ad
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Site { get; set; }
        public string SiteId { get; set; }

        [NotMapped]
        public string PhotosFileName { get; set; }

        public string Title { get; set; }
        public string City { get; set; }
        public string Body { get; set; }
        public string EngineCapacity { get; set; }
        public string Mileage { get; set; }
        public string Transmission { get; set; }
        public string Wheel { get; set; }
        public string Color { get; set; }
        public string Actuator { get; set; }
        public string CustomsClearedKZ { get; set; }
        public string State { get; set; }

        public string Discription { get; set; }

        public string Price { get; set; }

        public string URL { get; set; }

        public long FilterId { get; set; }
        public Filter Filter { get; set; }
    }
}
