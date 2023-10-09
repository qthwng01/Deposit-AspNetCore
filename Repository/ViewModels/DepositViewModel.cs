using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recharge_Test.Repository.ViewModels
{
    public class DepositViewModel
    {
        public string Amount { get; set; }
        //[Required]
        //public string EnterAmount { get; set; }
        public string ForeignCurrency { get; set; }
        public string PaymentContent { get; set; }
        public bool AgreeToTerms { get; set; }
        public string Payment { get; set; }
    }
}
