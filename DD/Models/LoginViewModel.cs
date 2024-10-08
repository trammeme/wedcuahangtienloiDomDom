﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class LoginViewModel
    {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; } // Changed from Email to Username

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
    }
