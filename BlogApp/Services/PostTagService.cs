using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;

namespace BlogApp.Services
{
    public class PostTagService : IPostTagService
    {
        private readonly BlogContext _context;

        public PostTagService(BlogContext context)
        {
            _context = context;
        }

        public async Task AddTagToPostAsync(Guid postId, Guid tagId)
        {
            // Check if relationship already exists
            var exists = await _context.PostTags
                .AnyAsync(pt => pt.PostId == postId && pt.TagId == tagId);

            if (!exists)
            {
                var postTag = new PostTag
                {
                    PostId = postId,
                    TagId = tagId
                };

                _context.PostTags.Add(postTag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetPostCountForTagAsync(Guid tagId)
        {
            return await _context.PostTags.CountAsync(pt => pt.TagId == tagId);
        }

        public async Task<IEnumerable<Tag>> GetTagsForPostAsync(Guid postId)
        {
            return await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .Include(pt => pt.Tag)
                .Select(pt => pt.Tag)
                .ToListAsync();
        }

        public async Task RemoveTagFromPostAsync(Guid postId, Guid tagId)
        {
            var postTag = await _context.PostTags
                .FirstOrDefaultAsync(pt => pt.PostId == postId && pt.TagId == tagId);

            if (postTag != null)
            {
                _context.PostTags.Remove(postTag);
                await _context.SaveChangesAsync();
            }
        }
    }
}
