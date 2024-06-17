using Ruddy.WEB.Enums;
using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    //TODO I'm not sure that this model is correct. Looking like fucking shit
    public class DietaryType : IBaseEntity
    {
        public int Id { get; set; }
        public Dietary Dietary { get; set; }
        public int DishId { get; set; }
    }
}
