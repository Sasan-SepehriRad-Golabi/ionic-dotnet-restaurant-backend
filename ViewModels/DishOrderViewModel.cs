using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class DishOrderViewModel
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string Comment { get; set; }
        public IEnumerable<int> DishComponentsIds { get; set; }
    }
}
