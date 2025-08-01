using BlogApp.Data.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(RoleManager<Role> roleManager, ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET /api/roles
        [HttpGet]
        public IActionResult GetAll()
        {
            var roles = _roleManager.Roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            }).ToList();

            return Ok(roles);
        }

        // POST /api/roles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleViewModel model)
        {
            _logger.LogInformation("Создание роли {RoleName} пользователем {UserId}",
                model.Name, User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Description = model.Description
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
                return CreatedAtAction(nameof(GetAll), new { id = role.Id }, role);

            return BadRequest(result.Errors);
        }

        // PUT /api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] RoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            role.Name = model.Name;
            role.Description = model.Description;

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
                return Ok(role);

            return BadRequest(result.Errors);
        }

        // DELETE /api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
                return NoContent();

            return BadRequest(result.Errors);
        }
    }
}
