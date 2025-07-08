using BlogApp.Models;

namespace BlogApp.InterfaceServices
{
    public interface IUserService
    {
        Task<User> CreateAsync(User user);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync (Guid id);
        Task UpdateAsync (User user);
        Task DeleteAsync (Guid id);
    }
}
