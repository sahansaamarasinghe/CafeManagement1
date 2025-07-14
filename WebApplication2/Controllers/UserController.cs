using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.DTOs;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _svc;
    public UserController(IUserService svc) => _svc = svc;

    [HttpGet("all users")]
    public async Task<IActionResult> GetAll()
        => Ok(new { statusCode = 200, data = await _svc.GetAllAsync() });

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
        => Ok(new { statusCode = 200, data = await _svc.GetCustomersAsync() });

    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email)
        => Ok(new { statusCode = 200, data = await _svc.GetByEmailAsync(email) });

    [HttpPut("by-email")]
    public async Task<IActionResult> UpdateByEmail(
        [FromQuery] string email,
        [FromBody] UpdateUserByEmailDTO dto)
    {
        await _svc.UpdateByEmailAsync(email, dto);
        return Ok(new { statusCode = 200, message = "Customer updated" });
    }

    [HttpPost("invite")]
    public async Task<IActionResult> Invite([FromQuery] string email)
    {
        await _svc.InviteCustomerAsync(email);
        return Ok(new { statusCode = 200, message = "Invitation sent" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPwd([FromQuery] string email)
    {
        await _svc.ResetPasswordAsync(email);
        return Ok(new { statusCode = 200, message = "Password-reset email sent" });
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromQuery] string email)
    {
        await _svc.ActivateAsync(email);
        return Ok(new { statusCode = 200, message = "Customer activated" });
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> Deactivate([FromQuery] string email)
    {
        await _svc.DeactivateAsync(email);
        return Ok(new { statusCode = 200, message = "Customer deactivated" });
    }
}
