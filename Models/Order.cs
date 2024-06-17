using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Ruddy.WEB.Enums;

namespace Ruddy.WEB.Models
{
    public class Order
    {
        public Order()
        {
            OrderedItems = new List<OrderedItem>();
        }

        public int Id { get; set; }

        //TODO Need more more obvious name
        public DateTime CreationDate { get; set; }
        public TypeOfPayment TypeOfPayment { get; set; }
        public Status OrderStatus { get; set; }
        public string PaymentIntentId { get; set; }

        public List<OrderedItem> OrderedItems { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int? RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        /// <summary>
        /// Check Payment Status BanContact
        /// </summary>
        public bool IsPaymentSuccess { get; set; }
        
    }
}
