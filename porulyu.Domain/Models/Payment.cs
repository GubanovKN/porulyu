using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace porulyu.Domain.Models
{
    public class Payment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime DateCreate { get; set; }
        public string PaymentId { get; set; }
        public string PaymentURL { get; set; }
        public string ReceiptURL { get; set; }
        public string Action { get; set; }
        public long ActionId { get; set; }
        public int Status { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
}
