using Ruddy.WEB.Enums;
using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public abstract class MenuItem : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        //TODO This is fucking shit. This property should be bool
        public IsActive IsActive { get; set; }
        public double Price { get; set; }
        public double PromotionalPrice { get; set; }
        public bool IsPromotional { get; set; }
        public string Description { get; set; }
        public Uri Image { get; set; }
    }
}
