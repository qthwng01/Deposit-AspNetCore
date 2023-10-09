using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Recharge_Test.Repository.Models
{
    public partial class InvoiceDetail
    {
        [Key]
        [Column("detail_id")]
        public Guid DetailId { get; set; }
        [Column("invoices_id")]
        public int? InvoicesId { get; set; }
        [Column("order_id")]
        public string OrderId { get; set; }
        [Column("payment")]
        public string Payment { get; set; }
        [Column("customer_name")]
        public string CustomerName { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("status_description")]
        public string StatusDescription { get; set; }
        [Column("created_date", TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [ForeignKey(nameof(InvoicesId))]
        [InverseProperty(nameof(Invoice.InvoiceDetails))]
        public virtual Invoice Invoices { get; set; }
    }
}
