// Services/UserService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using WebApplication2.DTOs;
using WebApplication2.Helpers;
using WebApplication2.Models;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;

    public UserService(UserManager<ApplicationUser> um,
                       IEmailSender email,
                       IConfiguration cfg)
    {
        _um = um;
        _email = email;
        _cfg = cfg;
    }


    public async Task<IReadOnlyList<UserDetailsDTO>> GetAllAsync()
    {
        var list = new List<UserDetailsDTO>();
        foreach (var u in _um.Users)
        {
            var roles = await _um.GetRolesAsync(u);
            list.Add(new UserDetailsDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                UserName = u.UserName,
                Email = u.Email!,
                Roles = roles.ToList()
            });
        }
        return list;
    }

    public async Task<IReadOnlyList<object>> GetCustomersAsync()
    {
        var customers = await _um.GetUsersInRoleAsync(RoleConstants.Customer);
        return customers.Select(c => new { c.Id, c.Email, c.FullName }).ToList();
    }

    public async Task<UserDetailsDTO> GetByEmailAsync(string email)
    {
        var user = await _um.FindByEmailAsync(email)
                   ?? throw new KeyNotFoundException("User not found.");

        var roles = await _um.GetRolesAsync(user);
        return new UserDetailsDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            UserName = user.UserName,
            Email = user.Email!,
            Roles = roles.ToList()
        };
    }

    public async Task InviteCustomerAsync(string email)
    {
        var user = await _um.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var create = await _um.CreateAsync(user);
            if (!create.Succeeded)
                throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));

            await _um.AddToRoleAsync(user, RoleConstants.Customer);
        }

        var token = await _um.GeneratePasswordResetTokenAsync(user);
        var link = BuildFrontEndLink(email, token);
        await _email.SendEmailAsync(email,
            "Set up your Café account",
            $"Hi! Click to choose your password: <a href=\"{link}\">Set password</a>");
    }

    public async Task ResetPasswordAsync(string email)
    {
        var user = await _um.FindByEmailAsync(email)
                   ?? throw new KeyNotFoundException("User not found.");

        var token = await _um.GeneratePasswordResetTokenAsync(user);
        var link = BuildFrontEndLink(email, token);

        await _email.SendEmailAsync(email,
            "Reset your Café password",
            $"Click to reset: <a href=\"{link}\">Reset password</a>");
    }

    public async Task UpdateByEmailAsync(string email, UpdateUserByEmailDTO dto)
    {
        var user = await _um.FindByEmailAsync(email)
                   ?? throw new KeyNotFoundException("Customer not found.");

        if (!string.IsNullOrWhiteSpace(dto.UserName)) user.UserName = dto.UserName;
        if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
        if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;

        var res = await _um.UpdateAsync(user);
        if (!res.Succeeded)
            throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
    }

    public async Task ActivateAsync(string email) => await SetLockout(email, enable: false);
    public async Task DeactivateAsync(string email) => await SetLockout(email, enable: true);



    private async Task SetLockout(string email, bool enable)
    {
        var user = await _um.FindByEmailAsync(email)
                   ?? throw new KeyNotFoundException("User not found.");

        user.LockoutEnabled = enable;
        user.LockoutEnd = enable ? DateTimeOffset.UtcNow.AddYears(100) : null;

        await _um.UpdateAsync(user);
    }

    private string BuildFrontEndLink(string email, string token)
    {
        var baseUrl = _cfg["Frontend:BaseUrl"] ?? "https://localhost:3000";
        return $"{baseUrl}/set-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
    }
}
