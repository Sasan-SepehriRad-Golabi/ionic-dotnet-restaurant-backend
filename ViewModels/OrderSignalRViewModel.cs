using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class OrderSignalRViewModel
    {
        public string Id { get; set; }
        public List<OrderedItemViewModel> OrderedItems;
        public string ApplicationUserId { get; set; }
        public string FirstName { get; set; }
        public string Comment { get; set; }
        public string LastName { get; set; }
        public string ConnectedAccountId { get; set; }
        public string CustomerAccountId { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantAddress { get; set; }
        public string RestaurantUserId { get; set; }
        public DateTime Date { get; set; }
        public double TotalPrice { get; set; }
        public TypeOfPayment TypeOfPayment { get; set; }
        public Status Status { get; set; }
        public string PaymentIntentId { get; set; }
        public double PromotionalPrice { get; set; }

        public OrderSignalRViewModel()
        {
            this.OrderedItems = new List<OrderedItemViewModel>();
        }
    }

}
/*
 {
  "id": 62,
  "orderedItems": [
    
  ],
  "applicationUserId": "99a23b3d-34d9-4b2b-9fbd-860d4b10a468",
  "applicationUser": {
    
    "firstName": "Danya",
    "lastName": "Asuev",
    "connectedAccountId": "acct_1I1ci4PRyCNFoQ7n",
    "customerAccountId": "cus_IcsTI9uGQ4Z5wK",
  },
  "restaurantId": 2,
  "restaurant": {
    "id": 2,
    "name": "Syject",
    "address": "вулиця Сумська, Харків, Харківська область, Украина, 61000",
    "firstPhoneNumber": "54645677767",
    "secondPhoneNumber": "89834434358",
    "longitude": 36.2281491,
    "latitude": 49.9924622,
    "restaurantCategory": 2,
    "times": null,
    "vat": "FC456756756",
    "description": "Descript",
    "facebook": "syject",
    "instagram": "syject",
    "whatsapp": "",
    "twitter": "syject",
    "website": "syject.com",
    "background": "https://ruddybackweb.azurewebsites.net/css/205af744aba8408c81426bd508f74db9.jpg",
    "logo": "https://ruddybackweb.azurewebsites.net/css/3b4c4e8e43f74715ae7073b2c492d050.jpg",
    "mail": "",
    "restaurantUserId": "52d2239d-6643-41e8-97cd-fdaac0ab3cac",
    "restaurantUser": null,
    "menu": null
  },
  "date": "0001-01-01T00:00:00",
  "price": 69.48,
  "typeOfPayment": 0,
  "status": 0,
  "paymentIntentId": null
}
 */