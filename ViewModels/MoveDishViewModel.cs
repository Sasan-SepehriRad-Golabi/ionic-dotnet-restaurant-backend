using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class MoveDishViewModel
    {
        public int RestaurantId { get; set; }
        public int SubCategoryId { get; set; }
        public DishType DishType { get; set; }
    }
}
