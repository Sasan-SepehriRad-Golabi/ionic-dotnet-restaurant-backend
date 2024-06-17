using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.Services
{
    public interface INotificationService
    {
        Task<bool> SendNotificationToUser(string userId, string title, string body, Dictionary<string, string> data = null);
        Task<bool> SendNotificationToRestaurants(string userId, int? RestaurantId, string title, string body, Dictionary<string, string> data = null);
    }
}
