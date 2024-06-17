using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.Utils
{
    public static class Initializers
    {
        public static async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (await roleManager.FindByNameAsync("Owner") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Owner"));
            }

            if (await roleManager.FindByNameAsync("Worker") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Worker"));
            }
        }

    }
}
