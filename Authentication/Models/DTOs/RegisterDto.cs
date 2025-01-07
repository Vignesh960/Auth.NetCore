using System.ComponentModel.DataAnnotations;

namespace Authentication.Models.DTOs
{

    public class RegisterUserDto
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string[] Roles { get; set; }
    }

}
