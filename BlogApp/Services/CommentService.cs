using BlogApp.InterfaceServices;
using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogApp.Services
{
    public class CommentService : ICommentService
    {
        private readonly BlogContext _context;

        public CommentService(BlogContext context)
        {
            _context = context;
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            comment.CreatedDate = DateTime.UtcNow;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> CreateFromViewAsync(Guid postId, string content, string authorId)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Комментарий не может быть пустым.");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                Content = content,
                CreatedDate = DateTime.UtcNow,
                AuthorId = authorId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<IEnumerable<CommentViewModel>> GetAllCommentsWithViewModelAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrModerator = user.IsInRole("Admin") || user.IsInRole("Moderator");

            var query = _context.Comments
        .Include(c => c.Author)
        .Include(c => c.Post)
        .AsQueryable();

            // Если не админ/модератор - показываем только свои комментарии
            if (!isAdminOrModerator)
            {
                query = query.Where(c => c.AuthorId == userId);
            }

            return await query
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorName = c.Author.UserName,
                    AuthorId = c.AuthorId,
                    CreatedDate = c.CreatedDate,
                    PostId = c.PostId,
                    PostTitle = c.Post.Title,
                    CanDelete = isAdminOrModerator || c.AuthorId == userId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllAsync ()
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Post)
                .ToListAsync();
        }

        public async Task<Comment> GetByIdAsync(Guid id)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if(comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
