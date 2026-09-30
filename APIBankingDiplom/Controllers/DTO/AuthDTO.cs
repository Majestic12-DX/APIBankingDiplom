using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.Controllers.DTO
{
    public class RegisterDTO
    {
        [Required] public string name { get; set; } = string.Empty;
        [Required] public string password { get; set; } = string.Empty;
        [Required] public string email { get; set; } = string.Empty;
        [Required, Phone] public string phoneNumber { get; set; } = string.Empty;
    };

    public class LoginDTO
    {
        [Required] public string email { get; set; } = string.Empty;

        [Required] public string password { get; set; } = string.Empty;
    };

    public class UserInfoDTO
    {
        [Required] public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }

    public class UserInfoUpdateDTO
    {
        [Required] public string Password { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
        public string? Name { get; set; }
        [Phone] public string? PhoneNumber { get; set; }
    }

    public class UserEmailUpdateDTO
    {
        [Required] public string Password { get; set; } = string.Empty;
        [Required] public string Email { get; set; } = string.Empty;
    }
}
