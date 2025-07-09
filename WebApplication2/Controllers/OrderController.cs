using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.DTOs;
using WebApplication2.Helpers;
using WebApplication2.Interfaces;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    //[Authorize(Roles = "Customer")]
    //  [Authorize]
    //[Authorize(Roles = RoleConstants.Customer)]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;


        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        //[HttpPost]
        //public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDTO dto)
        //{
        //    var userId = _userManager.GetUserId(User);
        //    await _orderService.PlaceOrderAsync(userId, dto);
        //    return Ok("Order placed successfully");
        //}

        //  [Authorize(Roles = RoleConstants.Customer)]
        [Authorize(Policy = "CustomerOnly")]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDTO dto)
        {
            ///COORECT

            //var email = "lily@example.com";


            //var user = await _userManager.FindByEmailAsync(email);
            //if (user is null) return NotFound("User not found");

            //await _orderService.PlaceOrderAsync(user.Id, dto);
            //return Ok("Order placed successfully");
            ///CORRECT
            ///
            //var user = await _userManager.GetUserAsync(User);
            //if (user is null) return Unauthorized();          // should never happen with a good token

            //await _orderService.PlaceOrderAsync(user.Id, dto);  // user.Id is the GUID string
            //return Ok("Order placed successfully");


            
       
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not found for this token");

            // 2) OPTIONAL fallback: if null, use the email claim
            if (user is null)
            {
                var email = User.FindFirstValue(ClaimTypes.Email); // comes from the token
                if (email is null) return Unauthorized();

                user = await _userManager.FindByEmailAsync(email);
                if (user is null) return Unauthorized(); // still not found
            }

            await _orderService.PlaceOrderAsync(user.Id, dto);
            return Ok("Order placed successfully");
        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("my")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("/api/orders")]
        public async Task<IActionResult> ListOrders(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? email,
            [FromQuery] decimal? minTotal,
            [FromQuery] decimal? maxTotal)
        {
            var filter = new OrderFilterDTO
            {
                From = from,
                To = to,
                Email = email,
                MinTotal = minTotal,
                MaxTotal = maxTotal
            };

            var orders = await _orderService.GetOrdersForAdminAsync(filter);
            return Ok(orders);
        }


    }

}
