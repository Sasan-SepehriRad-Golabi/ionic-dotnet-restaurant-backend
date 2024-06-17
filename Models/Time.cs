using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class Time
    {
        public int Id { get; set; }
        public DayOfWeek Day { get; set; }
        public DateTime OpeningTime { get; set; }
        public DateTime ClosingTime { get; set; }
        public List<Pause> Pauses { get; set; }
        public int? RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

    }
}
