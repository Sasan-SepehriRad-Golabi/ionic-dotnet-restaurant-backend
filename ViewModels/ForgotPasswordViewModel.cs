﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
