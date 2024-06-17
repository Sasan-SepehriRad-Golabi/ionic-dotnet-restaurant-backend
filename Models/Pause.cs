using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class Pause
    {
        public int Id { get; set; }
        public DateTime PauseStart { get; set; }
        public DateTime PauseEnd { get; set; }
        public Time Time { get; set; }
        public int TimeId { get; set; }
    }
}
