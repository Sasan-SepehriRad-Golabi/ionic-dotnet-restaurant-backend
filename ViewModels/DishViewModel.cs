using Ruddy.WEB.Enums;
using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class DishViewModel
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public double Price { get; set; }
        public double PromotionalPrice { get; set; }
        public bool IsPromotional { get; set; }
        public string Description { get; set; }
        public List<DietaryType> DietaryType { get; set; }
        public bool IsDesert { get; set; }
        public DishType DishType { get; set; }
        public double Weight { get; set; }
        public IsActive IsActive { get; set; }
        public int RestaurantId { get; set; }

        public List<DishComponent> Components { get; set; }
    }
}
