namespace Authentication.Models
{
    public class AuthenticatedResponseDto
    {

        public string? jwtToken { get; set; }
        public string? RefreshToken { get; set; }
        public string[] UserRoleType { get; set; }

    }
}
