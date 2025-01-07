using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Authentication.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [DefaultValue("admin@admin.com")]
        public string Username { get; set; }
        [Required]
        [DefaultValue("admin@12345")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
