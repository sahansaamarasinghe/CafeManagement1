using System.ComponentModel.DataAnnotations;

namespace WebApplication2.DTOs
{
    //public class ChangePasswordDTO
    //{
    //    [Required(ErrorMessage = "New Password ")]

    //    [StringLength(40, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 40 characters")]
    //    [DataType(DataType.Password)]
    //    public string NewPassword { get; set; }

    //    [Required(ErrorMessage = "Confirm New Password is required")]
    //    [Display(Name ="Confirm new Password")]
    //    [DataType(DataType.Password)]
    //    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    //    public string confirmNewPassword { get; set; }
    //}

    public class ChangePasswordDTO
    {
     

        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "New Password is required")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 40 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm New Password is required")]
        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; }
    }
}
