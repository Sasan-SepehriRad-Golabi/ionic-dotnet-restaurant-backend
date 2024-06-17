using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class Discount : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Percent { get; set; }
        public string Name { get; set; }

        public List<DiscountDish> DiscountDishes { get; set; }

    }
}
