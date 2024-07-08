using Capstone_Backend.Models;
using Capstone_Server.Models;

namespace Capstone_Backend.Repositories.AuthenticationRepository
{
    public interface IAuthenticationRepository
    {
        Task<Response<UserDTO>> Login(LoginModel loginModel, string host);
    }
}
