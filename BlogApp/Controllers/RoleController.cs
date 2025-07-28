using BlogApp.Data.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    public class RoleController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<RoleController> _logger;

        public RoleController(RoleManager<Role> roleManager, ILogger<RoleController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = _roleManager.Roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            }).ToList();

            return View("~/Views/Shared/RoleManager.cshtml", roles);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            _logger.LogInformation("Попытка создания новой роли {RoleName} пользователем {UserId}", model.Name, User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (ModelState.IsValid)
            {
                var role = new Role
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    Description = model.Description
                };
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Роль {RoleName} успешно создана", model.Name);
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Ошибка создания роли: {Error}", error.Description);
                    ModelState.AddModelError("", error.Description);
                }
            }

            var roles = _roleManager.Roles.Select(r => new RoleViewModel
            {
                Name = r.Name,
                Description = r.Description
            }).ToList();

            return View("~/Views/Shared/RoleManager.cshtml", roles);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RoleViewModel model)
        {
            _logger.LogInformation("Попытка изменения роли {RoleId} пользователем {UserId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                _logger.LogWarning("Роль {RoleId} не найдена при попытке изменения", id);
                return NotFound();
            }

            role.Name = model.Name;
            role.Description = model.Description;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Ошибка изменения роли: {Error}", error.Description);
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Попытка удаления роли {RoleId} пользователем {UserId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                _logger.LogWarning("Роль {RoleId} не найдена при попытке удаления", id);
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                _logger.LogError("Ошибка удаления роли {RoleName}: {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                ModelState.AddModelError("", "Ошибка при удалении роли");
            }

            _logger.LogInformation("Роль {RoleName} успешно удалена", role.Name);
            return RedirectToAction(nameof(Index));
        }
    }
}