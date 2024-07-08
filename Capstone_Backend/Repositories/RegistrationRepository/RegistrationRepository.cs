using Capstone_Backend.Data;
using Capstone_Backend.Models;
using Capstone_Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capstone_Backend.Repositories.RegistrationRepository
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly AppDbContext _context;

        public RegistrationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(RegistrationModel model)
        {
            AppUser newUser = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                Role = "Customer"
            };
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            newUser.ConfirmPassword = BCrypt.Net.BCrypt.HashPassword(model.ConfirmPassword);
            // Create a new WishList instance for the user
            // Add the new user to the context
            _context.ApplicationUsers.Add(newUser);

            // Save changes to the database to generate the user ID
            await _context.SaveChangesAsync();

            // Create a new WishList instance for the user
            WishList newUserWishList = new WishList();

            // Set the UserId of the wishlist to the ID of the new user
            newUserWishList.UserId = newUser.Id;

            // Add the new wishlist to the context
            _context.WishList.Add(newUserWishList);

            // Save changes to the database to generate the wishlist ID
            await _context.SaveChangesAsync();

            // Set the WishListId of the user to the ID of the new wishlist
            newUser.WishListId = newUserWishList.Id;

            // Save changes to update the user with the wishlist ID
            await _context.SaveChangesAsync();
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            return await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == email);
        }
    }
}
