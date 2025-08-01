using BlogApp.Data.Models;
using BlogApp.DTOs;
using BlogApp.InterfaceServices;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager, ILogger<UsersController> logger, IUserProfileService userProfileService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _userProfileService = userProfileService;
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
        public async Task<ActionResult<UserResponseDto>> UpdateUser(string id, [FromBody] UserUpdateDto dto)
        {
            // Проверка соответствия ID
            if (id != dto.Id)
                return BadRequest("ID в URL и теле запроса не совпадают");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Обновление данных
            user.UserName = dto.UserName ?? user.UserName;
            user.Email = dto.Email ?? user.Email;

            // Обновление ролей
            await UpdateUserRoles(user, dto.Roles);

            await _userManager.UpdateAsync(user);

            // Возвращаем обновленные данные
            return Ok(new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = dto.Roles
            });
        }

        // DELETE /api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userProfileService.DeleteUserWithContentAsync(id);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
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
