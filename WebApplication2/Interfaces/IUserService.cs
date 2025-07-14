// Interfaces/IUserService.cs
using WebApplication2.DTOs;

public interface IUserService
{
    Task<IReadOnlyList<UserDetailsDTO>> GetAllAsync();
    Task<IReadOnlyList<object>> GetCustomersAsync();
    Task<UserDetailsDTO> GetByEmailAsync(string email);
    Task InviteCustomerAsync(string email);
    Task ResetPasswordAsync(string email);
    Task UpdateByEmailAsync(string email, UpdateUserByEmailDTO dto);
    Task ActivateAsync(string email);
    Task DeactivateAsync(string email);
}
