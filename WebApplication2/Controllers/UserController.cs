using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.DTOs;
using WebApplication2.Helpers;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _email;                     
        private readonly IConfiguration _cfg;
       //private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> um, IEmailSender email, IConfiguration cfg)
        {
            _userManager = um;
            _email = email;
            _cfg = cfg;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserDetailsDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserDetailsDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return Ok(userList);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = $"{_cfg["Frontend:BaseUrl"]}/set-password?email={email}&token={Uri.EscapeDataString(token)}";

            await _email.SendEmailAsync(email, "Reset your Café password",
                $"Click to reset: <a href=\"{link}\">Reset password</a>");

            return Ok("Password-reset email sent");
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            // 1️⃣  Ask Identity for all users that are in the Customer role
            var customers = await _userManager.GetUsersInRoleAsync(RoleConstants.Customer);

            // 2️⃣  Project to a lightweight DTO
            var result = customers
                .Select(u => new { u.Id, u.Email, u.FullName })
                .ToList();

            return Ok(result);
        }
        

        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var dto = new UserDetailsDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList()
            };

            return Ok(dto);
        }

        [HttpPut("by-email")]
        public async Task<IActionResult> UpdateByEmail(
    [FromQuery] string email,             // admin passes email in query-string
    [FromBody] UpdateUserByEmailDTO dto) // JSON body with fields to update
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return NotFound("Customer not found");

            // Only overwrite properties that were provided
            if (!string.IsNullOrWhiteSpace(dto.UserName)) user.UserName = dto.UserName;
            if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded
                ? Ok("Customer updated")
                : BadRequest(result.Errors);
        }



        [HttpPost("invite")]
        public async Task<IActionResult> InviteCustomer([FromQuery] string email)
        {
            // create stub user if not exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var create = await _userManager.CreateAsync(user);     // no password
                if (!create.Succeeded) return BadRequest(create.Errors);
                await _userManager.AddToRoleAsync(user, RoleConstants.Customer);
            }

            // generate reset-password token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontUrl = _cfg["Frontend:BaseUrl"];                  // e.g., https://localhost:3000
            var link = $"{frontUrl}/set-password?email={email}&token={Uri.EscapeDataString(token)}";

            await _email.SendEmailAsync(email,
                "Set up your Café account",
                $"Hi! Click the link to choose your password: <a href=\"{link}\">Set password</a>");

            return Ok("Invitation sent");
        }

        //-----------------------DELETE-CUSTOMER------------------------------------------
        //[HttpDelete]
        //public async Task<IActionResult> Delete([FromQuery] string email)
        //{
        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (user is null) return NotFound();

        //    var res = await _userManager.DeleteAsync(user);
        //    return res.Succeeded ? Ok("Deleted") : BadRequest(res.Errors);
        //}


        // POST api/user/activate?email=john@example.com
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return NotFound("User not found");

            user.LockoutEnd = null;                         // unlock now
            await _userManager.UpdateAsync(user);
            return Ok("Customer activated");
        }

        [HttpPost("deactivate")]
        public async Task<IActionResult> Deactivate([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return NotFound("User not found");

            // lock the account for 100 years
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            await _userManager.UpdateAsync(user);
            return Ok("Customer deactivated");
        }
    }
}

