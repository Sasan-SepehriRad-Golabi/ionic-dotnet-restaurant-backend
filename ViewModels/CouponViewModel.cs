using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class CouponViewModel
    {
        public DateTime? ExpireTime { get; set; }
        public long? RedemptionsCount { get; set; }
        public long? AmountOff { get; set; }
        public decimal? PercentOff { get; set; }
        public string CouponId { get; set; }
    }
    
    public class CouponListViewModel
    {
        public string? ExpireTime { get; set; }
        public long? RedemptionsCount { get; set; }
        public long? AmountOff { get; set; }
        public int Id { get; set; }
        public string? CouponId { get; set; }
        public string? Currency { get; set; }
        public string? Name { get; set; }
        public decimal? PercentOff { get; set; }
        public bool Enable { get; set; } = false;
    }
    
    public class AddCouponViewModel
    {
        public string CouponId { get; set; }
        public bool Enable { get; set; } = false;

    }
    
    public class UpdateCouponViewModel
    {
        public int Id { get; set; }
        public bool Enable { get; set; } = false;

    }
}
