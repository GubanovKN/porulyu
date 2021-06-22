using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace porulyu.Domain.Models
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime DateCreate { get; set; }

        public long ChatId { get; set; }

        public bool UserAgreement { get; set; }

        public int PositionDialog { get; set; }
        public int PositionView { get; set; }

        public int LastMessageId { get; set; }

        public ICollection<Report> Reports { get; set; }

        public ICollection<Complain> Complains { get; set; }

        public ICollection<Filter> Filters { get; set; }

        public long RateId { get; set; }
        public Rate Rate { get; set; }

        public string BillId { get; set; }

        public DateTime DateExpired { get; set; }

        public bool Activate { get; set; }

        [NotMapped]
        public int CountFilters
        {
            get
            {
                return Filters.Count;
            }
        }

        [NotMapped]
        public string RateView
        {
            get
            {
                return Rate.Name;
            }
        }
    }
}
