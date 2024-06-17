using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class AlternativeIngredient: IBaseEntity
    {
        public int Id { get; set; }
        public int MainIngredientId { get; set; }
        public Ingredient MainIngredient { get; set; }
        public int AltIngredientId { get; set; }
    }
}
