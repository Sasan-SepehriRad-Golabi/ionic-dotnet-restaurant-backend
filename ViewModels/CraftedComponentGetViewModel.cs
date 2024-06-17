using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class CraftedComponentGetViewModel
    {
        public int Id { get; set; }
        public string NameFr { get; set; }
        public string NameEng { get; set; }
        public string NameEs { get; set; }
        public string NameNl { get; set; }
        public List<CraftedIngidient> CraftedIngidients { get; set; }
    }

    public class CraftedIngidient
    {
        public Ingredient Ingridient { get; set; }
        public double Weight { get; set; }
    }
}
