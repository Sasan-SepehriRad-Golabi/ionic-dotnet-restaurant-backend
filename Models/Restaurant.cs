using Ruddy.WEB.Enums;
using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class Restaurant : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string FirstPhoneNumber { get; set; }
        public string SecondPhoneNumber { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public RestaurantCategory RestaurantCategory { get; set; }
        public List<Time> Times { get; set; }
        public string VAT { get; set; }
        public string Description { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string Whatsapp { get; set; }
        public string Twitter { get; set; }
        public string Website { get; set; }
        public Uri Background { get; set; }
        public Uri Logo { get; set; }
        public string Mail { get; set; }
        public bool IsUnofficialRestaurant { get; set; } = true;
        public string chainCode { get; set; } = "";
        public string RestaurantUserId { get; set; }
        public bool IsAddByAdmin { get; set; }
        public bool adminRestaurantIncludeMenu { get; set; }
        public RestaurantUser RestaurantUser { get; set; }

        public List<SubCategory> Menu { get; set; }
    }
}
