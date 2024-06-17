using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class OrderedItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double? PromotionalPrice { get; set; }
        public Uri Image { get; set; }
        public string Comment { get; set; }
        public int Count { get; set; }
        public List<OrderedIngredient> OrderedIngredients { get; set; }

        public int ItemСharacteristicsId { get; set; }
        public ItemСharacteristics ItemСharacteristics { get; set; }
    }
}
