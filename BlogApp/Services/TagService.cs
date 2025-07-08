using BlogApp.InterfaceServices;
using BlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Services
{
    public class TagService : ITagService
    {
        private readonly BlogContext _context;

        public TagService(BlogContext context)
        {
            _context = context;
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            return await _context.Tags
                .Include(t => t.PostTags)
                .ThenInclude(pt => pt.Post)
                .ToListAsync();
        }

        public async Task<Tag> GetByIdAsync(Guid id)
        {
            return await _context.Tags
                .Include(t => t.PostTags)
                .ThenInclude(pt => pt.Post)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync (Tag tag)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null)
            {
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }
        }
    }
}
