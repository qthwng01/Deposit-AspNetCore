using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Recharge_Test.Repository.Models
{
    [Table("Deposit")]
    public partial class Deposit
    {
        [Key]
        [Column("deposit_id")]
        public int DepositId { get; set; }
        [Column("amount")]
        public double? Amount { get; set; }
        [Column("total_ex")]
        public double? TotalEx { get; set; }
        [Column("pm_content")]
        public string PmContent { get; set; }
        [Column("foreign_currency")]
        [StringLength(10)]
        public string ForeignCurrency { get; set; }
    }
}
