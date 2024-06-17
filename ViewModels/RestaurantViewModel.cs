using Ruddy.WEB.Enums;
using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class RestaurantViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string FirstPhoneNumber { get; set; }
        public string SecondPhoneNumber { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public RestaurantCategory RestaurantCategory { get; set; }
        public List<Time> Times { get; set; }
        public string Description { get; set; }
        public string VAT { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string Whatsapp { get; set; }
        public string Twitter { get; set; }
        public string Website { get; set; }
        public string Mail { get; set; }
    }
}
