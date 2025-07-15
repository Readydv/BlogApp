using BlogApp.Models;
using System.Runtime.CompilerServices;

namespace BlogApp.InterfaceServices
{
    public interface IPostService
    {
        Task<Post> CreateAsync(Post post);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<IEnumerable<Post>> GetAllWithTagsAsync();
        Task<IEnumerable<Post>> GetByAuthorAsync(string authorId);
        Task<Post> GetByIdWithDetailsAsync(Guid id);
        Task<Post> GetByIdAsync(Guid id);
        Task UpdateAsync(Post post);
        Task DeleteAsync(Guid id);
    }
}
