using BlogApp.Models;

namespace BlogApp.InterfaceServices
{
    public interface ICommentService
    {
        Task<Comment> CreateAsync(Comment comment);
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment> CreateFromViewAsync(Guid postId, string content, string authorId);
        Task<Comment> GetByIdAsync(Guid id);
        Task UpdateAsync (Comment comment);
        Task DeleteAsync (Guid id);
    }
}
