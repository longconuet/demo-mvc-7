using System.ComponentModel.DataAnnotations;

namespace WebApplication7.Requests
{
    public class LoginRequest
    {
        [Required]
        [Display(Name = "Email / Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
