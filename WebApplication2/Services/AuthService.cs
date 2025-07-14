using Microsoft.AspNetCore.Identity;
using WebApplication2.DTOs;
using WebApplication2.Helpers;
using WebApplication2.Interfaces;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           RoleManager<IdentityRole> roleManager,
                           IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            // role is always Customer
            const string role = RoleConstants.Customer;

            // email must be unique
            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                throw new InvalidOperationException("User already exists.");

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.UserName,
                FullName = dto.FullName
            };

            var create = await _userManager.CreateAsync(user, dto.Password);
            if (!create.Succeeded)
            {
                var detail = string.Join(", ", create.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {detail}");
            }

            // ensure Customer role exists, then add user to it
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);

            var token = JwtTokenGenerator.GenerateToken(user, new[] { role }, _config);

            return new AuthResponseDTO
            {
                Username = user.UserName,
                Token = token,
                Roles = new List<string> { role }
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidOperationException("Email and password are required.");

            var user = await _userManager.FindByEmailAsync(dto.Email);

            //invalid email
            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");   

            //confirm email
            //if (!user.EmailConfirmed)
            //    throw new InvalidOperationException("Please confirm your e-mail address first.");

            //unlocked account
            if (await _userManager.IsLockedOutAsync(user))
                throw new InvalidOperationException("Account is locked.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = JwtTokenGenerator.GenerateToken(user, roles, _config);

            return new AuthResponseDTO
            {
                Username = user.UserName,
                Token = token,
                Roles = roles
            };
        }



        public async Task ChangePasswordAsync(string userId, ChangePasswordDTO dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new InvalidOperationException("Passwords do not match.");

            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found.");

            var result = await _userManager.ChangePasswordAsync(
                            user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var detail = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(detail);
            }
        }
    }
}


