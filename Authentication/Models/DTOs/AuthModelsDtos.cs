using System.ComponentModel.DataAnnotations;

namespace Authentication.Models.DTOs
{
    public class AuthModelsDtos
    {
    }

    public class ChangeRoleModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string NewRoleType { get; set; }
    }
    public class UserWithRoles
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }
}
