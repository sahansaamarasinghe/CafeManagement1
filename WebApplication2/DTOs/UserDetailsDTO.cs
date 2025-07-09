namespace WebApplication2.DTOs
{
    public class UserDetailsDTO
    {
        
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }

    public class UpdateUserByEmailDTO
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }    // optional; if present will be set
    }
    public class AssignRoleDTO
    {
        public string Role { get; set; }
    }
}

