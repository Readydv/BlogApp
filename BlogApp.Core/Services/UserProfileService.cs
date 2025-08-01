using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using BlogApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;

namespace BlogApp.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly BlogContext _context;

        public UserProfileService(BlogContext context)
        {
            _context = context;
        }

        public async Task<UserProfileViewModel> GetUserProfileAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                .Include(u => u.Comments)
                    .ThenInclude(c => c.Post)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var posts = user.Posts?.ToList() ?? new List<Post>();
            var comments = user.Comments?.ToList() ?? new List<Comment>();

            var tags = posts
                .SelectMany(p => p.PostTags?.Select(pt => pt.Tag) ?? Enumerable.Empty<Tag>())
                .Concat(comments.SelectMany(c => c.Post?.PostTags?.Select(pt => pt.Tag) ?? Enumerable.Empty<Tag>()))
                .Distinct()
                .ToList();

            return new UserProfileViewModel
            {
                User = user,
                Posts = posts,
                Comments = comments,
                Tags = tags
            };
        }

        public async Task<ApplicationUser> GetUserWithPostsAndTagsAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
