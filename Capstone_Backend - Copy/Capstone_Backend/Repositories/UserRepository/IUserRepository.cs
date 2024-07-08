using Capstone_Backend.Models;
using Capstone_Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capstone_Backend.Repositories.UserRepository
{
    public interface IUserRepository
    {
        Task<string> SaveProfileImage(IFormFile imageFile);
        Task<string> SaveAppartmentImage(IFormFile appartmentFile, int appartmentId);
        Task<Response<Object>> EditProfile(string id, string email, string username, IFormFile image);
        Task<Response<AppartmentDTO>> UploadAppartment(AppartmentDTO appartment);
        Task<Category> GetCategoryByName(string categoryName);
        Task<List<string>> AddImagesUrls(List<IFormFile> imageFiles, int appartmentId);
        Task<List<AppartmentGetDTO>> GetAllAppartments(string host);
        Task<List<AppartmentGetDTO>> GetUserAppartments(string userId, string host);
        Task<Response<Object>> DeleteApartment(int apartmentId);
        Task<Response<Object>> SaveUserListing(string userId, int apartmentId);
        Task<List<AppartmentGetDTO>> GetSavedListingsAsync(string userId,string host);
        Task<Response<object>> RemoveSavedListing(string userId, int apartmentId);
        Task<Response<Object>> EditAppartment(int id, AppartmentEditDTO updatedAppartment);
        Task<Response<ReviewDTO>> UploadReview(ReviewDTO review);
        Task<List<ReviewGetDTO>> GetAppartmentReviews(int appartmentId,string host);
        Task<Response<Object>> DeleteReview(int reviewId);
        Task<Response<List<UserDTO>>> GetAllUsers(string host);
        Task<Response<Object>> DeleteUserAsync(String userId);
        Task<Response<FeedbackDTO>> SendFeedback(FeedbackDTO feedback);
        Task<Response<List<FeedbackGetDTO>>> getAllFeedbacks(string host);
        Task<Response<Object>> DeleteFeedback(int feedbackId);
        Task<Response<Object>> RequestReservation(RequestsDTO request);
        Task<Response<Object>> AcceptReservationRequest(int requestId);
        Task<Response<Object>> RejectReservationRequest(int requestId);
        Task<Response<List<RequestGetDTO>>> GetUserReservationRequests(string userId, string host);
        Task<Response<Object>> DeleteReservationRequest(int requestId);
        Task<Response<List<ReservationGetDTO>>> GetUserReservations(string userId, string host);




    }
}
