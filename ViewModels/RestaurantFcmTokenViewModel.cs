using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class RestaurantFcmTokenViewModel
    {
        public string FcmToken { get; set; }
        public List<int> RestaurantIds { get; set; }
    }
}
