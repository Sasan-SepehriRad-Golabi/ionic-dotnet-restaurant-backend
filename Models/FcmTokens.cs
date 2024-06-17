using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ruddy.WEB.Models
{
    //TODO Model should be modified. 
    public class FcmTokens
    {
        [Key]
        public string FcmToken { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }
        public List<RestaurantRecievers> RestaurantRecievers { get; set; }
    }
}
