using WebApplication2.DTOs;

namespace WebApplication2.Interfaces
{
    public interface IOrderService
    {
        Task PlaceOrderAsync(string userId, OrderRequestDTO dto);
        Task<List<OrderResponseDTO>> GetMyOrdersAsync(string userId);

        Task<List<OrderResponseDTO>> GetOrdersForAdminAsync(OrderFilterDTO filter);
    }

}
