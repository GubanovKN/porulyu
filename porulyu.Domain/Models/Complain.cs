using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace porulyu.Domain.Models
{
    public class Complain
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime DateCreate { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public string Type { get; set; }
        public string Link { get; set; }
    }
}
