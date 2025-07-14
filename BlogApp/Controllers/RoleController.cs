using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    public class RoleController : Controller
    {
        private readonly RoleManager<Role> _roleManager;

        public RoleController(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
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
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
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
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            role.Name = model.Name;
            role.Description = model.Description;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Ошибка при удалении роли");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}