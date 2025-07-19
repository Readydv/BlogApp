using BlogApp.InterfaceServices;
using BlogApp.Models;
using BlogApp.Services;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    [Route("[controller]")]
    [Authorize] // Все методы требуют аутентификации
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private readonly IPostTagService _postTagService;

        public TagController(ITagService tagService, IPostTagService postTagService)
        {
            _tagService = tagService;
            _postTagService = postTagService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllAsync();
            var viewModel = new List<TagViewModel>();

            foreach (var tag in tags)
            {
                var postCount = await _postTagService.GetPostCountForTagAsync(tag.Id); // Получаем количество статей
                viewModel.Add(new TagViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    PostCount = postCount
                });
            }

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
            if (ModelState.IsValid)
            {
                var tag = new Tag { Name = model.Name };
                await _tagService.CreateAsync(tag);
                return RedirectToAction(nameof(Index));
            }

            // Если ошибки валидации, возвращаем на страницу с текущими тегами
            var tags = await _tagService.GetAllAsync();
            var viewModel = tags.Select(t => new TagViewModel
            {
                Id = t.Id,
                Name = t.Name
            }).ToList();

            return View("Index", viewModel);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TagViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tag = await _tagService.GetByIdAsync(id);
                if (tag == null)
                {
                    return NotFound();
                }

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
            await _tagService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}