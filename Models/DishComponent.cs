using Ruddy.WEB.Enums;
using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class DishComponent : IBaseEntity
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        public double Weight { get; set; }
        public double Price { get; set; }
        public IngredientType IngredientType { get; set; }
        public int SubstituteGroup { get; set; }
    }
}
