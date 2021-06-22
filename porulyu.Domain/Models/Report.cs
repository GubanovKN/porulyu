using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace porulyu.Domain.Models
{
    public class Report
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime DateCreate { get; set; }

        public string Name { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public string Link { get; set; }

        public bool Pay { get; set; }
    }
}
