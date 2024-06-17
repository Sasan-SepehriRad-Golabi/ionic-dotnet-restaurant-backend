using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    //TODO this is subcategory not category
    public class DishCategory
    {
        public int DishId { get; set; }
        public Dish Dish { get; set; }
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }
    }
}
