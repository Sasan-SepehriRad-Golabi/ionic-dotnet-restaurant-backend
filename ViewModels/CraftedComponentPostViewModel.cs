using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class CraftedComponentPostViewModel
    {
        public string NameFr { get; set; }
        public string NameEng { get; set; }
        public string NameEs { get; set; }
        public string NameNl { get; set; }
        public List<CraftedComponentIngridientViewModel> CraftedComponentIngridients { get; set; }
    }
}
