using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using API.DTOs;
using BlogApp.DTOs;

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

        public async Task<Post> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Posts
        .Include(p => p.Author)
        .Include(p => p.Comments)
            .ThenInclude(c => c.Author) // <-- Это нужно
        .Include(p => p.PostTags)
            .ThenInclude(pt => pt.Tag)
        .FirstOrDefaultAsync(p => p.Id == id);
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

        public async Task<PostResponseDto> GetPostDetailsDtoAsync(Guid id)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    AuthorName = p.Author.UserName,
                    CreatedDate = p.CreatedDate,
                    ViewCount = p.ViewCount,
                    Tags = p.PostTags
                        .Select(pt => pt.Tag.Name)
                        .ToList(),
                    Comments = p.Comments
                        .Select(c => new CommentDto
                        {
                            Id = c.Id,
                            Content = c.Content,
                            AuthorName = c.Author.UserName,
                            CreatedDate = c.CreatedDate
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
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

        public async Task<IEnumerable<PostListItemDto>> GetPostListItemsByAuthorAsync(string authorId)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new PostListItemDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorName = p.Author.UserName,
                    CreatedDate = p.CreatedDate,
                    ViewCount = p.ViewCount,
                    TagNames = p.PostTags
                        .Select(pt => pt.Tag.Name)
                        .ToList()
                })
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
