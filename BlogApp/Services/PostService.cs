using BlogApp.InterfaceServices;
using BlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Services
{
    public class PostService : IPostService
    {
        private readonly BlogContext _context;

        public PostService(BlogContext context)
        {
            _context = context;
        }

        public async Task<Post> CreateAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetAllWithTagsAsync()
        {
            return await _context.Posts
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByAuthorAsync(string authorId)
        {
            return await _context.Posts
            .Where(p => p.AuthorId == authorId)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .ToListAsync();
        }

        public async Task<Post> GetByIdAsync(Guid id)
        {
            return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync (Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
