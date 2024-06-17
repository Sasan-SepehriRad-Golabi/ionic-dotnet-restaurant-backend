using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ruddy.WEB.Models
{
    public class CraftedComponent
    {
        public int Id { get; set; }
        public string NameFr { get; set; }
        public string NameEng { get; set; }
        public string NameEs { get; set; }
        public string NameNl { get; set; }

        public string RestaurantUserId { get; set; }
        public RestaurantUser RestaurantUser { get; set; }

        public List<CraftedComponentIngridient> CraftedComponentIngridients { get; set; }
    }
}
