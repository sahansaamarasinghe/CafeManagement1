using Microsoft.AspNetCore.Identity;
using WebApplication2.Helpers;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    //public class DbInitializer
    //{

    //    public static async Task SeedRoles(IServiceProvider serviceProvider)
    //    {
    //        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    //        string[] roles = { "Admin", "Customer" };

    //        foreach (var role in roles)
    //        {
    //            if (!await roleManager.RoleExistsAsync(role))
    //            {
    //                await roleManager.CreateAsync(new IdentityRole(role));
    //            }
    //        }
    //    }
    //}

    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdmins(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1️⃣  Ensure roles exist
            foreach (var role in new[] { RoleConstants.Admin, RoleConstants.Customer })
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));

            // 2️⃣  Hard-coded admin list (could also read from appsettings)
            var adminSeeds = new (string Email, string Password)[]
            {
            ("admin@example.com",  "Admin@123"),
            ("admin1@example.com", "Admin@123")
            };

            foreach (var (email, pwd) in adminSeeds)
            {
                if (await userMgr.FindByEmailAsync(email) is not ApplicationUser admin)
                {
                    admin = new ApplicationUser
                    {
                        UserName = email.Split('@')[0],   // “admin” / “admin1”
                        Email = email,
                        EmailConfirmed = true
                    };

                    var create = await userMgr.CreateAsync(admin, pwd);
                    if (!create.Succeeded)
                        throw new Exception($"Failed seeding admin {email}: " +
                            string.Join(", ", create.Errors.Select(e => e.Description)));
                }

                // Make sure they’re in the role
                if (!await userMgr.IsInRoleAsync(admin, RoleConstants.Admin))
                    await userMgr.AddToRoleAsync(admin, RoleConstants.Admin);
            }
        }
    }


}
