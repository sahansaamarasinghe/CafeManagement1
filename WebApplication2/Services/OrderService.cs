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

        //public async Task PlaceOrderAsync(string userId, OrderRequestDTO dto)
        //{
        //    var order = new Order
        //    {
        //        UserId = userId,
        //        OrderItems = new List<OrderItem>(),
        //        OrderDate = DateTime.UtcNow
        //    };

        //    decimal total = 0;

        //    foreach (var item in dto.Items)
        //    {
        //        var food = await _context.FoodItems.FindAsync(item.FoodItemId);
        //        if (food == null) throw new Exception("Invalid Food Item");

        //        total += food.Price * item.Quantity;

        //        order.OrderItems.Add(new OrderItem
        //        {
        //            FoodItemId = food.Id,
        //            Quantity = item.Quantity
        //        });
        //    }

        //    order.TotalAmount = total;
        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();
        //}

        public async Task PlaceOrderAsync(string userId, OrderRequestDTO dto)
        {
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User ID is null. Ensure the user is authenticated.");

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
                if (food == null) throw new Exception($"Invalid Food Item ID: {item.FoodItemId}");

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

        public async Task<List<OrderResponseDTO>> GetOrdersForAdminAsync(OrderFilterDTO f)
        {
            // 1️⃣  start with the whole table
            var q = _context.Orders
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.FoodItem)
                            .AsQueryable();

            // 2️⃣  apply filters if provided
            if (f.From.HasValue)
                q = q.Where(o => o.OrderDate >= f.From.Value);

            if (f.To.HasValue)
                q = q.Where(o => o.OrderDate <= f.To.Value);

            if (!string.IsNullOrWhiteSpace(f.Email))
                q = q.Where(o => o.User.Email == f.Email);   // needs .Include(o => o.User) OR navigation property

            if (f.MinTotal.HasValue)
                q = q.Where(o => o.TotalAmount >= f.MinTotal.Value);

            if (f.MaxTotal.HasValue)
                q = q.Where(o => o.TotalAmount <= f.MaxTotal.Value);

            // 3️⃣  (optional) sort newest first
            q = q.OrderByDescending(o => o.OrderDate);

            // 4️⃣  project to DTO and execute
            return await q.Select(o => new OrderResponseDTO
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Items = o.OrderItems
                                        .Select(oi => $"{oi.FoodItem.Name} x{oi.Quantity}")
                                        .ToList()
            })
                    .ToListAsync();
        }

    }

}
