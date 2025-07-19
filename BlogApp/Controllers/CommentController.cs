using BlogApp.InterfaceServices;
using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [Route("[controller]")]
    [Authorize] // Все методы требуют аутентификации
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var comments = await _commentService.GetAllCommentsWithViewModelAsync(User);
            return View("AllComments", comments);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Comment>> GetById(Guid id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();
            return Ok(comment);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CommentCreateViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                ModelState.AddModelError(nameof(model.Content), "Комментарий не может быть пустым.");
                return RedirectToAction("GetById", "Post", new { id = model.PostId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _commentService.CreateFromViewAsync(model.PostId, model.Content, userId);

            return RedirectToAction("GetById", "Post", new { id = model.PostId });
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

        [Authorize]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && comment.AuthorId != userId)
            {
                return Forbid();
            }

            await _commentService.DeleteAsync(id);

            return RedirectToAction("GetById", "Post", new { id = comment.PostId });
        }
    }
}