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
        private readonly ILogger<OrderController> _logger;


        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _userManager = userManager;
            _logger       = logger;  

        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return Unauthorized(new { statusCode = 401, message = "Unauthorized" });

            await _orderService.PlaceOrderAsync(user.Id, dto);   // may throw → middleware handles

            return Ok(new { statusCode = 200, message = "Order placed successfully" });
        }



        //[Authorize(Policy = "CustomerOnly")]
        //[HttpPost]
        //public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDTO dto)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user is null) return Unauthorized();

        //    await _orderService.PlaceOrderAsync(user.Id, dto);   
        //    return Ok(new { message = "Order placed successfully" });
        //}

        //    public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDTO dto)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user is null) return Unauthorized();

        //    try
        //    {
        //        await _orderService.PlaceOrderAsync(user.Id, dto);
        //        return Ok("Order placed successfully");
        //    }
        //    catch (InvalidOperationException ex)           // validation / business rule
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (KeyNotFoundException ex)                // missing food item or alike
        //    {
        //        return NotFound(ex.Message);
        //    }
        //    catch (Exception ex)                           // anything you didn't expect
        //    {
        //        _logger.LogError(ex, "PlaceOrder failed");
        //        return StatusCode(500, "Unexpected server error.");
        //    }
        //}


        //var user = await _userManager.GetUserAsync(User);
        //if (user is null) return Unauthorized("User not found for this token");

        //// 2) OPTIONAL fallback: if null, use the email claim
        //if (user is null)
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email); // comes from the token
        //    if (email is null) return Unauthorized();

        //    user = await _userManager.FindByEmailAsync(email);
        //    if (user is null) return Unauthorized(); // still not found
        //}

        //await _orderService.PlaceOrderAsync(user.Id, dto);
        //    return Ok("Order placed successfully");
        //}

        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("myOrder")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        //[Authorize(Policy = "AdminOnly")]
        //[HttpGet("/api/orders")]
        //public async Task<IActionResult> ListOrders(
        //    [FromQuery] DateTime? from,
        //    [FromQuery] DateTime? to,
        //    [FromQuery] string? email,
        //    [FromQuery] decimal? minTotal,
        //    [FromQuery] decimal? maxTotal)
        //{
        //    var filter = new OrderFilterDTO
        //    {
        //        From = from,
        //        To = to,
        //        Email = email,
        //        MinTotal = minTotal,
        //        MaxTotal = maxTotal
        //    };

        //    try
        //    {
        //        var orders = await _orderService.GetOrdersForAdminAsync(filter);
        //        return Ok(orders);
        //    }
        //    catch (InvalidOperationException ex)    // bad input -> 400
        //    {
        //        return Ok(new { message:"Bad REquest"});
        //    }
        //    catch (KeyNotFoundException ex)         // nothing found -> 404
        //    {
        //        //return NotFound(ex.Message);
        //        return Ok(ex.Message);
        //    }
        //  ;
        //}

        // Admin list
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("/api/orders")]
        public async Task<IActionResult> ListOrders([FromQuery] DateTime? from,
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
