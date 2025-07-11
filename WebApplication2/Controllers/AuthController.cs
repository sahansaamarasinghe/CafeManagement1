using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.DTOs;
using WebApplication2.Interfaces;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userMgr;

        public AuthController(IAuthService authService, UserManager<ApplicationUser> userMgr)
        {
            _authService = authService;
            
            _userMgr = userMgr;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var auth = await _authService.RegisterAsync(dto);  
            return Ok(new { statusCode = 200, data = auth });
        }


   
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            
            if (!ModelState.IsValid)
                return BadRequest(new { statusCode = 400, message = "Invalid input." });

            var auth = await _authService.LoginAsync(dto);  // may throw → middleware
            return Ok(new { statusCode = 200, data = auth });
        }


      
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // For JWT, the client simply discards the token
            return Ok(new { statusCode = 200, message = "Logout successful. Remove the token on the client." });
        }



        [HttpPost("change-password")]
        public async Task<IActionResult> ChangeMyPassword(
        [FromBody] ChangePasswordDTO dto)
        {
            var userId = _userMgr.GetUserId(User)!;                
            await _authService.ChangePasswordAsync(userId, dto);   

            return Ok(new { statusCode = 200, message = "Password changed." });
        }

    }
}
