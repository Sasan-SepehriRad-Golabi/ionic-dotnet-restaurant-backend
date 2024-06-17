using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ruddy.WEB.ViewModels
{
    public class addToJournal
    {
        [Required]
        public int dishId { get; set; }
    }
    public class CheckIfUserSignedIn
    {
        [Required]
        public string CheckOrNot { get; set; }
    }
}
