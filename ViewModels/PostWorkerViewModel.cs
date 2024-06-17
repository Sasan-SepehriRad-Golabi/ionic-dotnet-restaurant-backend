using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class PostWorkerViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<int> RestaurantIds { get; set; }
    }
}
