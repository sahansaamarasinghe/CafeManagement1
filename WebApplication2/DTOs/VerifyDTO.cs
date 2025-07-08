using System.ComponentModel.DataAnnotations;

namespace WebApplication2.DTOs
{
    public class VerifyDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
