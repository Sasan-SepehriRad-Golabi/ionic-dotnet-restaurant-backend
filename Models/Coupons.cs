using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class Coupons : IBaseEntity
    {
        public int Id { get; set; }
        public string CouponId { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public bool Enable { get; set; }

    }
}
