using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Auth
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public LoginResponse(string token)
        {
            AccessToken = token;
        }

        public string AccessToken { get; set; }
    }
}
