using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SendNotificationToRestaurants(string userId, int? RestaurantId, string title, string body, Dictionary<string, string> data = null)
        {
            var tokens = await _context.FcmTokens.Where(t => t.AccountId == userId && (t.RestaurantRecievers.Where(rr => rr.RestaurantId == RestaurantId).Count() > 0 || t.RestaurantRecievers.Count() == 0)).ToListAsync();

            var fails = 0;

            foreach (var t in tokens)
            {
                var pushMessage = new FirebaseAdmin.Messaging.Message()
                {
                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    
                    Android = new FirebaseAdmin.Messaging.AndroidConfig()
                    {
                        Notification = new FirebaseAdmin.Messaging.AndroidNotification()
                        {
                            Sound = "neworder.wav",
                            ChannelId = "pieceofruddy"
                        }
                    },
                    Apns = new FirebaseAdmin.Messaging.ApnsConfig()
                    {
                        Aps = new FirebaseAdmin.Messaging.Aps()
                        {
                            Sound = "neworder.caf"
                        },
                    },
                    Webpush = new FirebaseAdmin.Messaging.WebpushConfig()
                    {
                        FcmOptions = new FirebaseAdmin.Messaging.WebpushFcmOptions()
                        {
                            Link = "https://restaurant.ruddy.app/"
                        },
                        Headers =  new Dictionary<string, string>() { { "Urgency", "high"} },
                        Notification =  new FirebaseAdmin.Messaging.WebpushNotification()
                        {
                            
                        }
                    },
                    Token = t.FcmToken,
                    Data = data
                };

                if (!string.IsNullOrWhiteSpace(pushMessage.Token))
                {
                    var messaging = FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance;
                    try
                    {
                        var result = await messaging.SendAsync(pushMessage).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        fails++;
                    }
                }
            }

            if (fails < tokens.Count)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> SendNotificationToUser(string userId, string title, string body, Dictionary<string, string> data = null)
        {
            var tokens = await _context.FcmTokens.Where(t => t.AccountId == userId).ToListAsync();

            var fails = 0;

            foreach(var t in tokens)
            {
                var pushMessage = new FirebaseAdmin.Messaging.Message()
                {
                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Android = new FirebaseAdmin.Messaging.AndroidConfig()
                    {
                        Notification = new FirebaseAdmin.Messaging.AndroidNotification()
                        {
                            Sound = "default"
                        }
                    },
                    Apns =  new FirebaseAdmin.Messaging.ApnsConfig()
                    {
                        Aps = new FirebaseAdmin.Messaging.Aps()
                        {
                            Sound = "default"
                        }
                    },
                    Token = t.FcmToken,
                    Data = data
                };

                pushMessage.Webpush = new FirebaseAdmin.Messaging.WebpushConfig()
                {
                    FcmOptions =  new FirebaseAdmin.Messaging.WebpushFcmOptions()
                    {
                        Link = "https://restaurant.ruddy.app/"
                    }
                };

                if (!string.IsNullOrWhiteSpace(pushMessage.Token))
                {
                    var messaging = FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance;
                    try
                    {
                        var result = await messaging.SendAsync(pushMessage).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        fails++;
                    }
                }
            }
            
            if(fails < tokens.Count)
            {
                return true;
            }

            return false;
        }
    }
}
