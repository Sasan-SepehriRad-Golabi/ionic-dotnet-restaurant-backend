using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class SubCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<DishCategory> DishCategories { get; set; }
        public Category Category { get; set; }
        public int? RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
