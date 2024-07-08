using Capstone_Backend.Data;
using Capstone_Backend.Models;
using Capstone_Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Capstone_Backend.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;


        public UserRepository(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }



        public async Task<string> GetUserRoleAsync(string userId)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ApplicationException($"User with ID '{userId}' not found.");
            }

            // Assuming your user entity has a Role property
            return user.Role;
        }

        [NonAction]
        public async Task<string> SaveProfileImage(IFormFile formFile)
        {
            string imageName = new String(Path.GetFileNameWithoutExtension(formFile.Name).Take(10).ToArray());
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(formFile.FileName);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "ProfileImages", imageName);
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }
            return imageName;
        }
        [NonAction]
        public async Task<string> SaveAppartmentImage(IFormFile appartmentFile, int appartmentId)
        {
            string imageName = new String(Path.GetFileNameWithoutExtension(appartmentFile.Name).Take(10).ToArray());
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(appartmentFile.FileName);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "AppartmentsImages", imageName);
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await appartmentFile.CopyToAsync(fileStream);
            }

            // Associate the image with the appartment
            var Images = new Images
            {
                Url = imageName,
                AppartmentId = appartmentId
            };

            _context.Images.Add(Images);
            await _context.SaveChangesAsync();

            return imageName;
        }
        public async Task<Response<object>> EditProfile(string id, string email, string username, IFormFile? image)
        {
            Response<object> response = new Response<object>();
            try
            {
                // Find the user by ID
                var existingUser = await _context.ApplicationUsers.FindAsync(id);

                if (existingUser == null)
                {
                    // User with the provided ID not found
                    response.StatusCode = 404;
                    response.StatusMessage = "User not found";
                    return response;
                }

                // Validate email format
                if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Invalid email format!";
                    return response;
                }

                // Check if the new username is different from the current username
                if (existingUser.UserName != username)
                {
                    // Check if the new username already exists for other users
                    var userNameExist = await _context.ApplicationUsers
                        .Where(u => u.UserName == username && u.Id != id) // Exclude the current user with the specified id
                        .FirstOrDefaultAsync();

                    if (userNameExist != null)
                    {
                        // Log the user who already has the username
                        Console.WriteLine($"User with username {username} already exists for user ID {userNameExist.Id}");

                        response.StatusCode = 401;
                        response.StatusMessage = $"User with username {username} already exists for user ID {userNameExist.Id}";
                        return response;
                    }
                }



                // Check if the new email is different from the current email
                if (existingUser.Email != email)
                {
                    // Check if the new email already exists
                    var emailExist = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
                    if (emailExist != null)
                    {
                        response.StatusCode = 401;
                        response.StatusMessage = "Email already linked with another account!";
                        return response;
                    }
                }

                // Check if a new image file is provided and it's different from the current profile picture
                if (image != null && existingUser.ProfilePicture != image.FileName)
                {
                    // Save the new profile picture
                    existingUser.ProfilePicture = await SaveProfileImage(image);
                }

                // Update email if changed
                if (existingUser.Email != email)
                {
                    existingUser.Email = email;
                }

                // Update username if changed
                if (existingUser.UserName != username)
                {
                    existingUser.UserName = username;
                }

                // Save changes to the database if there are any
                if (_context.Entry(existingUser).State == EntityState.Modified)
                {
                    await _context.SaveChangesAsync();
                    response.StatusCode = 200;
                    response.StatusMessage = "Profile Updated Successfully";
                }
                else
                {
                    response.StatusCode = 199;
                    response.StatusMessage = "No changes submitted";
                }

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Internal server error
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";
                return response;
            }
        }

        private bool IsValidEmail(string email)
        {
            // Regular expression for basic email format validation
            string pattern = @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[\w-]{2,4}$";
            return Regex.IsMatch(email, pattern);
        }
        private async Task<Location> UploadLocation(double latitude, double longitude, string city, string country)
        {
            var location = new Location
            {
                Latitude = latitude,
                Longitude = longitude,
                City = city,
                Country = country
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return location;
        }
        public async Task<Category> GetCategoryByName(string categoryName)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
        }
        public async Task<List<string>> AddImagesUrls(List<IFormFile> imageFiles, int appartmentId)
        {
            var imageUrls = new List<string>();

            foreach (var imageFile in imageFiles)
            {
                string imageUrl = await SaveAppartmentImage(imageFile, appartmentId);
                imageUrls.Add(imageUrl);
            }

            return imageUrls;
        }
        public async Task<Response<AppartmentDTO>> UploadAppartment(AppartmentDTO appartment)
        {
            Response<AppartmentDTO> response = new Response<AppartmentDTO>();
            try
            {
                // Upload Location
                var location = await UploadLocation(appartment.Latitude, appartment.Longitude, appartment.City, appartment.Country);

                // Get or create Category
                var category = await GetCategoryByName(appartment.CategoryName);
                if (category == null)
                {
                    category = new Category { Name = appartment.CategoryName };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
                appartment.UploadDate = DateTime.Now;

                // Save Appartment
                var newAppartment = new Appartment
                {
                    Title = appartment.Title,
                    Description = appartment.Description,
                    Price = appartment.Price,
                    UploadDate = appartment.UploadDate,
                    NumberOfBedrooms = appartment.NumberOfBedrooms,
                    NumberOfBathrooms = appartment.NumberOfBathrooms,
                    NumberOfBeds = appartment.NumberOfBeds,
                    Elevator = appartment.Elevator,
                    Generator = appartment.Generator,
                    Area = appartment.Area,
                    MasterBedrooms = appartment.MasterBedrooms,
                    Garden = appartment.Garden,
                    WaterContainers = appartment.WaterContainers,
                    Pool = appartment.Pool,
                    Guard = appartment.Guard,
                    Kitchen = appartment.Kitchen,
                    BbqGrill = appartment.BbqGrill,
                    HotTube = appartment.HotTube,
                    Wifi = appartment.Wifi,
                    WorkSpace = appartment.WorkSpace,
                    IndoorFirePlace = appartment.IndoorFirePlace,
                    SmokingAllowed = appartment.SmokingAllowed,
                    airConditionner = appartment.airConditionner,
                    Gym = appartment.Gym,
                    Tvs = appartment.Tvs,
                    Parking = appartment.Parking,
                    CategoryId = category.Id,
                    LocationId = location.Id,
                    OwnerId = appartment.OwnerId,
                    TypeOfPlace = appartment.TypeOfPlace
                };

                _context.Appartments.Add(newAppartment);
                await _context.SaveChangesAsync();

                // Save Images
                var imageUrls = await AddImagesUrls(appartment.images, newAppartment.Id); // Pass the ID of the newly created apartment

                response.StatusCode = 200;
                response.StatusMessage = "Appartment Uploaded Successfully";
                response.Data = appartment;

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Set response status code and message
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";

                return response;
            }
        }
        private async Task<List<string>> GetImageUrls(int apartmentId, string host)
        {
            var baseImageUrl = $"https://{host}/AppartmentsImages/"; // Adjust directory as needed
            var imageUrls = await _context.Images
                                          .Where(img => img.AppartmentId == apartmentId)
                                          .Select(img => img.Url != null ? $"{baseImageUrl}{img.Url}" : null)
                                          .ToListAsync();
            return imageUrls;
        }
        public async Task<List<AppartmentGetDTO>> GetAllAppartments(string host)
        {
            var appartments = await _context.Appartments.OrderByDescending(a => a.Id).ToListAsync();
            var appartmentGetDTOs = new List<AppartmentGetDTO>();

            foreach (var apartment in appartments)
            {
                var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == apartment.OwnerId);
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == apartment.CategoryId);
                var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == apartment.LocationId);
                var images = await GetImageUrls(apartment.Id, host);
                var reservedDates = await GetReservedDatesForApartment(apartment.Id);
                var averageRating = await CalculateAverageRatingForApartment(apartment.Id);

                var dto = new AppartmentGetDTO
                {
                    Id = apartment.Id,
                    Title = apartment.Title,
                    Description = apartment.Description,
                    UploadDate = apartment.UploadDate,
                    Price = apartment.Price,
                    NumberOfBedrooms = apartment.NumberOfBedrooms,
                    NumberOfBathrooms = apartment.NumberOfBathrooms,
                    NumberOfBeds = apartment.NumberOfBeds,
                    Elevator = apartment.Elevator,
                    Generator = apartment.Generator,
                    Area = apartment.Area,
                    MasterBedrooms = apartment.MasterBedrooms,
                    Garden = apartment.Garden,
                    WaterContainers = apartment.WaterContainers,
                    Pool = apartment.Pool,
                    Guard = apartment.Guard,
                    Kitchen = apartment.Kitchen,
                    BbqGrill = apartment.BbqGrill,
                    HotTube = apartment.HotTube,
                    Wifi = apartment.Wifi,
                    WorkSpace = apartment.WorkSpace,
                    IndoorFirePlace = apartment.IndoorFirePlace,
                    SmokingAllowed = apartment.SmokingAllowed,
                    Gym = apartment.Gym,
                    Tvs = apartment.Tvs,
                    Parking = apartment.Parking,
                    airConditionner = apartment.airConditionner,
                    OwnerId = owner?.Id,
                    UserName = owner.UserName,
                    FirstName = owner.FirstName,
                    LastName = owner.LastName,
                    Email = owner.Email,
                    ProfilePicture = owner?.ProfilePicture,
                    ImageSrc = owner.ProfilePicture != null ? $"https://{host}/ProfileImages/{owner.ProfilePicture}" : null,
                    CategoryName = category.Name,
                    Longitude = location.Longitude,
                    Latitude = location.Latitude,
                    City = location.City,
                    Country = location.Country,
                    images = images,
                    TypeOfPlace = apartment.TypeOfPlace,
                    ReservedDates = reservedDates,
                    Rating = averageRating
                };

                appartmentGetDTOs.Add(dto);
            }

            return appartmentGetDTOs;
        }
        private async Task<List<String>> GetReservedDatesForApartment(int apartmentId)
        {
            var reservedDates = await _context.Reservations
                .Where(r => r.AppartmentId == apartmentId)
                .Select(r => r.Date) // Assuming there's a Date property in your Reservation model
                .ToListAsync();

            return reservedDates;
        }
        public async Task<List<AppartmentGetDTO>> GetUserAppartments(string userId, string host)
        {
            var appartments = await _context.Appartments
                .Where(a => a.OwnerId == userId) // Filter by User ID
                .Include(a => a.Owner)
                .Include(a => a.Category)
                .Include(a => a.Location)
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            var appartmentGetDTOs = new List<AppartmentGetDTO>();

            foreach (var apartment in appartments)
            {
                var images = await GetImageUrls(apartment.Id, host); // Assuming GetImageUrls is efficiently fetching URLs
                var reservedDates = await GetReservedDatesForApartment(apartment.Id);
                var averageRating = await CalculateAverageRatingForApartment(apartment.Id);


                var dto = new AppartmentGetDTO
                {
                    Id = apartment.Id,
                    Title = apartment.Title,
                    Description = apartment.Description,
                    UploadDate = apartment.UploadDate,
                    Price = apartment.Price,
                    NumberOfBedrooms = apartment.NumberOfBedrooms,
                    NumberOfBathrooms = apartment.NumberOfBathrooms,
                    NumberOfBeds = apartment.NumberOfBeds,
                    Elevator = apartment.Elevator,
                    Generator = apartment.Generator,
                    Area = apartment.Area,
                    MasterBedrooms = apartment.MasterBedrooms,
                    Garden = apartment.Garden,
                    WaterContainers = apartment.WaterContainers,
                    Pool = apartment.Pool,
                    Guard = apartment.Guard,
                    Kitchen = apartment.Kitchen,
                    BbqGrill = apartment.BbqGrill,
                    HotTube = apartment.HotTube,
                    Wifi = apartment.Wifi,
                    WorkSpace = apartment.WorkSpace,
                    IndoorFirePlace = apartment.IndoorFirePlace,
                    SmokingAllowed = apartment.SmokingAllowed,
                    Gym = apartment.Gym,
                    Tvs = apartment.Tvs,
                    Parking = apartment.Parking,
                    airConditionner = apartment.airConditionner,
                    OwnerId = apartment.Owner?.Id,
                    UserName = apartment.Owner?.UserName,
                    FirstName = apartment.Owner?.FirstName,
                    LastName = apartment.Owner?.LastName,
                    Email = apartment.Owner?.Email,
                    ProfilePicture = apartment.Owner?.ProfilePicture,
                    ImageSrc = apartment.Owner?.ProfilePicture != null ? $"https://{host}/ProfileImages/{apartment.Owner.ProfilePicture}" : null,
                    CategoryName = apartment.Category?.Name,
                    Longitude = apartment.Location.Longitude,
                    Latitude = apartment.Location.Latitude,
                    City = apartment.Location?.City,
                    Country = apartment.Location?.Country,
                    images = images,
                    TypeOfPlace = apartment.TypeOfPlace,
                    ReservedDates = reservedDates,
                    Rating = averageRating
                };

                appartmentGetDTOs.Add(dto);
            }

            return appartmentGetDTOs;
        }
        public async Task<Response<Object>> DeleteApartment(int apartmentId)
        {
            Response<Object> response = new Response<Object>();
            try
            {
                // Find the apartment by ID
                var apartment = await _context.Appartments.FindAsync(apartmentId);

                if (apartment == null)
                {
                    // Apartment with the provided ID not found
                    response.StatusCode = 404;
                    response.StatusMessage = "Apartment not found";
                    return response;
                }

                // Step 1: Remove all reviews associated with the apartment
                var reviews = _context.Reviews.Where(r => r.AppartmentId == apartmentId);
                _context.Reviews.RemoveRange(reviews);

                // Step 2: Remove all requests associated with the apartment
                var requests = _context.Requests.Where(r => r.AppartmentId == apartmentId);
                _context.Requests.RemoveRange(requests);

                // Step 3: Remove all responses associated with the apartment's reservations
                var reservations = _context.Reservations.Where(r => r.AppartmentId == apartmentId).ToList();
                foreach (var reservation in reservations)
                {
                    var responses = _context.Responses.Where(r => r.ReservationId == reservation.Id);
                    _context.Responses.RemoveRange(responses);
                }

                // Step 4: Remove all reservations associated with the apartment
                _context.Reservations.RemoveRange(reservations);

                // Step 5: Remove images associated with the apartment
                var imageUrls = _context.Images.Where(i => i.AppartmentId == apartmentId)
                                    .Select(i => i.Url)
                                    .ToList();
                var apartmentWishList = await _context.AppartmentWishLists.Where(i => i.AppartmentId == apartment.Id).ToListAsync();

                foreach (var appartment in apartmentWishList)
                {
                    _context.AppartmentWishLists.RemoveRange(appartment);
                }

                // Remove the apartment from the context
                _context.Appartments.Remove(apartment);
                await _context.SaveChangesAsync();

           

                // Apartment and associated reviews deleted successfully
                response.StatusCode = 200;
                response.StatusMessage = "Apartment deleted successfully";

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Internal server error
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";

                return response;
            }
        }

        public async Task<Response<object>> SaveUserListing(string userId, int apartmentId)
        {
            try
            {
                // Find the user by userId
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new Response<object>
                    {
                        StatusCode = 404,
                        StatusMessage = "User not found",
                        Data = null
                    };
                }

                // Find the apartment by apartmentId
                var apartment = await _context.Appartments.FindAsync(apartmentId);
                if (apartment == null)
                {
                    return new Response<object>
                    {
                        StatusCode = 404,
                        StatusMessage = "Apartment not found",
                        Data = null
                    };
                }

                // Check if the apartment is already in the user's wishlist
                var existingWishlistEntry = await _context.AppartmentWishLists
                    .FirstOrDefaultAsync(w => w.WishListId == user.WishListId && w.AppartmentId == apartmentId);

                if (existingWishlistEntry != null)
                {
                    return new Response<object>
                    {
                        StatusCode = 400, // Bad Request
                        StatusMessage = "Apartment already saved",
                        Data = null
                    };
                }

                // Create a new instance of AppartmentWishList
                var appartmentWishList = new AppartmentWishList
                {
                    AppartmentId = apartmentId,
                    Appartment = apartment,
                    WishListId = user.WishListId
                };

                // Add the new instance to the context
                _context.AppartmentWishLists.Add(appartmentWishList);

                // Save changes to the database
                await _context.SaveChangesAsync();

                return new Response<object>
                {
                    StatusCode = 200,
                    StatusMessage = "Apartment added to your wishlist successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new Response<object>
                {
                    StatusCode = 500,
                    StatusMessage = "Internal Server Error: " + ex.Message,
                    Data = null
                };
            }
        }
        public async Task<List<AppartmentGetDTO>> GetSavedListingsAsync(string userId, string host)
        {
            var apartments = await _context.AppartmentWishLists
                                            .Where(awl => awl.WishList.UserId == userId)
                                            .Select(awl => awl.Appartment)
                                            .OrderByDescending(a => a.Id)
                                            .ToListAsync();

            var savedListings = new List<AppartmentGetDTO>();

            foreach (var apartment in apartments)
            {
                var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == apartment.OwnerId);
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == apartment.CategoryId);
                var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == apartment.LocationId);
                var images = await GetImageUrls(apartment.Id, host);
                var reservedDates = await GetReservedDatesForApartment(apartment.Id);
                var averageRating = await CalculateAverageRatingForApartment(apartment.Id);

                var dto = new AppartmentGetDTO
                {
                    Id = apartment.Id,
                    Title = apartment.Title,
                    Description = apartment.Description,
                    UploadDate = apartment.UploadDate,
                    Price = apartment.Price,
                    NumberOfBedrooms = apartment.NumberOfBedrooms,
                    NumberOfBathrooms = apartment.NumberOfBathrooms,
                    NumberOfBeds = apartment.NumberOfBeds,
                    Elevator = apartment.Elevator,
                    Generator = apartment.Generator,
                    Area = apartment.Area,
                    MasterBedrooms = apartment.MasterBedrooms,
                    Garden = apartment.Garden,
                    WaterContainers = apartment.WaterContainers,
                    Pool = apartment.Pool,
                    Guard = apartment.Guard,
                    Kitchen = apartment.Kitchen,
                    BbqGrill = apartment.BbqGrill,
                    HotTube = apartment.HotTube,
                    Wifi = apartment.Wifi,
                    WorkSpace = apartment.WorkSpace,
                    IndoorFirePlace = apartment.IndoorFirePlace,
                    SmokingAllowed = apartment.SmokingAllowed,
                    Gym = apartment.Gym,
                    Tvs = apartment.Tvs,
                    Parking = apartment.Parking,
                    airConditionner = apartment.airConditionner,
                    OwnerId = owner?.Id,
                    UserName = owner?.UserName,
                    FirstName = owner?.FirstName,
                    LastName = owner?.LastName,
                    Email = owner?.Email,
                    ProfilePicture = owner?.ProfilePicture,
                    ImageSrc = owner?.ProfilePicture != null ? $"https://{host}/ProfileImages/{owner?.ProfilePicture}" : null,
                    CategoryName = category?.Name,
                    Longitude = location?.Longitude ?? 0, // Default value if location not found
                    Latitude = location?.Latitude ?? 0, // Default value if location not found
                    City = location?.City,
                    Country = location?.Country,
                    images = images,
                    TypeOfPlace = apartment.TypeOfPlace,
                    ReservedDates = reservedDates,
                    Rating = averageRating
                };

                savedListings.Add(dto);
            }

            return savedListings;
        }
        public async Task<Response<object>> RemoveSavedListing(string userId, int apartmentId)
        {
            try
            {
                // Find the user by userId
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new Response<object>
                    {
                        StatusCode = 404,
                        StatusMessage = "User not found",
                        Data = null
                    };
                }

                // Find the saved listing in the user's wishlist
                var savedListing = await _context.AppartmentWishLists
                    .FirstOrDefaultAsync(wl => wl.WishListId == user.WishListId && wl.AppartmentId == apartmentId);

                if (savedListing == null)
                {
                    return new Response<object>
                    {
                        StatusCode = 404,
                        StatusMessage = "Listing not found in user's wishlist",
                        Data = null
                    };
                }

                // Remove the saved listing
                _context.AppartmentWishLists.Remove(savedListing);
                await _context.SaveChangesAsync();

                return new Response<object>
                {
                    StatusCode = 200,
                    StatusMessage = "Listing removed from user's wishlist successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new Response<object>
                {
                    StatusCode = 500,
                    StatusMessage = "Internal Server Error: " + ex.Message,
                    Data = null
                };
            }
        }
        public async Task<Response<Object>> EditAppartment(int id, AppartmentEditDTO updatedAppartment)
        {
            Response<Object> response = new Response<Object>();
            try
            {
                // Find the existing apartment by ID
                var existingAppartment = await _context.Appartments.FindAsync(id);
                if (existingAppartment == null)
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "Apartment not found";
                    return response;
                }

                // Update the existing apartment properties with the new values
                existingAppartment.Title = updatedAppartment.Title;
                existingAppartment.Description = updatedAppartment.Description;
                existingAppartment.Price = updatedAppartment.Price;
                existingAppartment.NumberOfBedrooms = updatedAppartment.NumberOfBedrooms;
                existingAppartment.NumberOfBathrooms = updatedAppartment.NumberOfBathrooms;
                existingAppartment.NumberOfBeds = updatedAppartment.NumberOfBeds;
                existingAppartment.Elevator = updatedAppartment.Elevator;
                existingAppartment.Generator = updatedAppartment.Generator;
                existingAppartment.Area = updatedAppartment.Area;
                existingAppartment.MasterBedrooms = updatedAppartment.MasterBedrooms;
                existingAppartment.Garden = updatedAppartment.Garden;
                existingAppartment.WaterContainers = updatedAppartment.WaterContainers;
                existingAppartment.Pool = updatedAppartment.Pool;
                existingAppartment.Guard = updatedAppartment.Guard;
                existingAppartment.Kitchen = updatedAppartment.Kitchen;
                existingAppartment.BbqGrill = updatedAppartment.BbqGrill;
                existingAppartment.HotTube = updatedAppartment.HotTube;
                existingAppartment.Wifi = updatedAppartment.Wifi;
                existingAppartment.WorkSpace = updatedAppartment.WorkSpace;
                existingAppartment.IndoorFirePlace = updatedAppartment.IndoorFirePlace;
                existingAppartment.SmokingAllowed = updatedAppartment.SmokingAllowed;
                existingAppartment.airConditionner = updatedAppartment.airConditionner;
                existingAppartment.Gym = updatedAppartment.Gym;
                existingAppartment.Tvs = updatedAppartment.Tvs;
                existingAppartment.Parking = updatedAppartment.Parking;
                existingAppartment.TypeOfPlace = updatedAppartment.TypeOfPlace;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Set response status code and message
                response.StatusCode = 200;
                response.StatusMessage = "Apartment updated successfully";
                return response;
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Set response status code and message
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";

                return response;
            }
        }
        public async Task<Response<ReviewDTO>> UploadReview(ReviewDTO review)
        {
            var response = new Response<ReviewDTO>();

            try
            {
                if (!string.IsNullOrEmpty(review.Content))
                {

                    var newReview = new Review
                    {
                        Content = review.Content,
                        Value = review.Value,
                        DateRated = DateTime.Now,
                        ReviewerId = review.ReviewerId,
                        AppartmentId = review.AppartmentId
                    };

                    // Add the new review to the database
                    _context.Reviews.Add(newReview);
                    await _context.SaveChangesAsync();

                    // Set response status code and message
                    response.StatusCode = 200;
                    response.StatusMessage = "Review uploaded successfully";
                    response.Data = review;

                    return response;
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Please enter a review";
                    return response;
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Set response status code and message
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";

                return response;
            }
        }
        public async Task<Response<FeedbackDTO>> SendFeedback(FeedbackDTO feedback)
        {
            var response = new Response<FeedbackDTO>();
            try
            {
                if (!string.IsNullOrEmpty(feedback.Content))
                {

                    var feedBack = new Feedback
                    {
                        Content = feedback.Content,
                        Value = feedback.Value,
                        WriterId = feedback.WriterId,
                    };

                    // Add the new review to the database
                    _context.Feedbacks.Add(feedBack);
                    await _context.SaveChangesAsync();

                    // Set response status code and message
                    response.StatusCode = 200;
                    response.StatusMessage = "Feedback sent successfully!";
                    response.Data = feedback;

                    return response;
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Please enter a feedback";
                    return response;
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Set response status code and message
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";

                return response;
            }
        }
        public async Task<List<ReviewGetDTO>> GetAppartmentReviews(int appartmentId, string host)
        {
            var reviews = await _context.Reviews
                .Where(r => r.AppartmentId == appartmentId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            // Now, join the reviews with the AppUsers table to get reviewer details
            var reviewDetails = new List<ReviewGetDTO>();

            foreach (var review in reviews)
            {
                var user = await _context.ApplicationUsers.FindAsync(review.ReviewerId);

                if (user != null)
                {
                    var hasReservedBefore = await HasReviewerReservedBefore(review.ReviewerId, appartmentId);

                    reviewDetails.Add(new ReviewGetDTO
                    {
                        Id = review.Id,
                        Content = review.Content,
                        Value = review.Value,
                        DateRated = review.DateRated,
                        AppartmentId = review.AppartmentId,
                        ReviewrFirstName = user.FirstName,
                        ReviewrLastName = user.LastName,
                        ProfilePicture = user.ProfilePicture, // Assuming this is the URL of the reviewer's profile picture
                        ImageSrc = user.ProfilePicture != null ? $"https://{host}/ProfileImages/{user.ProfilePicture}" : null,
                        HasReservedBefore = hasReservedBefore
                    });
                }
            }

            return reviewDetails;
        }
        public async Task<double> CalculateAverageRatingForApartment(int apartmentId)
        {
            // Get all reviews for the specified apartment
            var reviewsForApartment = await _context.Reviews
                .Where(r => r.AppartmentId == apartmentId)
                .ToListAsync();

            // Check if there are any reviews for the apartment
            if (reviewsForApartment.Any())
            {
                double totalRating = 0;
                int numberOfReviews = 0;

                // Iterate through each review and calculate the total rating
                foreach (var review in reviewsForApartment)
                {
                    // Check if the reviewer has reserved the apartment before
                    bool hasReserved = await HasReviewerReservedBefore(review.ReviewerId, apartmentId);

                    // If the reviewer has reserved before, include their rating in the total
                    if (hasReserved)
                    {
                        totalRating += review.Value;
                        numberOfReviews++;
                    }
                }

                // Check if there are reviews from reserved users
                if (numberOfReviews > 0)
                {
                    // Calculate the average rating
                    double averageRating = totalRating / numberOfReviews;

                    // If the average rating is 0, return 0
                    if (averageRating == 0)
                    {
                        return 0;
                    }

                    // Return the calculated average rating
                    return averageRating;
                }
            }

            // No reviews found, return a response message
            return -1; // You can choose an appropriate value to indicate no reviews found
        }
        private async Task<bool> HasReviewerReservedBefore(String reviewerId, int appartmentId)
        {
            return await _context.Reservations
                .AnyAsync(r => r.CustomerId == reviewerId && r.AppartmentId == appartmentId);
        }
        public async Task<Response<List<FeedbackGetDTO>>> getAllFeedbacks(string host)
        {
            var feedbackDTOs = new List<FeedbackGetDTO>();
            var response = new Response<List<FeedbackGetDTO>>();

            try
            {
                // Retrieve all feedback entries from the database
                var feedbacks = await _context.Feedbacks.OrderByDescending(f => f.Id).ToListAsync();

                foreach (var feedback in feedbacks)
                {
                    // Fetch the associated user details using the writer ID
                    var writer = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == feedback.WriterId);

                    if (writer != null)
                    {
                        // Create a new FeedbackGetDTO object and populate it with data
                        var feedbackDTO = new FeedbackGetDTO
                        {
                            Id = feedback.Id,
                            Value = feedback.Value,
                            Content = feedback.Content,
                            writerFirstName = writer.FirstName,
                            writerLastName = writer.LastName,
                            ProfilePicture = writer.ProfilePicture, // Example URL to profile picture
                            ImageSrc = writer.ProfilePicture != null ? $"https://{host}/ProfileImages/{writer.ProfilePicture}" : null // Example URL to feedback image
                        };

                        // Add the FeedbackGetDTO object to the list
                        feedbackDTOs.Add(feedbackDTO);
                    }
                }
                response.StatusCode = 200;
                response.Data = feedbackDTOs;
                return response;
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
                response.Data = null;
                // Return an empty list or handle the error as needed
                return response;
            }
        }
        public async Task<Response<Object>> DeleteReview(int reviewId)
        {
            Response<Object> response = new Response<Object>();
            try
            {
                // Find the review by ID
                var review = await _context.Reviews.FindAsync(reviewId);

                if (review == null)
                {
                    // Review with the provided ID not found
                    response.StatusCode = 404;
                    response.StatusMessage = "Review not found";
                    return response;
                }

                // Remove the review from the context
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                // Review deleted successfully
                response.StatusCode = 200;
                response.StatusMessage = "Review deleted successfully";

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Internal server error
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";

                return response;
            }
        }
        public async Task<Response<Object>> DeleteFeedback(int feedbackId)
        {
            Response<Object> response = new Response<Object>();
            try
            {
                // Find the review by ID
                var feedback = await _context.Feedbacks.FindAsync(feedbackId);

                if (feedback == null)
                {
                    // Review with the provided ID not found
                    response.StatusCode = 404;
                    response.StatusMessage = "Feedback not found";
                    return response;
                }

                // Remove the review from the context
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                // Review deleted successfully
                response.StatusCode = 200;
                response.StatusMessage = "Feedback deleted successfully";

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Internal server error
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";

                return response;
            }
        }
        public async Task<Response<List<UserDTO>>> GetAllUsers(string host)
        {
            var users = await _context.ApplicationUsers.Where(u => u.Role == "Customer").ToListAsync();
            var usersDTO = new List<UserDTO>();

            foreach (var user in users)
            {
                var userDTO = new UserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    ProfilePicture = user.ProfilePicture,
                    //ImageSrc = $"https://{host}/ProfileImages/{existingUser.ProfilePicture}" // Use the provided host parameter
                    ImageSrc = user.ProfilePicture != null ? $"https://{host}/ProfileImages/{user.ProfilePicture}" : null
                };
                usersDTO.Add(userDTO);
            }
            Response<List<UserDTO>> response = new Response<List<UserDTO>>();
            response.StatusCode = 200;
            response.StatusMessage = "Users found";
            response.Data = usersDTO;
            return response;

        }
        public async Task<Response<object>> DeleteUserAsync(string userId)
        {
            Response<object> response = new Response<object>();
            try
            {
                // Step 1: Delete the user's responses related to reservations
                var userResponses = await _context.Responses.Where(r => r.UserId == userId || r.reservation.CustomerId == userId).ToListAsync();
                _context.Responses.RemoveRange(userResponses);
                await _context.SaveChangesAsync();

                // Step 2: Delete the user's reservations
                var userReservations = await _context.Reservations.Where(r => r.CustomerId == userId).ToListAsync();
                _context.Reservations.RemoveRange(userReservations);
                await _context.SaveChangesAsync();

                // Step 3: Delete the user's requests
                var userRequests = await _context.Requests.Where(r => r.UserId == userId).ToListAsync();
                _context.Requests.RemoveRange(userRequests);
                await _context.SaveChangesAsync();

                // Step 7: Delete the user's feedbacks
                var userFeedbacks = await _context.Feedbacks.Where(f => f.WriterId == userId).ToListAsync();
                _context.Feedbacks.RemoveRange(userFeedbacks);
                await _context.SaveChangesAsync();


                // Step 4: Delete the user's apartments
                var userApartments = await _context.Appartments.Where(a => a.OwnerId == userId).ToListAsync();
                foreach (var apartment in userApartments)
                {
                    // Delete the apartment and associated entities using the DeleteApartment method
                    var apartmentDeletionResponse = await DeleteApartment(apartment.Id);

                    // Check if apartment deletion was successful
                    if (apartmentDeletionResponse.StatusCode != 200)
                    {
                        // If deletion fails for any apartment, return the response immediately
                        return apartmentDeletionResponse;
                    }
                }

                // Step 5: Delete the user's wish list
                var userWishList = await _context.WishList.SingleOrDefaultAsync(x => x.UserId == userId);
                if (userWishList != null)
                {
                    _context.WishList.Remove(userWishList);
                    await _context.SaveChangesAsync();
                }

                // Step 6: Delete the user's reviews
                var userReviews = await _context.Reviews.Where(r => r.ReviewerId == userId).ToListAsync();
                _context.Reviews.RemoveRange(userReviews);
                await _context.SaveChangesAsync();

                // Step 7: Delete the user
                var user = await _context.ApplicationUsers.FindAsync(userId);
                if (user == null)
                {
                    // User with the provided ID not found
                    response.StatusCode = 404;
                    response.StatusMessage = "User not found";
                    return response;
                }

                // Remove the user from the context
                _context.ApplicationUsers.Remove(user);
                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.StatusMessage = "User deleted successfully";
                return response;
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Internal server error
                response.StatusCode = 500;
                response.StatusMessage = "Internal server error";
                return response;
            }
        }
        public async Task<Response<Object>> RequestReservation(RequestsDTO request)
        {
            try
            {
                // Get the user's first and last names
                var user = await _context.Users.FindAsync(request.UserId);
                string userFullName = $"{user.FirstName} {user.LastName}";

                // Get the apartment owner's ID using the apartment ID
                var apartment = await _context.Appartments.FindAsync(request.AppartmentId);

                string ownerId = apartment.OwnerId;

                // Get the owner's first and last names
                var owner = await _context.Users.FindAsync(ownerId);
                string ownerFullName = $"{owner.FirstName} {owner.LastName}";

                // Generate a professional message for the reservation request
                string message = $"{userFullName} is requesting to reserve the apartment for the date range {request.DateRange}. Please review and respond accordingly.";

                // Create a new reservation request
                var reservationRequest = new Requests
                {
                    AppartmentId = request.AppartmentId,
                    UserId = request.UserId,
                    DateRange = request.DateRange,
                    ownerId = ownerId, 
                    Content = message,
                    Status = "Pending"

                };

                // Save the reservation request to the database
                _context.Requests.Add(reservationRequest);
                await _context.SaveChangesAsync();

                // Return a success response
                return new Response<Object>
                {
                    StatusCode = 200,
                    StatusMessage = "Reservation request successful",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                // Return an error response
                return new Response<Object>
                {
                    StatusCode = 500,
                    StatusMessage = "Failed to request reservation",
                    Data = null
                };
            }
        }
        public async Task<Response<Object>> AcceptReservationRequest(int requestId)
        {
            try
            {
                // Find the request by ID
                var request = await _context.Requests.FindAsync(requestId);

                // Check if the request exists
                if (request == null)
                {
                    return new Response<Object>
                    {
                        StatusCode = 404,
                        StatusMessage = "Request not found",
                        Data = null
                    };
                }

                // Update the status of the request to "Accepted"
                request.Status = "Accepted";

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Get the user's first and last names
                var user = await _context.Users.FindAsync(request.UserId);
                string userFullName = $"{user.FirstName} {user.LastName}";

                // Get the apartment owner's ID using the apartment ID
                var apartment = await _context.Appartments.FindAsync(request.AppartmentId);
                string ownerId = apartment.OwnerId;

                // Get the owner's first and last names
                var owner = await _context.Users.FindAsync(ownerId);
                string ownerFullName = $"{owner.FirstName} {owner.LastName}";

                // Generate a professional message for the response content
                string content = $"Owner {ownerFullName} accepted your reservation for the apartment on {request.DateRange}. Please proceed accordingly.";

                var ReservationContent = $"Dear guest, we are delighted to inform you that you have successfully reserved a stay at the luxurious {ownerFullName}'s residence. Your upcoming experience promises comfort, elegance, and unparalleled hospitality. We look forward to welcoming you on {request.DateRange}.";
                // Create a new response with the professional content
                var response = new Responses
                {
                    Content = content,
                    DateRange = request.DateRange,
                    Status = "Accepted", // Set the status of the response to "Accepted"
                    UserId = request.UserId,
                    ReservationId = null, // Set ReservationId to null in the response
                    RequestId = request.Id // Set the request ID as the ID of the response
                };

                // Add the response to the response table
                _context.Responses.Add(response);
                await _context.SaveChangesAsync();

                request.ResponsesId = response.Id;
                await _context.SaveChangesAsync();

                // Check if the response status is "Accepted"
                if (response.Status == "Accepted")
                {
                    // Create a new reservation based on the request information
                    var reservation = new Reservation
                    {
                        AppartmentId = request.AppartmentId,
                        CustomerId = request.UserId,
                        Date = request.DateRange,
                        ResponsesId = response.Id,
                        Content = ReservationContent

                        // Add any other necessary properties
                    };

                    // Add the reservation to the reservation table
                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();

                    // Set the ReservationId in the response to the ID of the newly created reservation
                    response.ReservationId = reservation.Id;
                    await _context.SaveChangesAsync();
                }

                return new Response<Object>
                {
                    StatusCode = 200,
                    StatusMessage = "Reservation request accepted",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                // Return an error response
                return new Response<Object>
                {
                    StatusCode = 500,
                    StatusMessage = "Failed to accept reservation request",
                    Data = null
                };
            }
        }
        public async Task<Response<Object>> RejectReservationRequest(int requestId)
        {
            try
            {
                // Find the request by ID
                var request = await _context.Requests.FindAsync(requestId);

                // Check if the request exists
                if (request == null)
                {
                    return new Response<Object>
                    {
                        StatusCode = 404,
                        StatusMessage = "Request not found",
                        Data = null
                    };
                }

                // Update the status of the request to "Rejected"
                request.Status = "Rejected";

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Get the user's first and last names
                var user = await _context.Users.FindAsync(request.UserId);
                string userFullName = $"{user.FirstName} {user.LastName}";

                // Get the apartment owner's ID using the apartment ID
                var apartment = await _context.Appartments.FindAsync(request.AppartmentId);
                string ownerId = apartment.OwnerId;

                // Get the owner's first and last names
                var owner = await _context.Users.FindAsync(ownerId);
                string ownerFullName = $"{owner.FirstName} {owner.LastName}";

                // Generate a professional message for the response content
                string content = $"Dear {userFullName},\n\nWe regret to inform you that your reservation request for the apartment on {request.DateRange} has been rejected.\n\nSincerely,\n{ownerFullName}";
                // Create a new response with the professional content
                var response = new Responses
                {
                    Content = content,
                    DateRange = request.DateRange,
                    Status = "Rejected", // Set the status of the response to "Rejected"
                    UserId = request.UserId,
                    ReservationId = null, // No reservation for a rejected request
                    RequestId = request.Id // Set the request ID as the ID of the response
                };

                // Add the response to the response table
                _context.Responses.Add(response);
                await _context.SaveChangesAsync();

                // Set ResponsesId in the request to the ID of the created response
                request.ResponsesId = response.Id;
                await _context.SaveChangesAsync();

                return new Response<Object>
                {
                    StatusCode = 200,
                    StatusMessage = "Reservation request rejected",
                    Data = null
                };
            }
            catch
            {
                // Log the exception or handle it as needed
                // Return an error response
                return new Response<Object>
                {
                    StatusCode = 500,
                    StatusMessage = "Failed to reject reservation request",
                    Data = null
                };
            }
        }
        public async Task<Response<List<RequestGetDTO>>> GetUserReservationRequests(string userId, string host)
        {
            try
            {
                // Find all requests for the specific user
                var requests = await _context.Requests
                    .Where(r => r.ownerId == userId)
                    .Include(r => r.appartment)
                    .Include(r => r.user)
                    .OrderByDescending(r => r.Id)
                    .ToListAsync();

     


                // Check if requests exist
                if (requests == null || requests.Count == 0)
                {
                    return new Response<List<RequestGetDTO>>
                    {
                        StatusCode = 202,
                        StatusMessage = "No requests found for the user",
                        Data = null
                    };
                }

                // Map the requests to RequestGetDTO
                var requestsDTO = requests.Select(async r =>
                {
                    // Get the apartment image
                    var apartmentImage = await _context.Images
                        .Where(i => i.AppartmentId == r.AppartmentId)
                        .Select(i => i.Url) // Assuming the URL is stored in the Url property
                        .FirstOrDefaultAsync();

                    var userIdFromRequest = r.UserId;
                    var userProfilePicture = await _context.ApplicationUsers
                        .Where(u => u.Id == userIdFromRequest)
                        .Select(u => u.ProfilePicture) // Assuming the profile picture URL is stored in the ProfilePicture property
                        .FirstOrDefaultAsync();

                    return new RequestGetDTO
                    {
                        Id = r.Id,
                        Content = r.Content,
                        DateRange = r.DateRange,
                        UserId = r.UserId,
                        AppartmentId = r.AppartmentId,
                        Status = r.Status,
                        UserProfilePicture = userProfilePicture != null ? $"https://{host}/ProfileImages/{userProfilePicture}" : null,
                        AppartmentImage = apartmentImage != null ? $"https://{host}/AppartmentsImages/{apartmentImage}" : null
                    };
                }).Select(t => t.Result).ToList();

                return new Response<List<RequestGetDTO>>
                {
                    StatusCode = 200,
                    StatusMessage = "Requests retrieved successfully",
                    Data = requestsDTO
                };
            }
            catch
            {
                // Log the exception or handle it as needed
                // Return an error response
                return new Response<List<RequestGetDTO>>
                {
                    StatusCode = 500,
                    StatusMessage = "Failed to retrieve requests",
                    Data = null
                };
            }
        }
        public async Task<Response<object>> DeleteReservationRequest(int requestId)
        {
            try
            {
                // Get the request from the database
                var request = await _context.Requests.Include(r => r.responses).FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                {
                    return new Response<object>
                    {
                        StatusCode = 404,
                        StatusMessage = "Request not found",
                        Data = null
                    };
                }

                // Check if there is a response associated with the request
                var response = request.responses;

                if (response != null)
                {
                    if (response.Status == "Accepted")
                    {
                        // Get the reservation associated with the response
                        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == response.ReservationId);

                        if (reservation != null)
                        {
                            // Delete the reservation
                            _context.Reservations.Remove(reservation);
                        }

                        // Delete the response
                        _context.Responses.Remove(response);
                    }
                    else if (response.Status == "Rejected")
                    {
                        // Delete the response
                        _context.Responses.Remove(response);
                    }
                }

                // Finally, delete the request
                _context.Requests.Remove(request);

                // Save changes to the database
                await _context.SaveChangesAsync();

                return new Response<object>
                {
                    StatusCode = 200,
                    StatusMessage = "Request deleted successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new Response<object>
                {
                    StatusCode = 500,
                    StatusMessage = $"Error deleting request: {ex.Message}",
                    Data = null
                };
            }
        }
        public async Task<Response<List<ReservationGetDTO>>> GetUserReservations(string userId, string host)
        {
            try
            {
                // Find all reservations for the specific user
                var reservations = await _context.Reservations
                    .Where(r => r.CustomerId == userId)
                    .Include(r => r.Appartment)
                    .Include(r => r.Customer)
                    .OrderByDescending(r => r.Id)
                    .ToListAsync();

                // Check if reservations exist
                if (reservations == null || reservations.Count == 0)
                {
                    return new Response<List<ReservationGetDTO>>
                    {
                        StatusCode = 202,
                        StatusMessage = "No reservations found for the user",
                        Data = null
                    };
                }

                // Map the reservations to ReservationGetDTO
                var reservationsDTO = new List<ReservationGetDTO>();
                foreach (var reservation in reservations)
                {
                    // Get the owner's name
                    var owner = await _context.Appartments
                        .Where(a => a.Id == reservation.AppartmentId)
                        .Select(a => new { a.Owner.FirstName, a.Owner.LastName })
                        .FirstOrDefaultAsync();

                    if (owner == null)
                    {
                        // Handle the case where owner details are not found
                        continue;
                    }

                    var ownerName = $"{owner.FirstName} {owner.LastName}";

                    // Get the apartment image
                    var apartmentImage = await _context.Images
                        .Where(i => i.AppartmentId == reservation.AppartmentId)
                        .Select(i => i.Url)
                        .FirstOrDefaultAsync();

                    // Construct the reservation content
                    var content = $"Dear guest, we are delighted to inform you that you have successfully reserved a stay at the luxurious {ownerName}'s residence. Your upcoming experience promises comfort, elegance, and unparalleled hospitality. We look forward to welcoming you on {reservation.Date}.";

                    reservationsDTO.Add(new ReservationGetDTO
                    {
                        Id = reservation.Id,
                        Content = content,
                        Date = reservation.Date,
                        CustomerId = reservation.CustomerId,
                        AppartmentId = reservation.AppartmentId,
                        ResponsesId = reservation.ResponsesId,
                        AppartmentImage = apartmentImage != null ? $"https://{host}/AppartmentsImages/{apartmentImage}" : null
                    });
                }

                return new Response<List<ReservationGetDTO>>
                {
                    StatusCode = 200,
                    StatusMessage = "Reservations retrieved successfully",
                    Data = reservationsDTO
                };
            }
            catch
            {
                // Log the exception or handle it as needed
                // Return an error response
                return new Response<List<ReservationGetDTO>>
                {
                    StatusCode = 500,
                    StatusMessage = "Failed to retrieve reservations",
                    Data = null
                };
            }
        }
        public async Task<AppartmentGetDTO> GetAppartmentById(int apartmentId, string host)
        {
            var apartment = await _context.Appartments
                .Where(a => a.Id == apartmentId)
                .Include(a => a.Owner)
                .Include(a => a.Category)
                .Include(a => a.Location)
                .FirstOrDefaultAsync();

            if (apartment == null)
            {
                return null; // Or throw an exception as per your requirement
            }

            var images = await GetImageUrls(apartment.Id, host); // Assuming GetImageUrls is efficiently fetching URLs

            var dto = new AppartmentGetDTO
            {
                Id = apartment.Id,
                Title = apartment.Title,
                Description = apartment.Description,
                UploadDate = apartment.UploadDate,
                Price = apartment.Price,
                NumberOfBedrooms = apartment.NumberOfBedrooms,
                NumberOfBathrooms = apartment.NumberOfBathrooms,
                NumberOfBeds = apartment.NumberOfBeds,
                Elevator = apartment.Elevator,
                Generator = apartment.Generator,
                Area = apartment.Area,
                MasterBedrooms = apartment.MasterBedrooms,
                Garden = apartment.Garden,
                WaterContainers = apartment.WaterContainers,
                Pool = apartment.Pool,
                Guard = apartment.Guard,
                Kitchen = apartment.Kitchen,
                BbqGrill = apartment.BbqGrill,
                HotTube = apartment.HotTube,
                Wifi = apartment.Wifi,
                WorkSpace = apartment.WorkSpace,
                IndoorFirePlace = apartment.IndoorFirePlace,
                SmokingAllowed = apartment.SmokingAllowed,
                Gym = apartment.Gym,
                Tvs = apartment.Tvs,
                Parking = apartment.Parking,
                airConditionner = apartment.airConditionner,
                OwnerId = apartment.Owner?.Id,
                UserName = apartment.Owner?.UserName,
                FirstName = apartment.Owner?.FirstName,
                LastName = apartment.Owner?.LastName,
                Email = apartment.Owner?.Email,
                ProfilePicture = apartment.Owner?.ProfilePicture,
                ImageSrc = apartment.Owner?.ProfilePicture != null ? $"https://{host}/ProfileImages/{apartment.Owner.ProfilePicture}" : null,
                CategoryName = apartment.Category?.Name,
                Longitude = apartment.Location.Longitude,
                Latitude = apartment.Location.Latitude,
                City = apartment.Location?.City,
                Country = apartment.Location?.Country,
                images = images,
                TypeOfPlace = apartment.TypeOfPlace
            };

            return dto;
        }
        public async Task<List<ResponsesGetDTO>> GetUserResponses(string userId, string host)
        {
            // Query the database to get user responses with related apartment image URL
            var responses = await _context.Responses
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            var responsesDTO = new List<ResponsesGetDTO>();

            foreach (var response in responses)
            {
                // Find the request associated with the response
                var request = await _context.Requests.FindAsync(response.RequestId);

                if (request != null)
                {
                    // Get the apartment ID from the request
                    var apartmentId = request.AppartmentId;

                    // Find the apartment image URL directly from the Images table
                    var imageUrl = await _context.Images
                        .Where(i => i.AppartmentId == apartmentId)
                        .Select(i => i.Url)
                        .FirstOrDefaultAsync();

                    // Create a ResponsesDTO object and populate its properties
                    var responseDTO = new ResponsesGetDTO
                    {
                        Id = response.Id,
                        Content = response.Content,
                        DateRange = response.DateRange,
                        Status = response.Status,
                        UserId = response.UserId,
                        ReservationId = response.ReservationId,
                        RequestId = response.RequestId,
                        AppartmentImage = imageUrl != null ? $"https://{host}/AppartmentsImages/{imageUrl}" : null
                    };

                    responsesDTO.Add(responseDTO);
                }
            }

            return responsesDTO;
        }
        public async Task<int> GetNumberOfReservedApartmentsAsync()
        {
            return await _context.Reservations.CountAsync();
        }
        public async Task<int> GetNumberOfPropertiesForSaleAsync()
        {
            var saleCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == "HomeForSale");

            if (saleCategory == null)
            {
                throw new Exception("Sale category not found");
            }

            return await _context.Appartments
                .CountAsync(a => a.CategoryId == saleCategory.Id);
        }
        public async Task<int> GetNumberOfPropertiesForRentAsync()
        {
            var rentCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == "HomeForRent");

            if (rentCategory == null)
            {
                throw new Exception("Rent category not found");
            }

            return await _context.Appartments
                .CountAsync(a => a.CategoryId == rentCategory.Id);
        }
        public async Task<int> GetNumberOfUploadedApartmentsAsync()
        {
            return await _context.Appartments.CountAsync();
        }

    }
}






 

    