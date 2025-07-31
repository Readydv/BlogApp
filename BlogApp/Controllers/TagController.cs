using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using BlogApp.Services;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [Authorize] // Все методы требуют аутентификации
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private readonly IPostTagService _postTagService;
        private readonly ILogger<TagController> _logger;

        public TagController(ITagService tagService, IPostTagService postTagService, ILogger<TagController> logger)
        {
            _tagService = tagService;
            _postTagService = postTagService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Запрос списка тегов пользователем {UserName}.", User.Identity.Name);

            var tags = await _tagService.GetAllAsync();
            var viewModel = new List<TagViewModel>();

            foreach (var tag in tags)
            {
                var postCount = await _postTagService.GetPostCountForTagAsync(tag.Id); // Получаем количество статей
                _logger.LogDebug("Тег {TagId}: {TagName}, количество постов: {PostCount}",
                        tag.Id, tag.Name, postCount);

                viewModel.Add(new TagViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    PostCount = postCount
                });
            }
            _logger.LogInformation("Успешно возвращено {TagCount} тегов", viewModel.Count);
            return View("~/Views/Shared/TagManager.cshtml", viewModel);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetById(Guid id)
        {
            var tag = await _tagService.GetByIdAsync(id);
            if (tag == null)
                return NotFound();
            return Ok(tag);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagViewModel model)
        {
            _logger.LogInformation("Попытка создания нового тега '{TagName}' пользователем {UserId}",
                model.Name, User.Identity.Name);

            if (ModelState.IsValid)
            {
                var tag = new Tag { Name = model.Name };
                await _tagService.CreateAsync(tag);

                _logger.LogInformation("Тег '{TagName}' (ID: {TagId}) успешно создан",
                        tag.Name, tag.Id);
                return RedirectToAction(nameof(Index));
            }

            // Если ошибки валидации, возвращаем на страницу с текущими тегами
            var tags = await _tagService.GetAllAsync();
            var viewModel = tags.Select(t => new TagViewModel
            {
                Id = t.Id,
                Name = t.Name
            }).ToList();

            return View("~/Views/Shared/TagManager.cshtml", viewModel);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TagViewModel model)
        {
            _logger.LogInformation("Попытка редактирования тега {TagId} пользователем {UserId}",
                id, User.Identity.Name);

            if (ModelState.IsValid)
            {
                var tag = await _tagService.GetByIdAsync(id);
                if (tag == null)
                {
                    _logger.LogWarning("Тег {TagId} не найден при редактировании", id);
                    return NotFound();
                }

                _logger.LogDebug("Изменение тега: старое имя '{OldName}', новое имя '{NewName}'", tag.Name, model.Name);
                tag.Name = model.Name;
                await _tagService.UpdateAsync(tag);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Попытка удаления тега {TagId} администратором {UserId}",
                id, User.Identity.Name);

            await _tagService.DeleteAsync(id);
            _logger.LogInformation("Тег {TagId} успешно удален", id);

            return RedirectToAction(nameof(Index));
        }
    }
}