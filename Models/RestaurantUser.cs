using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class RestaurantUser : Account
    {
        public string StaffLink { get; set; }
        public Uri LogoLink { get; set; }
        public string CompanyName { get; set; }
        public List<Restaurant> Restaurants { get; set; }
        public bool IsStripeAccountCompleted { get; set; }

    }
}
