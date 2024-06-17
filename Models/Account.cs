using Microsoft.AspNetCore.Identity;
using Ruddy.WEB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class Account : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ConnectedAccountId { get; set; }
        public string CustomerAccountId { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender Gender { get; set; }
    }
}
