using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class DiscountGetViewModel
    {
        public int Id { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Percent { get; set; }
        public string Name { get; set; }
        public List<DishInDiscount> Dishes { get; set; }
    }

    public class DishInDiscount
    {
        public Dish Dish { get; set; }
        public int? RestaurantId { get; set; }
        public string RestaurantName { get; set; }
    }
}
