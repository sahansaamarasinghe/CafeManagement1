namespace WebApplication2.DTOs
{
  
        public class AuthResponseDTO
        {
            public string Username { get; set; }
            public string Token { get; set; }
            public IList<string> Roles { get; set; }
        }
    

}
