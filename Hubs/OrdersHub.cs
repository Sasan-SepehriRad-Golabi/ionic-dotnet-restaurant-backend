using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.Hubs
{
    public class OrdersHub : Hub
    {
        private readonly UserManager<Account> _userManager;
        private readonly ApplicationDbContext _context;


        public OrdersHub(UserManager<Account> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromGroup();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task ConnectToGroups()
        {
            var user = await _userManager.FindByNameAsync(this.Context.User.Identity.Name);

            await Groups.AddToGroupAsync(Context.ConnectionId, user.Id);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task RemoveFromGroup()
        {
            var user = await _userManager.FindByNameAsync(this.Context.User.Identity.Name);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.Id);
        }
    }
}
