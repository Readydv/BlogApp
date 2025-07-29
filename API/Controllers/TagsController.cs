using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly IPostTagService _postTagService;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ITagService tagService, IPostTagService postTagService, ILogger<TagsController> logger)
        {
            _tagService = tagService;
            _postTagService = postTagService;
            _logger = logger;
        }

        // GET /api/tags
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _tagService.GetAllAsync();
            var viewModel = new List<TagViewModel>();

            foreach (var tag in tags)
            {
                var postCount = await _postTagService.GetPostCountForTagAsync(tag.Id);
                viewModel.Add(new TagViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    PostCount = postCount
                });
            }

            return Ok(viewModel);
        }

        // GET /api/tags/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tag = await _tagService.GetByIdAsync(id);
            if (tag == null) return NotFound();
            return Ok(tag);
        }

        // POST /api/tags
        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TagViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tag = new Tag { Name = model.Name };
            await _tagService.CreateAsync(tag);

            return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
        }

        // PUT /api/tags/{id}
        [Authorize(Roles = "Admin,User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] TagViewModel model)
        {
            var tag = await _tagService.GetByIdAsync(id);
            if (tag == null) return NotFound();

            tag.Name = model.Name;
            await _tagService.UpdateAsync(tag);

            return Ok(new { message = "Тег обновлён", tag });
        }

        // DELETE /api/tags/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _tagService.DeleteAsync(id);
            return NoContent();
        }
    }
}
