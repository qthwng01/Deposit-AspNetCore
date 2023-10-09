using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Recharge_Test.Repository.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Invoices = new HashSet<Invoice>();
        }

        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }
        [Column("username")]
        public string Username { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("customer_name")]
        public string CustomerName { get; set; }
        [Column("account_balance")]
        public double? AccountBalance { get; set; }

        [InverseProperty(nameof(Invoice.Customer))]
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
