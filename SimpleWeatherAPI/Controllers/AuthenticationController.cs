using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SimpleWeatherAPI.Configuration;
using SimpleWeatherAPI.Models;
using SimpleWeatherAPI.Models.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleWeatherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        //private readonly JwtConfig _jwtConfig;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            // _jwtConfig = jwtConfig;

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto userRegistrationRequestDto)
        {
            //Validate incoming Request
            try
            {
                if (ModelState.IsValid)
                {
                    //check if email exist
                    var user_email = await _userManager.FindByEmailAsync(userRegistrationRequestDto.Email);
                    if (user_email != null)
                    {
                        return BadRequest(new AuthenticationResult()
                        {
                            Result = false,
                            Errors = new List<string>()
                        {
                            "Email already exist"
                        }
                        });
                    }
                    //create User
                    var new_user = new IdentityUser()
                    {
                        Email = userRegistrationRequestDto.Email,
                        UserName = userRegistrationRequestDto.Email
                    };

                    var is_created = await _userManager.CreateAsync(new_user, userRegistrationRequestDto.Password);
                    if (is_created.Succeeded)
                    {
                        //generate token
                        var token = GenerateJwtToken(new_user);
                        return Ok(new AuthenticationResult()
                        {
                            Token = token,
                            Result = true

                        });
                    }
                    return BadRequest(new AuthenticationResult()
                    {
                        Errors = new List<string>()
                        {
                          "Server error"
                        },
                                Result = false
                    });
                }
            }
            catch(Exception)
            {
                throw;
            }

            return BadRequest();
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequestDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Check if User exist
                    var existing_user = await _userManager.FindByEmailAsync(loginRequestDto.Email);
                    if (existing_user == null)
                        return BadRequest(new AuthenticationResult()
                        {
                            Errors = new List<string>()
                            {
                               "Invalid Email"
                            },
                            Result= false
                        });

                    var isCorrect = await _userManager.CheckPasswordAsync(existing_user, loginRequestDto.Password);
                    if (!isCorrect)
                        return BadRequest(new AuthenticationResult()
                        {
                            Errors = new List<string>()
                            {
                                "Invalid Password,Password must contain a Uppercase,SpecialKey & Number"
                            },
                            Result = false
                        });

                    var jwtToken = GenerateJwtToken(existing_user);
                    return Ok(new AuthenticationResult()
                    {
                        Token = jwtToken,
                        Result = true
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
            return BadRequest(new AuthenticationResult()
            {
                Errors = new List<string>()
                {
                    "Invalid Payload"
                },
                Result = false
            });
        }
        private string GenerateJwtToken(IdentityUser user)
        {
            var claims = new List<Claim>
             {
             new Claim(ClaimTypes.Name, user.UserName),
             new Claim(ClaimTypes.NameIdentifier, user.Id),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
             };
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Secret"]));
            JwtSecurityToken token = new JwtSecurityToken(
             issuer: _configuration["JwtConfig:Issuer"],
             audience: _configuration["JwtConfig:Audience"],
             expires: DateTime.Now.AddHours(5),
             claims: claims,
             signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
             );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
