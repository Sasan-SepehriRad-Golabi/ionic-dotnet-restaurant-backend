using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class DiscountPostViewModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Percent { get; set; }
        public string Name { get; set; }

        public List<int> DishesId { get; set; }
    }
}
