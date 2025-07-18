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
            var cfg = services.GetRequiredService<IConfiguration>();

            //var cfg = services.GetRequiredService<IConfiguration>();
            //var adminSeeds = cfg.GetSection("AdminSeeds").Get<IEnumerable<AdminSeed>>()!;
            foreach (var role in new[] { RoleConstants.Admin, RoleConstants.Customer })
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));
            }

            var adminSeeds = cfg.GetSection("AdminSeeds")
                                .Get<IEnumerable<AdminSeed>>()
                             ?? Enumerable.Empty<AdminSeed>();

            foreach (var seed in adminSeeds)
            {
                // a) Create the admin user if it doesn't exist
                var admin = await userMgr.FindByEmailAsync(seed.Email);
                if (admin is null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = seed.Email.Split('@')[0],  
                        Email = seed.Email,
                        FullName = "System Administrator",     
                        EmailConfirmed = true
                    };

                    var create = await userMgr.CreateAsync(admin, seed.Password);
                    if (!create.Succeeded)
                        throw new Exception($"Admin seed failed ({seed.Email}): " +
                            string.Join(", ", create.Errors.Select(e => e.Description)));
                }

                // b) Ensure the user is in the Admin role
                if (!await userMgr.IsInRoleAsync(admin, RoleConstants.Admin))
                    await userMgr.AddToRoleAsync(admin, RoleConstants.Admin);





                // ✅ 3️⃣ Ensure guest user exists
                var guestUserId = "guest_user_01";
                var guestEmail = "guest@cafeapp.com";
                var guestUser = await userMgr.FindByIdAsync(guestUserId);

                if (guestUser is null)
                {
                    guestUser = new ApplicationUser
                    {
                        Id = guestUserId, 
                        UserName = "guestuser",
                        Email = guestEmail,
                        FullName = "Guest User",
                        EmailConfirmed = true
                    };

                    var guestCreated = await userMgr.CreateAsync(guestUser, "Guest@123");

                    if (!guestCreated.Succeeded)
                        throw new Exception($"Guest user seed failed: " +
                            string.Join(", ", guestCreated.Errors.Select(e => e.Description)));
                }

                if (!await userMgr.IsInRoleAsync(guestUser, RoleConstants.Customer))
                    await userMgr.AddToRoleAsync(guestUser, RoleConstants.Customer);

            }
        }
    }


    //// 1️⃣  Ensure roles exist
    //foreach (var role in new[] { RoleConstants.Admin, RoleConstants.Customer })
    //    if (!await roleMgr.RoleExistsAsync(role))
    //        await roleMgr.CreateAsync(new IdentityRole(role));

    //// 2️⃣  Hard-coded admin list (could also read from appsettings)
    //var adminSeeds = new (string Email, string Password)[]
    //{
    //("admin@example.com",  "Admin@123"),
    //("admin1@example.com", "Admin@123")
    //};



    //foreach (var (email, pwd) in adminSeeds)
    //{
    //    if (await userMgr.FindByEmailAsync(email) is not ApplicationUser admin)
    //    {
    //        admin = new ApplicationUser
    //        {
    //            UserName = email.Split('@')[0],   // “admin” / “admin1”
    //            Email = email,
    //            EmailConfirmed = true
    //        };

    //        var create = await userMgr.CreateAsync(admin, pwd);
    //        if (!create.Succeeded)
    //            throw new Exception($"Failed seeding admin {email}: " +
    //                string.Join(", ", create.Errors.Select(e => e.Description)));
    //    }

    //    // Make sure they’re in the role
    //    if (!await userMgr.IsInRoleAsync(admin, RoleConstants.Admin))
    //        await userMgr.AddToRoleAsync(admin, RoleConstants.Admin);
    //}

    public record AdminSeed
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
