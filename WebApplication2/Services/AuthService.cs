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


        //public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        //{
        //    // 1️⃣  Figure out which role we’ll assign
        //    var roleToAssign = string.IsNullOrWhiteSpace(dto.Role)
        //                       ? RoleConstants.Customer
        //                       : dto.Role.Trim();

        //    if (!RoleConstants.All.Contains(roleToAssign))
        //        throw new Exception("Invalid role.");

        //    if (roleToAssign == RoleConstants.Admin)             // self-registration guard
        //        throw new Exception("You are not allowed to create an Admin account.");

        //    // 2️⃣  Ensure the email isn’t already taken
        //    if (await _userManager.FindByEmailAsync(dto.Email) is not null)
        //        throw new Exception("User already exists.");

        //    // 3️⃣  Create the ApplicationUser object
        //    var user = new ApplicationUser
        //    {
        //        Email = dto.Email,
        //        UserName = dto.UserName,
        //        FullName = dto.FullName
        //    };

        //    // 4️⃣  Persist it with the supplied password
        //    var createResult = await _userManager.CreateAsync(user, dto.Password);
        //    if (!createResult.Succeeded)
        //    {
        //        var error = string.Join(", ", createResult.Errors.Select(e => e.Description));
        //        throw new Exception($"Registration failed: {error}");
        //    }

        //    // 5️⃣  Ensure the role exists, then add the user to it
        //    if (!await _roleManager.RoleExistsAsync(roleToAssign))
        //        await _roleManager.CreateAsync(new IdentityRole(roleToAssign));

        //    await _userManager.AddToRoleAsync(user, roleToAssign);

        //    // 6️⃣  Build JWT + response
        //    var roles = await _userManager.GetRolesAsync(user);
        //    var token = JwtTokenGenerator.GenerateToken(user, roles, _config);

        //    return new AuthResponseDTO
        //    {
        //        Username = user.UserName,
        //        Token = token,
        //        Roles = roles
        //    };
        //}


        //public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        //{
        //    var user = await _userManager.FindByEmailAsync(dto.Email);
        //    if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        //        throw new Exception("Invalid email or password");

        //    var roles = await _userManager.GetRolesAsync(user);
        //    var token = JwtTokenGenerator.GenerateToken(user, roles, _config);

        //    return new AuthResponseDTO
        //    {
        //        Username = user.UserName,
        //        Token = token,
        //        Roles = roles
        //    };
        //}

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidOperationException("Email and password are required.");

            var user = await _userManager.FindByEmailAsync(dto.Email);

            //invalid email
            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");   

            //confirm email
            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Please confirm your e-mail address first.");

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


