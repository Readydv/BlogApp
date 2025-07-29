using BlogApp.Data.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET /api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();

            var result = new List<UserItemViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                result.Add(new UserItemViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = userRoles.ToList(),
                    RoleDescriptions = roles.ToDictionary(r => r.Name, r => r.Description)
                });
            }

            return Ok(result);
        }

        // GET /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                Roles = roles
            });
        }

        // PUT /api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.UserName = model.UserName ?? user.UserName;
            user.Email = model.Email ?? user.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return BadRequest(updateResult.Errors);

            await UpdateUserRoles(user, model.SelectedRoles ?? new List<string>());

            return Ok(new { Message = "Пользователь успешно обновлён" });
        }

        // DELETE /api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        // PUT /api/users/{id}/password
        [HttpPut("{id}/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            if (user.Id.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Admin"))
                return Forbid();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { Message = "Пароль успешно изменен" });
        }

        private async Task UpdateUserRoles(ApplicationUser user, List<string> selectedRoles)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();
            if (rolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            if (rolesToAdd.Any())
                await _userManager.AddToRolesAsync(user, rolesToAdd);
        }
    }
}
