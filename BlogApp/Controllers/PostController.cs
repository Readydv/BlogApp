using BlogApp.InterfaceServices;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Post model)
        {
            model.AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var created = await _postService.CreateAsync(model);
            return Ok(created);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Post>>> GetAll()
        {
            var posts = await _postService.GetAllAsync();
            return Ok(posts);
        }

        [HttpGet("Author/{authorId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetByAuthor(string authorId)
        {
            var posts = await _postService.GetByAuthorAsync(authorId);
            return Ok(posts);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Post>> GetById(Guid id)
        {
            var post = await _postService.GetByIdAsync(id);
            if (post == null)
                return NotFound();
            return Ok(post);
        }

        // Редактировать пост могут: автор, модератор, админ
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Post post)
        {
            var existingPost = await _postService.GetByIdAsync(id);
            if (existingPost == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Проверка: если не админ и не модератор, можно редактировать только свои посты
            if (userRole != "Admin" && userRole != "Moderator" && existingPost.AuthorId != userId)
                return Forbid("Вы не можете редактировать этот пост.");

            post.AuthorId = existingPost.AuthorId;

            await _postService.UpdateAsync(post);
            return NoContent();
        }

        // Удалять пост могут: автор, модератор, админ
        [Authorize(Roles = "Admin,Moderator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _postService.GetByIdAsync(id);
            if (post == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && post.AuthorId != userId)
                return Forbid("Вы не можете удалить этот пост.");

            await _postService.DeleteAsync(id);
            return NoContent();
        }
    }
}