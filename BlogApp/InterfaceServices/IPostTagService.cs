using BlogApp.Data.Models;

namespace BlogApp.InterfaceServices
{
    public interface IPostTagService
    {
        Task AddTagToPostAsync(Guid postId, Guid tagId);
        Task<IEnumerable<Tag>> GetTagsForPostAsync(Guid postId);
        Task<int> GetPostCountForTagAsync(Guid tagId);
        Task RemoveTagFromPostAsync(Guid postId, Guid tagId);
    }
}
