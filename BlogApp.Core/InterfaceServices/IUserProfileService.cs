using BlogApp.Data.Models;
using BlogApp.ViewModels;

namespace BlogApp.InterfaceServices
{
    public interface IUserProfileService
    {
        Task<UserProfileViewModel> GetUserProfileAsync(string userId);
        Task<ApplicationUser> GetUserWithPostsAndTagsAsync(string userId);
    }
}
