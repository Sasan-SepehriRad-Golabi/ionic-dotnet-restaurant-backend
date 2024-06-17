using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class RestaurantOrderViewModel
    {
        public int Id { get; set; }

        public DateTime CreationDate { get; set; }
        public TypeOfPayment TypeOfPayment { get; set; }
        public Status OrderStatus { get; set; }

        public double OrderPrice { get; set; }
        public double? PromotionalOrderPrice { get; set; }

        public List<OrderedItemV2ViewModel> OrderedItems { get; set; }

        public string CustomerId { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerPhoneNumber { get; set; }

        public int? RestaurantId { get; set; }
    }

    public class OrderedItemV2ViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double? PromotionalPrice { get; set; }
        public Uri Image { get; set; }
        public string Comment { get; set; }
        public int Count { get; set; }
        public List<OrderedIngredientViewModel> OrderedIngredients { get; set; }
    }

    public class OrderedIngredientViewModel
    {
        public string IngredientNameFr { get; set; }
        public string IngredientNameEng { get; set; }
        public string IngredientNameEs { get; set; }
        public string IngredientNameNl { get; set; }
        public IngredientType IngredientType { get; set; }
        public double Price { get; set; }
        public double Weight { get; set; }
    }
}
