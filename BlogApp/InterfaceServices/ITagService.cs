using BlogApp.Models;

namespace BlogApp.InterfaceServices
{
    public interface ITagService
    {
        Task<Tag> CreateAsync(Tag tag);
        Task<IEnumerable<Tag>> GetAllAsync();
        Task<Tag> GetByIdAsync(Guid id);
        Task UpdateAsync(Tag tag);
        Task DeleteAsync(Guid id);
    }
}
