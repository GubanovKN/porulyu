using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace porulyu.Domain.Models
{
    public class City
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Name { get; set; }
        public string KolesaId { get; set; }
        public string OLXId { get; set; }
        public string AsterId { get; set; }
        public string MyCarId { get; set; }
        public long RegionId { get; set; }
        public Region Region { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
