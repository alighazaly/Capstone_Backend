using Capstone_Backend.Data;
using Capstone_Backend.Models;
using Capstone_Backend.Repositories.UserRepository;
using Capstone_Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capstone_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;

        public UserController(AppDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        [HttpPost("editprofile")]
        public async Task<Response<Object>> EditProfile( string id,string email,string username, IFormFile? imageFile)
        {
           var response = await _userRepository.EditProfile(id,email,username,imageFile);

           return response;
            
        }

        [HttpGet("getprofiledata/{id}")]
        public async Task<Response<UserDTO>> GetEmployee(string id)
        {
            // Find the user by ID
            var user = await _context.ApplicationUsers.FindAsync(id);

            if (user == null)
            {
                // User with the provided ID not found
                return new Response<UserDTO>
                {
                    StatusCode = 404,
                    StatusMessage = "User not found"
                };
            }

            // Construct the UserDTO object
            var userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                ProfilePicture = user.ProfilePicture,
                ImageSrc = $"https://{Request.Host}/ProfileImages/{user.ProfilePicture}"
            };

            return new Response<UserDTO> { Data = userDTO };
        }

        [HttpPost("uploadappartment")]
        public async Task<IActionResult> UploadAppartment([FromForm] AppartmentDTO appartment)
        {
            var response = await _userRepository.UploadAppartment(appartment);

            if (response.StatusCode == 200)
            {
                return Ok(response); // Return 200 OK with response body
            }
            else
            {
                return StatusCode(response.StatusCode, response); // Return appropriate status code with response body
            }
        }

        [HttpGet("getallappartments")]
        public async Task<ActionResult<List<AppartmentGetDTO>>> GetAllAppartments()
        {
            try
            {
                string host = Request.Host.ToString();
                var appartmentGetDTOs = await _userRepository.GetAllAppartments(host);
                return Ok(appartmentGetDTOs);
            }
            catch
            {
                // Log the exception details here, for debugging purposes
                return StatusCode(500, "An error occurred while retrieving the apartments. Please try again later.");
            }
        }
        [HttpGet("getuserappartments")]
        public async Task<IActionResult> GetUserAppartments(string userId)
        {
            try
            {
                string host = Request.Host.ToString();
                var userAppartments = await _userRepository.GetUserAppartments(userId, host);
                return Ok(userAppartments); // This will return HTTP 200 with the list of apartments
            }
            catch
            {
                // It's good to log the exception details somewhere, like a file or a logging service
                return StatusCode(500, "An error occurred while retrieving the apartments. Please try again later.");
            }
        }

        [HttpDelete("deleteapartment/{apartmentId}")]
        public async Task<IActionResult> DeleteApartment(int apartmentId)
        {
            try
            {
                // Call the delete method from the user repository
                var response = await _userRepository.DeleteApartment(apartmentId);

                // Check the response status code and return accordingly
                if (response.StatusCode == 200)
                {
                    return Ok(response); // Return 200 OK with response body
                }
                else
                {
                    return StatusCode(response.StatusCode, response); // Return appropriate status code with response body
                }
            }
            catch
            {
                // Log the exception details here, for debugging purposes
                return StatusCode(500, "An error occurred while deleting the apartment. Please try again later.");
            }
        }

        [HttpPost("saveUserListing")]
        public async Task<IActionResult> SaveUserListing(string userId, int apartmentId)
        {
            var response = await _userRepository.SaveUserListing(userId, apartmentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("getsavedlisting")]
        public async Task<List<AppartmentGetDTO>> GetSavedListingsAsync(string userId)
        {
            string host = Request.Host.ToString();
            var response = await _userRepository.GetSavedListingsAsync(userId, host);
            return response;
        }

        [HttpDelete("removeSavedListing")]
        public async Task<IActionResult> RemoveSavedListing(string userId, int apartmentId)
        {
            var response = await _userRepository.RemoveSavedListing(userId, apartmentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("editApartment")]
        public async Task<Response<Object>> EditAppartment(int id,[FromForm] AppartmentEditDTO updatedAppartment)
        {
            var response = await _userRepository.EditAppartment(id, updatedAppartment);
            return response;
        }
        [HttpPost("uploadReview")]
        public async Task<IActionResult> UploadReview([FromForm] ReviewDTO review)
        {
            var response = await _userRepository.UploadReview(review);

            if (response.StatusCode == 200)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(response.StatusCode, response);
            }
        }

        [HttpPost("sendFeedback")]
        public async Task<IActionResult> SendFeedback([FromForm] FeedbackDTO feedback)
        {
            var response = await _userRepository.SendFeedback(feedback);
            if(response.StatusCode == 200)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(response.StatusCode, response);
            }
        }

        [HttpGet("getAppartmentReview")]
        public async Task<ActionResult<List<ReviewGetDTO>>> GetAppartmentReviews(int appartmentId)
        {
            try
            {
                string host = Request.Host.ToString();
                var reviews = await _userRepository.GetAppartmentReviews(appartmentId, host);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Return an error response
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpDelete("deleteReview")]
        public async Task<Response<Object>> DeleteReview(int reviewId)
        {
           var response = await _userRepository.DeleteReview(reviewId);
            return response;
        }
        [HttpDelete("deleteFeedback")]
        public async Task<Response<Object>> DeleteFeedback(int feedbackId)
        {
            var response = await _userRepository.DeleteFeedback(feedbackId);
            return response;
        }
        [HttpGet("getAllUsers")]
        public async Task<Response<List<UserDTO>>> getAllUsers()
        {
                string host = Request.Host.ToString();
                var response = await _userRepository.GetAllUsers(host);
                return response;           
        }
        [HttpDelete("deleteUser")]
        public async Task<Response<Object>> DeleteUser(string userId)
        {
            try
            {
                var response = await _userRepository.DeleteUserAsync(userId);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex}");

                // Create a response object with the exception message
                Response<Object> response = new Response<Object>();
                response.StatusCode = 500;
                response.StatusMessage = $"Failed to delete user: {ex.Message}";
                return response;
            }
        }

        [HttpGet("getAllFeedbacks")]
        public async Task<Response<List<FeedbackGetDTO>>> getAllFeedbacks()
        {
            string host = Request.Host.ToString();
            var response = await _userRepository.getAllFeedbacks(host);
            return response;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestReservation([FromBody] RequestsDTO request)
        {
            // Call the repository method to request the reservation
            var response = await _userRepository.RequestReservation(request);

            // Check if the reservation request was successful
            if (response.StatusCode == 200)
            {
                return Ok(response); // Return a success response
            }
            else
            {
                return StatusCode(response.StatusCode, response); // Return an error response
            }
        }

        [HttpPost("accept-reservation-request")]
        public async Task<IActionResult> AcceptReservation(int requestId)
        {
            var response = await _userRepository.AcceptReservationRequest(requestId);

            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("reject-reservation-request")]
        public async Task<IActionResult> RejectReservationRequest(int requestId)
        {
            var response = await _userRepository.RejectReservationRequest(requestId);
            if (response.StatusCode == 200)
            {
                return Ok(response);
            }
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetUserReservationRequests")]
        public async Task<ActionResult<Response<List<RequestGetDTO>>>> GetUserReservationRequests(string userId)
        {
            try
            {
                string host = Request.Host.ToString();
                var response = await _userRepository.GetUserReservationRequests(userId, host);
                if (response.StatusCode == 200)
                {
                    return Ok(response);
                }
                return StatusCode(response.StatusCode, response);
            }
            catch
            {
                // Log the exception details here, for debugging purposes
                return StatusCode(500, "An error occurred while retrieving the reservation requests. Please try again later.");
            }
        }

        [HttpDelete("delete-reservation-request")]
        public async Task<ActionResult<Response<Object>>> DeleteReservationRequest(int requestId)
        {
            var response = await _userRepository.DeleteReservationRequest(requestId);

            if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            if (response.StatusCode == 500)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("GetUserReservations")]
        public async Task<ActionResult<Response<List<ReservationGetDTO>>>> getUserReservations(string userId)
        {
            try
            {
                string host = Request.Host.ToString();
                var response = await _userRepository.GetUserReservations(userId, host);
                if (response.StatusCode == 200)
                {
                    return Ok(response);
                }
                return StatusCode(response.StatusCode, response);
            }
            catch
            {
                // Log the exception details here, for debugging purposes
                return StatusCode(500, "An error occurred while retrieving the reservation requests. Please try again later.");
            }
        }

    }

}
