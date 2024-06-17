using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class OrderedIngredient
    {
        public int Id { get; set; }

        public string IngredientNameFr { get; set; }
        public string IngredientNameEng { get; set; }
        public string IngredientNameEs { get; set; }
        public string IngredientNameNl { get; set; }
        public IngredientType IngredientType { get; set; }

        public double Price { get; set; }
        public double Weight { get; set; }
    }
}
