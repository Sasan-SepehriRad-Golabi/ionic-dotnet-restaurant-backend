using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class OrderViewModel
    {
        public List<DishOrderViewModel> DishesIdCount { get; set; }
        public int RestarauntId { get; set; }
        //public double Price { get; set; }
        //public double PromotinalPrice { get; set; }
    }
}
