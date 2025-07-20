using BlogApp.InterfaceServices;
using BlogApp.Models;
using BlogApp.Services;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using static BlogApp.ViewModels.PostListViewModel;

namespace BlogApp.Controllers
{
    [Route("[controller]")]
    [Authorize] 
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ITagService _tagService;
        private readonly IPostTagService _postTagService;
        private readonly ILogger<PostController> _logger;

        public PostController(IPostService postService, ITagService tagService, IPostTagService postTagService, ILogger<PostController> logger)
        {
            _postService = postService;
            _tagService = tagService;
            _postTagService = postTagService;
            _logger = logger;
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var model = new CreatePostViewModel
            {
                AvailableTags = await _tagService.GetAllAsync()
            };
            return View("CreatePost", model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            _logger.LogInformation("Начало обработки создания статьи");

            if (!ModelState.IsValid)
            {
                // Логируем все ошибки валидации
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        _logger.LogError($"Ошибка в поле {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                model.AvailableTags = await _tagService.GetAllAsync();
                return View("CreatePost", model);
            }

            try
            {
                _logger.LogInformation("Создание новой статьи: {Title}", model.Title);

                var post = new Post
                {
                    Title = model.Title,
                    Content = model.Content,
                    AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    CreatedDate = DateTime.Now,
                    ViewCount = 0
                };

                _logger.LogInformation("Перед сохранением в БД");
                var createdPost = await _postService.CreateAsync(post);
                _logger.LogInformation("Статья сохранена, ID: {Id}", createdPost.Id);

                // Обработка тегов
                if (model.SelectedTagId != null && model.SelectedTagId.Any())
                {
                    _logger.LogInformation("Добавление тегов: {TagIds}", string.Join(",", model.SelectedTagId));
                    foreach (var tagId in model.SelectedTagId)
                    {
                        await _postTagService.AddTagToPostAsync(createdPost.Id, tagId);
                    }
                }

                _logger.LogInformation("Перенаправление на AllPosts");
                return RedirectToAction("AllPosts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании статьи");
                ModelState.AddModelError("", $"Ошибка при создании статьи: {ex.Message}");
                model.AvailableTags = await _tagService.GetAllAsync();
                return View("CreatePost", model);
            }
        }

        [HttpGet("")] // Будет обрабатывать /Post
        [HttpGet("AllPosts")] // Будет обрабатывать /Post/Index
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetAllWithTagsAsync(); // Нужно реализовать этот метод в сервисе

            var model = new PostListViewModel
            {
                Posts = posts.Select(p => new PostItemViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    ViewCount = p.ViewCount,
                    Tags = p.PostTags?.Select(pt => pt.Tag.Name) ?? Enumerable.Empty<string>()
                })
            };

            return View("~/Views/Shared/AllPosts.cshtml", model);
        }

        [HttpGet("Author/{authorId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetByAuthor(string authorId)
        {
            var posts = await _postService.GetByAuthorAsync(authorId);
            return Ok(posts);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var post = await _postService.GetByIdWithDetailsAsync(id); // Нужно реализовать этот метод
            if (post == null)
            {
                return NotFound();
            }

            // Увеличиваем счетчик просмотров
            post.ViewCount++;
            await _postService.UpdateAsync(post);

            return View("~/Views/Shared/PostInfo.cshtml", post); // Создадим это представление
        }

        // Редактировать пост могут: автор, модератор, админ
        [HttpGet("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            var post = await _postService.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Проверка прав доступа
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && post.AuthorId != userId)
            {
                return Forbid();
            }

            var model = new EditPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                AvailableTags = await _tagService.GetAllAsync(),
                SelectedTagId = post.PostTags?.Select(pt => pt.TagId).ToList() ?? new List<Guid>()
            };

            return View("~/Views/Shared/EditPost.cshtml", model);
        }

        [HttpPost("edit/{id}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableTags = await _tagService.GetAllAsync();
                return View("PostEdit", model);
            }

            try
            {
                var existingPost = await _postService.GetByIdAsync(id);
                if (existingPost == null)
                {
                    return NotFound();
                }

                // Проверка прав доступа
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (userRole != "Admin" && userRole != "Moderator" && existingPost.AuthorId != userId)
                {
                    return Forbid();
                }

                // Обновляем пост
                existingPost.Title = model.Title;
                existingPost.Content = model.Content;

                await _postService.UpdateAsync(existingPost);

                // Обновляем теги
                await UpdatePostTags(id, model.SelectedTagId);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при редактировании статьи");
                ModelState.AddModelError("", $"Ошибка при редактировании статьи: {ex.Message}");
                model.AvailableTags = await _tagService.GetAllAsync();
                return View("~/Views/Shared/EditPost.cshtml", model);
            }
        }

        private async Task UpdatePostTags(Guid postId, List<Guid> selectedTagIds)
        {
            // Получаем текущие теги поста
            var currentTags = (await _postTagService.GetTagsForPostAsync(postId))
                .Select(t => t.Id)
                .ToList();

            // Теги для удаления
            var tagsToRemove = currentTags.Except(selectedTagIds ?? new List<Guid>());
            foreach (var tagId in tagsToRemove)
            {
                await _postTagService.RemoveTagFromPostAsync(postId, tagId);
            }

            // Теги для добавления
            var tagsToAdd = (selectedTagIds ?? new List<Guid>()).Except(currentTags);
            foreach (var tagId in tagsToAdd)
            {
                await _postTagService.AddTagToPostAsync(postId, tagId);
            }
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _postService.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin" && userRole != "Moderator" && post.AuthorId != userId)
            {
                return Forbid();
            }

            await _postService.DeleteAsync(id);

            return RedirectToAction("Index");
        }
    }
}