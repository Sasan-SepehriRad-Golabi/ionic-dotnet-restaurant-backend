using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class RestaurantRecievers
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
