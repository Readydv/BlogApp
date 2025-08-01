using API.DTOs;
using BlogApp.DTOs;
using BlogApp.InterfaceServices;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAll()
        {
            var comments = await _commentService.GetAllCommentsWithDtoAsync(User);
            return Ok(comments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentCreateDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(model.Content))
                return BadRequest("Комментарий не может быть пустым");

            var created = await _commentService.CreateFromViewAsync(model.PostId, model.Content, userId);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CommentResponseDto>> Update(Guid id, [FromBody] CommentEditDto model)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrModerator = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (!isAdminOrModerator && comment.AuthorId != userId)
                return Forbid();

            comment.Content = model.Content;
            await _commentService.UpdateAsync(comment);

            // Получаем обновленный комментарий с нужными данными автора
            var updatedComment = await _commentService.GetCommentDetailsAsync(id);

            return Ok(updatedComment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && !User.IsInRole("Moderator") && comment.AuthorId != userId)
                return Forbid();

            await _commentService.DeleteAsync(id);
            return Ok(new { message = "Комментарий удалён" });
        }
    }
}
