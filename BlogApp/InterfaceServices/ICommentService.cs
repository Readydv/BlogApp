using BlogApp.Models;
using BlogApp.ViewModels;
using System.Security.Claims;

namespace BlogApp.InterfaceServices
{
    public interface ICommentService
    {
        Task<Comment> CreateAsync(Comment comment);
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment> CreateFromViewAsync(Guid postId, string content, string authorId);
        Task<Comment> GetByIdAsync(Guid id);
        Task UpdateAsync (Comment comment);
        Task<IEnumerable<CommentViewModel>> GetAllCommentsWithViewModelAsync(ClaimsPrincipal user);
        Task DeleteAsync (Guid id);
    }
}
