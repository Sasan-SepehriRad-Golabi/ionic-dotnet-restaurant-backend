using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class Dish : MenuItem
    {
        public DishType DishType { get; set; }
        public List<DietaryType> DietaryType { get; set; }
        public double Weight { get; set; }
        public int? RestaurantId { get; set; }
        //0 not inserted, 1 inserted just partly and should be removed, 2 inserted Completely 
        public int isChainInsertedByAdmin { get; set; } = 0;
        public Restaurant Restaurant { get; set; }
        public List<DishComponent> Components { get; set; }
        public ICollection<DishCategory> DishCategories { get; set; }
        public List<DiscountDish> DiscountDishes { get; set; }
    }
}
