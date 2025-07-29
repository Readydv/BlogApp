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
        public async Task<IActionResult> GetAll()
        {
            var comments = await _commentService.GetAllCommentsWithViewModelAsync(User);
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();
            return Ok(comment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(model.Content))
                return BadRequest("Комментарий не может быть пустым");

            var created = await _commentService.CreateFromViewAsync(model.PostId, model.Content, userId);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CommentEditViewModel model)
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

            return Ok(comment);
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
