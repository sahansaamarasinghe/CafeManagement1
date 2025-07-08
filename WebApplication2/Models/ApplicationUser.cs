﻿using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get;  set; }

        public ICollection<Order>? Orders { get; set; }
    }
}
