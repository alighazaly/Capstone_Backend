using Capstone_Backend.Data;
using Capstone_Backend.Models;
using Capstone_Backend.Repositories.RegistrationRepository;
using Capstone_Backend.Repositories.UserRepository;
using Capstone_Server.Models;

namespace Capstone_Backend.Repositories.AuthenticationRepository
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;

        public AuthenticationRepository(IRegistrationRepository registrationRepository, AppDbContext context)
        {
            _registrationRepository = registrationRepository;
            _context = context;
        }

        public async Task<Response<UserDTO>> Login(LoginModel model, string host)
        {
            var existingUser = await _registrationRepository.GetUserByUsernameAsync(model.Username);
            if (existingUser != null)
            {
                // Check if the provided password matches the stored hashed password
                if (BCrypt.Net.BCrypt.Verify(model.Password, existingUser.Password))
                {
                    // Password is correct, return success response with user data
                    var userDTO = new UserDTO
                    {
                        Id = existingUser.Id,
                        UserName = existingUser.UserName,
                        FirstName = existingUser.FirstName,
                        LastName = existingUser.LastName,
                        Email = existingUser.Email,
                        Role = existingUser.Role,
                        ProfilePicture = existingUser.ProfilePicture,
                        //ImageSrc = $"https://{host}/ProfileImages/{existingUser.ProfilePicture}" // Use the provided host parameter
                        ImageSrc = existingUser.ProfilePicture != null ? $"https://{host}/ProfileImages/{existingUser.ProfilePicture}" : null
                    };
                    return new Response<UserDTO>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        StatusMessage = $"Welcome back {existingUser.UserName}",
                        Data = userDTO
                    };
                }
                else
                {
                    // Password is incorrect, return unauthorized response
                    return new Response<UserDTO>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        StatusMessage = "Incorrect password"
                    };
                }
            }
            else
            {
                // User not found, return not found response
                return new Response<UserDTO>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    StatusMessage = "User not found"
                };
            }

        }
    }
}
