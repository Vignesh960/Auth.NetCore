using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.dbContext
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [DefaultValue(true)]
        public bool isActive { get; set; } = true;
        [NotMapped]
        private static readonly TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); 
        public DateTime CreatedAt { get; set; }= TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
        public DateTime? LastLoginTime { get; set; }
        public string? JwtToken { get; set; }
        public string? RefreshToken { get; set; }
    }
    public class authDbContext : IdentityDbContext<ApplicationUser>
    {
        public authDbContext(DbContextOptions<authDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            string adminRoleId = "02ded42b-ff51-4578-91dc-a9b3ce7df96a";
            string managerRoleId = "4913c669-8788-4a1e-b045-90c7ef93ae47";
            string userRoleId = "bd03a410-f630-4dc8-a242-4dc8cdec330b";
            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                    {
                        Id=adminRoleId,
                        ConcurrencyStamp=adminRoleId,
                        Name="Admin",
                        NormalizedName="Admin".ToUpper(),
                    },
                new IdentityRole
                    {
                        Id=managerRoleId,
                        ConcurrencyStamp=managerRoleId,
                        Name="Manager",
                        NormalizedName="Manager".ToUpper(),
                    },
                new IdentityRole
                    {
                        Id=userRoleId,
                        ConcurrencyStamp=userRoleId,
                        Name="User",
                        NormalizedName="User".ToUpper(),
                    },
            };

            builder.Entity<IdentityRole>().HasData(roles);


        }
    }
}
