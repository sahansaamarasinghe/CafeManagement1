using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.DTOs;
using WebApplication2.Interfaces;
using Microsoft.Extensions.Configuration;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly int _minQty;
        private readonly int _maxQty;

        public OrderService(AppDbContext context, IConfiguration cfg)
        {
            _context = context;

            _minQty = int.TryParse(cfg["OrderLimits:MinQuantity"], out var min) ? min : 1;
            _maxQty = int.TryParse(cfg["OrderLimits:MaxQuantity"], out var max) ? max : 50;

        }


        public async Task PlaceOrderAsync(string userId, OrderRequestDTO dto)
        {

            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("User ID is null. Ensure the user is authenticated.");

            if (dto.Items is null || dto.Items.Count == 0)
                throw new InvalidOperationException("The order must contain at least one item.");

            var seenIds = new HashSet<int>();
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            decimal grandTotal = 0m;

            foreach (var item in dto.Items)
            {
                // Quantity has to be between 1-50
                if (item.Quantity < _minQty || item.Quantity > _maxQty)
                    throw new InvalidOperationException(
                        $"Quantity for foodItemId={item.FoodItemId} must be between {_minQty} and {_maxQty}.");

                // No duplicate FoodItemId
                if (!seenIds.Add(item.FoodItemId))
                    throw new InvalidOperationException(
                        $"Duplicate foodItemId={item.FoodItemId} detected in the order.");

                // Food item must exist
                var food = await _context.FoodItems.FindAsync(item.FoodItemId);
                if (food is null)
                    throw new InvalidOperationException(
                        $"Food item {item.FoodItemId} does not exist.");

                // Add to order
                decimal lineTotal = food.Price * item.Quantity;
                grandTotal += lineTotal;

                order.OrderItems.Add(new OrderItem
                {
                    FoodItemId = food.Id,
                    Quantity = item.Quantity,
                    TotalPrice = lineTotal          
                });
            }

            order.TotalAmount = grandTotal;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OrderResponseDTO>> GetMyOrdersAsync(string userId)
        {
            // 0️⃣  Guard – token must have a user-id
            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("User ID is missing; are you authenticated?");

            // 1️⃣  Query the customer’s orders
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                .OrderByDescending(o => o.OrderDate)           
                .Select(o => new OrderResponseDTO
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems
                                      .Select(oi => $"{oi.FoodItem.Name} x{oi.Quantity}")
                                      .ToList()
                })
                .ToListAsync();

            
            if (orders.Count == 0)
                throw new KeyNotFoundException("No orders found for this user.");

            return orders;
        }


        public async Task<List<OrderResponseDTO>> GetOrdersForAdminAsync(OrderFilterDTO f)
        {
            if (f.From.HasValue && f.To.HasValue && f.From > f.To)
                throw new InvalidOperationException("`from` date cannot be after `to` date.");

            if (f.MinTotal.HasValue && f.MaxTotal.HasValue && f.MinTotal > f.MaxTotal)
                throw new InvalidOperationException("`minTotal` cannot be greater than `maxTotal`.");

            var q = _context.Orders
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.FoodItem)
                            .AsQueryable();

            // Date filters 
            if (f.From.HasValue)
                q = q.Where(o => o.OrderDate >= f.From.Value);

            if (f.To.HasValue)
                q = q.Where(o => o.OrderDate <= f.To.Value);

            //Customer e-mail filter 
            if (!string.IsNullOrWhiteSpace(f.Email))
            {
                // Resolve the user once to avoid N+1
                var user = await _context.Users
                                         .SingleOrDefaultAsync(u => u.Email == f.Email);
                if (user is null)
                    throw new KeyNotFoundException("No customer with that e-mail.");

                q = q.Where(o => o.UserId == user.Id);
            }

            // Total filters
            if (f.MinTotal.HasValue)
                q = q.Where(o => o.TotalAmount >= f.MinTotal.Value);

            if (f.MaxTotal.HasValue)
                q = q.Where(o => o.TotalAmount <= f.MaxTotal.Value);

            // Sort & project
            var orders = await q.OrderByDescending(o => o.OrderDate)
                                .Select(o => new OrderResponseDTO
                                {
                                    OrderId = o.Id,
                                    OrderDate = o.OrderDate,
                                    TotalAmount = o.TotalAmount,
                                    Items = o.OrderItems
                                                   .Select(oi => $"{oi.FoodItem.Name} x{oi.Quantity}")
                                                   .ToList()
                                })
                                .ToListAsync();

            if (orders.Count == 0)
                throw new KeyNotFoundException("No orders match the specified filters.");

            return orders;
        }

    }

}
