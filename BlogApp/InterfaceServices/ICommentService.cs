using API.DTOs;
using BlogApp.Data.Models;
using BlogApp.DTOs;
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
        Task<IEnumerable<CommentDto>> GetAllCommentsWithDtoAsync(ClaimsPrincipal user);
        Task<CommentResponseDto> GetCommentDetailsAsync(Guid id);
        Task DeleteAsync (Guid id);
    }
}
