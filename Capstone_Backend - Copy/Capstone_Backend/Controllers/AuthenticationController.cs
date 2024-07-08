using Capstone_Backend.Data;
using Capstone_Backend.Models;
using Capstone_Backend.Repositories.AuthenticationRepository;
using Capstone_Backend.Repositories.RegistrationRepository;
using Capstone_Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
   
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly AppDbContext _context;

        public AuthenticationController(IRegistrationRepository registrationRepository, IAuthenticationRepository authenticationRepository, AppDbContext context)
        {
            _registrationRepository = registrationRepository;
            _authenticationRepository = authenticationRepository;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var existingUser = await _registrationRepository.GetUserByUsernameAsync(model.UserName);
                if(existingUser != null)
                {
                    Response<RegistrationModel> response = new Response<RegistrationModel>
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        StatusMessage = "Username already exists",
                        Data = null
                    };
                    return Conflict(response);
                }
                await _registrationRepository.CreateUserAsync(model);
                await _context.SaveChangesAsync();
                return Ok(new Response<RegistrationModel>
                {
                    StatusCode = StatusCodes.Status200OK,
                    StatusMessage = "User registered successfully",
                    Data = model
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response<RegistrationModel>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    StatusMessage = "Internal server error",
                    Data = null
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var loginResponse = await _authenticationRepository.Login(model, Request.Host.ToString());

                if (loginResponse.StatusCode == StatusCodes.Status200OK)
                {
                    return Ok(loginResponse);
                }
                else
                {
                    return Unauthorized(loginResponse);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response<AppUser>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    StatusMessage = "Internal server error",
                    Data = null
                });
            }
        }
    }
}
