using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace porulyu.Domain.Models
{
    public class Rate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime DateCreate { get; set; }

        public string Name { get; set; }
        public double Price { get; set; }
        public int CountFilters { get; set; }
        public int CountReports { get; set; }
        public int CountDays { get; set; }
        public bool CanBuy { get; set; }
        public bool Demo { get; set; }

        public ICollection<User> Users { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
