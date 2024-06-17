using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class DiscountDish
    {
        public int DiscountId { get; set; }
        public Discount Discount { get; set; }
        
        public int DishId { get; set; }
        public Dish Dish { get; set; }
    }
}
