using Ruddy.WEB.Enums;
using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class OrderedItemViewModel
    {
        public string Id { get; set; }
        public string DishId { get; set; }
        public DishType DishType { get; set; }
        public DietaryType DietaryType { get; set; }
        public double Weight { get; set; }
        public string RestarauntId { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public IsActive IsActive { get; set; }
        public double Price { get; set; }
        public double PromotionalPrice { get; set; }
        public bool IsPromotial { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Count { get; set; }
        public ComponentsDishViewModel Components { get; set; }
    }
}
/*
 {
      "id": 80,
      "dishId": 17,
      "dish": {
        "dishType": 0,
        "dietaryType": null,
        "weight": 0,
        "restaurantId": 2,
        "components": null,
        "dishCategories": null,
        "id": 17,
        "name": "Syject dish with vegetables",
        "summary": null,
        "isActive": 1,
        "price": 29.99,
        "promotionalPrice": 26.99,
        "isPromotional": true,
        "description": "syject dish with tomatoes",
        "image": "https://ruddybackweb.azurewebsites.net/profileimages/ba52921b54784c3d827f2fb1b578ad4e.jpg"
      },
      "count": 1
    }
 */