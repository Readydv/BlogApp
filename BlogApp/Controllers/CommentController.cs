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
            _logger.LogInformation("Пользователь {UserId} запросил список комментариев.",
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            var comments = await _commentService.GetAllCommentsWithViewModelAsync(User);
            return View("AllComments", comments);
        }

        [Authorize]
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(Guid id, string returnUrl = null)
        {
            _logger.LogInformation("Пользователь {UserId} запросил редактирование комментария {CommentId}.",
                User.FindFirstValue(ClaimTypes.NameIdentifier), id);

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                _logger.LogWarning("Комментарий с ID {CommentId} не найден при попытке редактирования.", id);
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrModerator = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (!isAdminOrModerator && comment.AuthorId != userId)
            {
                _logger.LogWarning("Попытка несанкционированного редактирования комментария {CommentId} пользователем {UserId}.",
                    id, userId);
                return Forbid();
            }

            var model = new CommentEditViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                PostId = comment.PostId
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Shared/EditComment.cshtml", model);
        }

        [Authorize]
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CommentEditViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Редактирование комментария не удалось: модель не валидна.");
                return View("~/Views/Shared/EditComment.cshtml", model);
            }

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                _logger.LogWarning("Комментарий с ID {CommentId} не найден при попытке сохранения", id);
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrModerator = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (!isAdminOrModerator && comment.AuthorId != userId)
            {
                _logger.LogWarning("Попытка несанкционированного сохранения комментария {CommentId} пользователем {UserId}",
                    id, userId);
                return Forbid();
            }

            _logger.LogInformation("Сохранение комментария {CommentId} пользователем {UserId}", id, userId);

            comment.Content = model.Content;
            await _commentService.UpdateAsync(comment);

            // Универсальный редирект
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Дефолтный редирект, если returnUrl не указан
            return RedirectToAction("Index");
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CommentCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                _logger.LogWarning("Попытка создания пустого комментария к посту {PostId} пользователем {UserId}",
                    model.PostId, userId);

                ModelState.AddModelError(nameof(model.Content), "Комментарий не может быть пустым.");
                return RedirectToAction("GetById", "Post", new { id = model.PostId });
            }
            _logger.LogInformation("Создание комментария к посту {PostId} пользователем {UserId}",
                model.PostId, userId);

            await _commentService.CreateFromViewAsync(model.PostId, model.Content, userId);

            return RedirectToAction("GetById", "Post", new { id = model.PostId });
        }


        [Authorize]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Запрос на удаление комментария {CommentId} пользователем {UserId}",
                id, userId);

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                _logger.LogWarning("Комментарий с ID {CommentId} не найден при попытке удаления", id);
                return NotFound();
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && comment.AuthorId != userId)
            {
                _logger.LogWarning("Попытка несанкционированного удаления комментария {CommentId}", id);
                return Forbid();
            }

            await _commentService.DeleteAsync(id);
            _logger.LogInformation("Комментарий {CommentId} успешно удален пользователем {UserId}",
                id, userId);

            return RedirectToAction("GetById", "Post", new { id = comment.PostId });
        }
    }
}