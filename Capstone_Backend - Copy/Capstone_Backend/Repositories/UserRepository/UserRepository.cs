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

        public async Task<Response<Object>> EditProfile(string id, string email, string username, IFormFile? image)
        {
            Response<Object> response = new Response<Object>();
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

                // Check if the new username is different from the current username
                if (existingUser.UserName != username)
                {
                    // Check if the new username already exists
                    var userNameExist = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == username);
                    if (userNameExist != null)
                    {
                        response.StatusCode = 401;
                        response.StatusMessage = "Username already exists!";
                        return response;
                    }
                }


                // Validate email format
                if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Invalid email format!";
                    return response;
                }


                if (existingUser.ProfilePicture == image?.FileName)
                {
                    // If the provided image is the same as the current profile picture, set image to null
                    image = null;
                }
                // Check if a new image file is provided and it's different from the current profile picture
                else if (image != null && existingUser.ProfilePicture != image.FileName)
                {
                    // Save the new profile picture
                    existingUser.ProfilePicture = await SaveProfileImage(image);
                }

                // Check if there are any changes to be saved
                if (existingUser.Email == email && existingUser.UserName == username && image == null)
                {
                    response.StatusCode = 199;
                    response.StatusMessage = "No changes submitted";
                    return response;
                }

                // Update email and username
                existingUser.Email = email;
                existingUser.UserName = username;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Email, username, and profile picture updated successfully
                response.StatusCode = 200;
                response.StatusMessage = "Profile Updated Successfully";

                return response;
            }
            catch (Exception ex)
            {
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
            var appartments = await _context.Appartments.ToListAsync();
            var appartmentGetDTOs = new List<AppartmentGetDTO>();

            foreach (var apartment in appartments)
            {
                var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == apartment.OwnerId);
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == apartment.CategoryId);
                var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == apartment.LocationId);
                var images = await GetImageUrls(apartment.Id, host);


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
                    TypeOfPlace = apartment.TypeOfPlace
                };

                appartmentGetDTOs.Add(dto);
            }

            return appartmentGetDTOs;
        }
        public async Task<List<AppartmentGetDTO>> GetUserAppartments(string userId, string host)
        {
            var appartments = await _context.Appartments
                .Where(a => a.OwnerId == userId) // Filter by User ID
                .Include(a => a.Owner)
                .Include(a => a.Category)
                .Include(a => a.Location)
                .ToListAsync();

            var appartmentGetDTOs = new List<AppartmentGetDTO>();

            foreach (var apartment in appartments)
            {
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

                // Remove all reviews associated with the apartment
                var reviews = _context.Reviews.Where(r => r.AppartmentId == apartmentId);
                _context.Reviews.RemoveRange(reviews);

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

                foreach (var imageUrl in imageUrls)
                {
                    // Construct the image path based on the image name
                    var imageName = imageUrl; // Assuming imageUrl contains the image name
                    var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "AppartmentsImages", imageName);

                    // Check if the file exists, then delete it
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                    else
                    {
                        // Log a message indicating that the file does not exist
                        Console.WriteLine($"File '{imagePath}' does not exist.");
                    }
                }

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
                                            .ToListAsync();

            var savedListings = new List<AppartmentGetDTO>();

            foreach (var apartment in apartments)
            {
                var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == apartment.OwnerId);
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == apartment.CategoryId);
                var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == apartment.LocationId);
                var images = await GetImageUrls(apartment.Id, host);

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
                    TypeOfPlace = apartment.TypeOfPlace
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
                .ToListAsync();

            // Now, join the reviews with the AppUsers table to get reviewer details
            var reviewDetails = reviews
                .Join(_context.ApplicationUsers,
                      review => review.ReviewerId,
                      user => user.Id,
                      (review, user) => new ReviewGetDTO
                      {
                          Id = review.Id,
                          Content = review.Content,
                          Value = review.Value,
                          DateRated = review.DateRated,
                          AppartmentId = review.AppartmentId,
                          ReviewrFirstName = user.FirstName,
                          ReviewrLastName = user.LastName,
                          ProfilePicture = user.ProfilePicture, // Assuming this is the URL of the reviewer's profile picture
                                                                // You may need to adjust the above property based on how you store profile pictures
                          ImageSrc = user.ProfilePicture != null ? $"https://{host}/ProfileImages/{user.ProfilePicture}" : null,
                      })
                .ToList();

            return reviewDetails;
        }
        public async Task<Response<List<FeedbackGetDTO>>> getAllFeedbacks(string host)
        {
            var feedbackDTOs = new List<FeedbackGetDTO>();
            var response = new Response<List<FeedbackGetDTO>>();

            try
            {
                // Retrieve all feedback entries from the database
                var feedbacks = await _context.Feedbacks.ToListAsync();

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
            var users = await _context.ApplicationUsers.ToListAsync();
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
                // Find all apartments owned by the user
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
                var userWishList = await _context.WishList.SingleOrDefaultAsync(x => x.UserId == userId);
                if(userWishList != null)
                {
                    _context.WishList.Remove(userWishList);
                    await _context.SaveChangesAsync();
                }
                var userReviews = await _context.Reviews.Where(r => r.ReviewerId == userId).ToListAsync();

                _context.Reviews.RemoveRange(userReviews);
                await _context.SaveChangesAsync();

                // Find the user by ID
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

                // Save changes to apply the deletions
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
                string content = $"Owner {ownerFullName} accepted your reservation for the apartment. Please proceed accordingly.";

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
                        OwnerId = request.UserId,
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
                string content = $"Owner {ownerFullName} has rejected your reservation request for the apartment.";

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

                    return new RequestGetDTO
                    {
                        Id = r.Id,
                        Content = r.Content,
                        DateRange = r.DateRange,
                        UserId = r.UserId,
                        AppartmentId = r.AppartmentId,
                        Status = r.Status,
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
                    .Where(r => r.OwnerId == userId)
                    .Include(r => r.Appartment)
                    .Include(r => r.Owner)
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
                var reservationsDTO = reservations.Select(async r =>
                {
                    // Get the apartment image
                    var apartmentImage = await _context.Images
                        .Where(i => i.AppartmentId == r.AppartmentId)
                        .Select(i => i.Url) // Assuming the URL is stored in the Url property
                        .FirstOrDefaultAsync();

                    // Get the owner's name
                    var ownerName = $"{r.Owner.FirstName} {r.Owner.LastName}";

                    // Construct the reservation content
                    var content = $"Dear guest, we are delighted to inform you that you have successfully reserved a stay at the luxurious {ownerName}'s residence. Your upcoming experience promises comfort, elegance, and unparalleled hospitality. We look forward to welcoming you on {r.Date}.";

                    return new ReservationGetDTO
                    {
                        Id = r.Id,
                        Content = content,
                        Date = r.Date,
                        OwnerId = r.OwnerId,
                        AppartmentId = r.AppartmentId,
                        ResponsesId = r.ResponsesId,
                        AppartmentImage = apartmentImage != null ? $"https://{host}/AppartmentsImages/{apartmentImage}" : null
                    };
                }).Select(t => t.Result).ToList();

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


    }
}






 

    