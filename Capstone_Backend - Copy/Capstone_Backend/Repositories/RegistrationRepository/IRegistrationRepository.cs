using Capstone_Backend.Models;
using Capstone_Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Capstone_Backend.Repositories.RegistrationRepository
{
    public interface IRegistrationRepository
    {
        Task CreateUserAsync(RegistrationModel model);
        Task <AppUser> GetUserByUsernameAsync(string username);
    }
}
