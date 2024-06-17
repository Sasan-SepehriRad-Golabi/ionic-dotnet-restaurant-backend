using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class WorkerViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public List<string> RestaurantIds { get; set; }
    }
}
