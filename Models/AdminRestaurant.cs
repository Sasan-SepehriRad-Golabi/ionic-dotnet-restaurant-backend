using Ruddy.WEB.Enums;
using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruddy.WEB.Models
{
    public class AdminRestaurant : IBaseEntity
    {


        public int Id { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string RestaurantName { get; set; } = "";

        public int RestaurantCtegory { get; set; }

        public string Chain { get; set; } = "";

        public string Country { get; set; } = "";

        public string Region { get; set; } = "";

        public string AddressLine1 { get; set; } = "";

        public string FirstPhoneNumber { get; set; } = "";

        public string Email { get; set; } = "";

        public string Website { get; set; } = "";

        public string Facebook { get; set; } = "";

        public string Twitter { get; set; } = "";

        public string Instagram { get; set; } = "";

        public string OpenTime { get; set; } = "";

        public string CloseTime { get; set; } = "";

        public string Logo { get; set; } = "";

        public string BackgroundImage { get; set; } = "";

        public string Vat { get; set; } = "Not Provided";

        public string Description { get; set; } = "not Provided";

        public bool IsUnofficialRestaurant { get; set; }
    }
}
