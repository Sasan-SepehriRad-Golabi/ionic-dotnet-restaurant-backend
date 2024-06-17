using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class CraftedComponentIngridient
    {
        public int CraftedComponentId { get; set; }
        public CraftedComponent CraftedComponent { get; set; }

        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        public double Weight { get; set; }
    }
}
