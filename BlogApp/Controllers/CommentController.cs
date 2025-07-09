using BlogApp.InterfaceServices;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Все методы требуют аутентификации
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetAll()
        {
            var comments = await _commentService.GetAllAsync();
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetById(Guid id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();
            return Ok(comment);
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> Create([FromBody] Comment model)
        {
            model.AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var created = await _commentService.CreateAsync(model);
            return Ok(created);
        }

        // Редактировать комментарии могут админ, модератор и автор
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Comment model)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && comment.AuthorId != userId)
                return Forbid();

            comment.Content = model.Content;
            await _commentService.UpdateAsync(comment);

            return NoContent();
        }

        // Удалять комментарии могут админ, модератор и автор
        [Authorize(Roles = "Admin,Moderator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && comment.AuthorId != userId)
                return Forbid();

            await _commentService.DeleteAsync(id);
            return NoContent();
        }
    }
}