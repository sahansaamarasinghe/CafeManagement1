using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.DTOs;
using WebApplication2.Interfaces;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task PlaceOrderAsync(string userId, OrderRequestDTO dto)
        {
            var order = new Order
            {
                UserId = userId,
                OrderItems = new List<OrderItem>(),
                OrderDate = DateTime.UtcNow
            };

            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var food = await _context.FoodItems.FindAsync(item.FoodItemId);
                if (food == null) throw new Exception("Invalid Food Item");

                total += food.Price * item.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    FoodItemId = food.Id,
                    Quantity = item.Quantity
                });
            }

            order.TotalAmount = total;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OrderResponseDTO>> GetMyOrdersAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.FoodItem)
                .Select(o => new OrderResponseDTO
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => $"{oi.FoodItem.Name} x {oi.Quantity}").ToList()
                })
                .ToListAsync();
        }
    }

}
