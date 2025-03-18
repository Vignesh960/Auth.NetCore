using Authentication.dbContext;
using Authentication.Models;
using Authentication.Models.DTOs;
using Authentication.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ITokenRepository tokenRepository;

        public AuthController(UserManager<ApplicationUser> userManager, ITokenRepository tokenRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerRequestDto)
        {
            var userExistence = await userManager.FindByEmailAsync(registerRequestDto.Username);
            if (userExistence != null)
            {
                return Conflict(new ResponseDto { statusCode = StatusCodes.Status403Forbidden, Message = "User with this E-mail already exists. Please try with diffrerent E-mail" });
            }
            ApplicationUser applicationUser = new ApplicationUser()
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Username,
                FirstName = registerRequestDto.Firstname,
                LastName = registerRequestDto.Lastname,
                LastLoginTime = null
            };
            var identityResult = await userManager.CreateAsync(applicationUser, registerRequestDto.Password);
            if (identityResult.Succeeded)
            {
                if (registerRequestDto.Roles != null && registerRequestDto.Roles.Any())
                {
                    identityResult = await userManager.AddToRolesAsync(applicationUser, registerRequestDto.Roles);
                    if (identityResult.Succeeded)
                    {
                        return Ok("User registered successfully! Please login.");
                    }
                    else
                    {
                        return BadRequest("User registration succeeded, but assigning roles failed.");
                    }
                }
                return Ok("User registered successfully! Please login.");
            }
            else
            {
                return BadRequest("User registration failed: " + string.Join(", ", identityResult.Errors.Select(e => e.Description)));
            }
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            string currentUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(loginRequestDto.Username);
                if (user != null)
                {
                    if (!user.isActive)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new ResponseDto { statusCode = StatusCodes.Status403Forbidden, Message = "User is inactive. Please Contact your Admin" });
                    }
                    bool passwordVerified = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);
                    if (passwordVerified)
                    {
                        //get the roles
                        var userRole = await userManager.GetRolesAsync(user);

                        // var lastLoginTime = user.LastLoginTime ?? DateTime.UtcNow;

                        //generate token
                        var accesToken = tokenRepository.GenerateJWTToken(user, userRole.ToList());
                        //await userManager.SetAuthenticationTokenAsync(user, $"{user.Email}", "jwtAccess", accesToken);
                        //var refreshToken = tokenRepository.GenerateRefreshToken();
                        //await userManager.SetAuthenticationTokenAsync(user, $"{user.Email}", "RefreshToken", refreshToken);
                        //asign to responseDto
                        var userinfo = await userManager.Users.Where(user => user.Email == loginRequestDto.Username).Select(user => new { user.FirstName, user.LastName, user.Email, user.isActive }).ToListAsync();
                        //LoginResponseDto loginResponseDto = new LoginResponseDto() { Username = loginRequestDto.Username, RoleType = userRole.ToArray(), JwtToken = token, success = true, isactive = true };
                        return Ok(new { jwtToken = accesToken, UserRoleType = userRole.ToArray(), userinfo });
                    }
                    return Unauthorized(new
                    {
                        message = "Invalid UserName or Password",
                        StatusCode = StatusCodes.Status404NotFound
                    });

                }
            }
            return BadRequest(ModelState);

        }

        [HttpPost]
        [Route("ChangeRole")]
        [Authorize]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound(new ResponseDto { statusCode = StatusCodes.Status404NotFound, Message = $"User not found with user id {model.UserId}" });
            }
            var currentRoles = await userManager.GetRolesAsync(user);
            var removeRolesResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRolesResult.Succeeded)
            {
                return BadRequest("Failed to remove current roles");
            }
            var addRoleResult = await userManager.AddToRoleAsync(user, model.NewRoleType);
            if (!addRoleResult.Succeeded)
            {
                return BadRequest(new ResponseDto { statusCode = StatusCodes.Status500InternalServerError, Message = "Failed to add new role" });
            }
            return Ok("Role changed successfully");
        }


        [HttpGet]
        [Route("users")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Manager")]

        public async Task<IActionResult> GetUsers()
        {
            var users = await userManager.Users.ToListAsync();
            return Ok(users);
        }
        [HttpGet]
        [Route("GetUserwithRoles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserRoles()
        {
            var users = await userManager.Users.ToListAsync();
            var usersWithRoles = new List<UserWithRoles>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRoles { UserId = user.Id, UserName = user.UserName, Roles = roles.ToList() });
            }
            return Ok(usersWithRoles);
        }
        [HttpDelete("RemoveAllUsers")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Manager")]
        public async Task<IActionResult> RemoveAllUsers()
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = await userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                if (user.Id == currentUser)
                    continue;
                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest($"Failed to delete user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            return Ok(new ResponseDto { statusCode = StatusCodes.Status200OK, Message = "All users have been removed" });
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Manager")]
        [Route("GetuserInfo")]
        public async Task<IActionResult> GetuserInfo()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Unauthorized("User not authenticated");
            }
            var claims = identity.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(
                new
                {
                    username = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    Email = identity.FindFirst(ClaimTypes.Email)?.Value,
                    Role = identity.FindFirst(ClaimTypes.Role)?.Value,
                    Claims = claims
                });
        }
    }
}
