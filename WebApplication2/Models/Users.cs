using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class Users: IdentityUser
    {
        public string FulName { get;  set; }
    }
}
