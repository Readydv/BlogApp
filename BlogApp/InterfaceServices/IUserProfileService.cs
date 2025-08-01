using BlogApp.Data.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.InterfaceServices
{
    public interface IUserProfileService
    {
        Task<UserProfileViewModel> GetUserProfileAsync(string userId);
        Task<ApplicationUser> GetUserWithPostsAndTagsAsync(string userId);
        Task<IdentityResult> DeleteUserWithContentAsync(string userId);
    }
}
