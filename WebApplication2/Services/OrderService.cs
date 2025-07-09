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
            // ------ 0. Basic guards ------------------------------------------
            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("User ID is null. Ensure the user is authenticated.");

            if (dto.Items is null || dto.Items.Count == 0)
                throw new InvalidOperationException("The order must contain at least one item.");

            // ------ 1. Validate & accumulate ---------------------------------
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
                if (item.Quantity < 1 || item.Quantity > 50)
                    throw new InvalidOperationException(
                        $"Quantity for foodItemId={item.FoodItemId} must be between 1 and 50.");

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
                    TotalPrice = lineTotal          // stores per-item total
                });
            }

            // ------ 2. Persist -----------------------------------------------
            order.TotalAmount = grandTotal;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        //public async Task PlaceOrderAsync(string userId, OrderRequestDTO dto)
        //{
        //    if (string.IsNullOrEmpty(userId))
        //        throw new Exception("User ID is null. Ensure the user is authenticated.");

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
        //        if (food == null) throw new Exception($"Invalid Food Item ID: {item.FoodItemId}");

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
                .OrderByDescending(o => o.OrderDate)           // newest first (optional)
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
        //public async Task<List<OrderResponseDTO>> GetMyOrdersAsync(string userId)
        //{
        //    return await _context.Orders
        //        .Where(o => o.UserId == userId)
        //        .Include(o => o.OrderItems).ThenInclude(oi => oi.FoodItem)
        //        .Select(o => new OrderResponseDTO
        //        {
        //            OrderId = o.Id,
        //            OrderDate = o.OrderDate,
        //            TotalAmount = o.TotalAmount,
        //            Items = o.OrderItems.Select(oi => $"{oi.FoodItem.Name} x {oi.Quantity}").ToList()
        //        })
        //        .ToListAsync();
        //}


        public async Task<List<OrderResponseDTO>> GetOrdersForAdminAsync(OrderFilterDTO f)
        {
            // ---------- 0. Guard against contradictory filters -----------------
            if (f.From.HasValue && f.To.HasValue && f.From > f.To)
                throw new InvalidOperationException("`from` date cannot be after `to` date.");

            if (f.MinTotal.HasValue && f.MaxTotal.HasValue && f.MinTotal > f.MaxTotal)
                throw new InvalidOperationException("`minTotal` cannot be greater than `maxTotal`.");

            // ---------- 1. Base query -----------------------------------------
            var q = _context.Orders
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.FoodItem)
                            .AsQueryable();

            // ---------- 2. Date filters ---------------------------------------
            if (f.From.HasValue)
                q = q.Where(o => o.OrderDate >= f.From.Value);

            if (f.To.HasValue)
                q = q.Where(o => o.OrderDate <= f.To.Value);

            // ---------- 3. Customer e-mail filter ------------------------------
            if (!string.IsNullOrWhiteSpace(f.Email))
            {
                // Resolve the user once to avoid N+1
                var user = await _context.Users
                                         .SingleOrDefaultAsync(u => u.Email == f.Email);
                if (user is null)
                    throw new KeyNotFoundException("No customer with that e-mail.");

                q = q.Where(o => o.UserId == user.Id);
            }

            // ---------- 4. Total filters --------------------------------------
            if (f.MinTotal.HasValue)
                q = q.Where(o => o.TotalAmount >= f.MinTotal.Value);

            if (f.MaxTotal.HasValue)
                q = q.Where(o => o.TotalAmount <= f.MaxTotal.Value);

            // ---------- 5. Sort & project -------------------------------------
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
        //public async Task<List<OrderResponseDTO>> GetOrdersForAdminAsync(OrderFilterDTO f)
        //{
        //    // 1️⃣  start with the whole table
        //    var q = _context.Orders
        //                    .Include(o => o.OrderItems)
        //                        .ThenInclude(oi => oi.FoodItem)
        //                    .AsQueryable();

        //    // 2️⃣  apply filters if provided
        //    if (f.From.HasValue)
        //        q = q.Where(o => o.OrderDate >= f.From.Value);

        //    if (f.To.HasValue)
        //        q = q.Where(o => o.OrderDate <= f.To.Value);

        //    if (!string.IsNullOrWhiteSpace(f.Email))
        //        q = q.Where(o => o.User.Email == f.Email);   // needs .Include(o => o.User) OR navigation property

        //    if (f.MinTotal.HasValue)
        //        q = q.Where(o => o.TotalAmount >= f.MinTotal.Value);

        //    if (f.MaxTotal.HasValue)
        //        q = q.Where(o => o.TotalAmount <= f.MaxTotal.Value);

        //    // 3️⃣  (optional) sort newest first
        //    q = q.OrderByDescending(o => o.OrderDate);

        //    // 4️⃣  project to DTO and execute
        //    return await q.Select(o => new OrderResponseDTO
        //    {
        //        OrderId = o.Id,
        //        OrderDate = o.OrderDate,
        //        TotalAmount = o.TotalAmount,
        //        Items = o.OrderItems
        //                                .Select(oi => $"{oi.FoodItem.Name} x{oi.Quantity}")
        //                                .ToList()
        //    })
        //            .ToListAsync();
        //}

    }

}
