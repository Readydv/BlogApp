using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using BlogApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly BlogContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfileService(BlogContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<IdentityResult> DeleteUserWithContentAsync(string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Находим пользователя
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "User not found"
                    });
                }

                // 2. Удаляем комментарии пользователя
                var comments = await _context.Comments
                    .Where(c => c.AuthorId == userId)
                    .ToListAsync();

                _context.Comments.RemoveRange(comments);
                await _context.SaveChangesAsync();

                // 3. Удаляем посты пользователя (вместе с их тегами)
                var posts = await _context.Posts
                    .Include(p => p.PostTags)
                    .Where(p => p.AuthorId == userId)
                    .ToListAsync();

                foreach (var post in posts)
                {
                    // Удаляем связи с тегами
                    _context.PostTags.RemoveRange(post.PostTags);
                }

                _context.Posts.RemoveRange(posts);
                await _context.SaveChangesAsync();

                // 4. Удаляем самого пользователя
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Ошибка удаления пользователя и постов"
                });
            }
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
