using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public string Last4 { get; set; }
        public DateTime? BirthDate { get; set; }
        public int LevelOfActivity { get; set; }
        public string ProfileImage { get; set; }
        //public List<Friend> Friends { get; set; }
        //public List<Order> Orders { get; set; }
    }
}
