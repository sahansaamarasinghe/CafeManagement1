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



        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("myOrder")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

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
