using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class RestaurantUserUpdateViewModel
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public string Relation { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string LogoLink { get; set; }
        public string CompanyName { get; set; }
        public int OrdersNum { get; set; }
    }
}
