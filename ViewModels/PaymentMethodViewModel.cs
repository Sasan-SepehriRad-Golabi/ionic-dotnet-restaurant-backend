using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class PaymentMethodViewModel
    {
        public string Id { get; set; }
        public string Last4 { get; set; }
        public string ExpYear { get; set; }
        public string ExpMonth { get; set; }
        public bool IsDefault { get; set; }
        public string Brand { get; set; }
    }
}
