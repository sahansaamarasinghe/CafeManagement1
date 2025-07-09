using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.DTOs;
using WebApplication2.Helpers;
using WebApplication2.Interfaces;
using WebApplication2.Models;

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
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDTO dto)
        {
            //var email = "john@example.com";
            //var testing = _userManager.FindByEmailAsync(email);
            var userId = _userManager.GetUserId(User); // 

            await _orderService.PlaceOrderAsync(userId, dto);
            return Ok("Order placed successfully");
        }

        [HttpGet("my")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }
    }

}
