using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class ApplicationUser : Account
    {
        public double Height { get; set; }
        public double Weight { get; set; }
        public int LevelOfActivity { get; set; }
        public List<Friend> Friends { get; set; }
        public Uri ProfileImage { get; set; }
        public List<Order> Orders { get; set; }
    }
}
