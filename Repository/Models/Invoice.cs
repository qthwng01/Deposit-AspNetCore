using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Recharge_Test.Repository.Models
{
    public partial class Invoice
    {
        public Invoice()
        {
            InvoiceDetails = new HashSet<InvoiceDetail>();
        }

        [Key]
        [Column("invoices_id")]
        public int InvoicesId { get; set; }
        [Column("customer_id")]
        public int? CustomerId { get; set; }
        [Column("foreign_currency")]
        [StringLength(50)]
        public string ForeignCurrency { get; set; }
        [Column("total_amount")]
        public double? TotalAmount { get; set; }
        [Column("total_exchange")]
        public string? TotalExchange { get; set; }
        [Column("status_id")]
        public int? StatusId { get; set; }
        [Column("note")]
        public string Note { get; set; }
        [Column("created_date", TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [ForeignKey(nameof(CustomerId))]
        [InverseProperty("Invoices")]
        public virtual Customer Customer { get; set; }
        [InverseProperty(nameof(InvoiceDetail.Invoices))]
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}
