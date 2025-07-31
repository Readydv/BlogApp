using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ITagService _tagService;
        private readonly IPostTagService _postTagService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(IPostService postService, ITagService tagService, IPostTagService postTagService, ILogger<PostsController> logger)
        {
            _postService = postService;
            _tagService = tagService;
            _postTagService = postTagService;
            _logger = logger;
        }

        // ✅ Создание поста
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CreatedDate = DateTime.UtcNow,
                ViewCount = 0
            };

            var createdPost = await _postService.CreateAsync(post);

            if (model.SelectedTagId != null && model.SelectedTagId.Any())
            {
                foreach (var tagId in model.SelectedTagId)
                {
                    await _postTagService.AddTagToPostAsync(createdPost.Id, tagId);
                }
            }

            return Ok(createdPost);
        }

        // ✅ Получить все посты
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postService.GetAllWithTagsAsync();

            var result = posts.Select(p => new
            {
                p.Id,
                p.Title,
                p.ViewCount,
                Tags = p.PostTags?.Select(pt => pt.Tag.Name)
            });

            return Ok(result);
        }

        // ✅ Получить по автору
        [HttpGet("author/{authorId}")]
        public async Task<IActionResult> GetByAuthor(string authorId)
        {
            var posts = await _postService.GetByAuthorAsync(authorId);
            return Ok(posts);
        }

        // ✅ Получить по Id
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var post = await _postService.GetByIdWithDetailsAsync(id);
            if (post == null) return NotFound();

            post.ViewCount++;
            await _postService.UpdateAsync(post);

            return Ok(post);
        }

        // ✅ Редактировать пост
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] EditPostDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingPost = await _postService.GetByIdAsync(id);
            if (existingPost == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && existingPost.AuthorId != userId)
            {
                return Forbid();
            }

            existingPost.Title = model.Title;
            existingPost.Content = model.Content;
            await _postService.UpdateAsync(existingPost);

            await UpdatePostTags(id, model.SelectedTagId);

            return Ok(existingPost);
        }

        // ✅ Удаление поста
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _postService.GetByIdAsync(id);
            if (post == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && post.AuthorId != userId)
            {
                return Forbid();
            }

            await _postService.DeleteAsync(id);
            return Ok(new { message = "Post deleted successfully" });
        }

        private async Task UpdatePostTags(Guid postId, List<Guid> selectedTagIds)
        {
            var currentTags = (await _postTagService.GetTagsForPostAsync(postId))
                .Select(t => t.Id)
                .ToList();

            var tagsToRemove = currentTags.Except(selectedTagIds ?? new List<Guid>());
            foreach (var tagId in tagsToRemove)
            {
                await _postTagService.RemoveTagFromPostAsync(postId, tagId);
            }

            var tagsToAdd = (selectedTagIds ?? new List<Guid>()).Except(currentTags);
            foreach (var tagId in tagsToAdd)
            {
                await _postTagService.AddTagToPostAsync(postId, tagId);
            }
        }
    }
}
