using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.Hubs
{
    public interface ISendOrder
    {
        public Task SendOrder(string name, Order order);
    }
}
