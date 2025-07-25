using BlogApp.InterfaceServices;
using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [Route("[controller]")]
    [Authorize (Roles = "Admin")] // Все методы требуют аутентификации
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Запрос списка пользователей администратором.");
            // 1. Получаем данные последовательно
            var users = await _userManager.Users
                .AsNoTracking()
                .ToListAsync();

            var roles = await _roleManager.Roles
                .AsNoTracking()
                .ToListAsync();

            // 2. Собираем информацию о ролях пользователей
            var userRolesDict = new Dictionary<string, List<string>>();
            var roleDescriptionsDict = roles.ToDictionary(r => r.Name, r => r.Description);

            foreach (var user in users)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                userRolesDict[user.Id] = rolesForUser.ToList();
                _logger.LogDebug("Обработан пользователь {UserId} с ролями: {Roles}",
                        user.Id, string.Join(", ", rolesForUser));
            }

            // 3. Строим модель представления
            var model = new UserListViewModel
            {
                Users = users.Select(u => new UserItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Roles = userRolesDict.TryGetValue(u.Id, out var rolesList) ? rolesList : new List<string>(),
                    RoleDescriptions = roleDescriptionsDict
                }),
                AvailableRoles = roles
            };

            _logger.LogInformation("Успешно возвращено {UserCount} пользователей", users.Count);

            return View("~/Views/Shared/AllUsers.cshtml", model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Запрос данных пользователя {UserId} администратором",
                id);

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning("Пользователь {UserId} не найден", id);
                return NotFound();
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email
            });
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            _logger.LogInformation("Запрос формы редактирования пользователя {UserId} администратором", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Пользователь {UserId} не найден при запросе формы редактирования", id);
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                AvailableRoles = await _roleManager.Roles.ToListAsync(),
                SelectedRoles = (await _userManager.GetRolesAsync(user)).ToList()
            };

            _logger.LogDebug("Данные для редактирования пользователя {UserId} успешно подготовлены", id);
            return View("~/Views/Account/EditUser.cshtml", model);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            _logger.LogInformation("Попытка обновления пользователя {UserId} администратором.",
                id);

            // Загружаем роли перед проверкой модели
            model.AvailableRoles = await _roleManager.Roles.ToListAsync();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Некорректные данные при редактировании пользователя {UserId}: {Errors}",
                    id, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors)));
                return View("~/Views/Account/EditUser.cshtml", model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Пользователь {UserId} не найден при попытке обновления", id);
                return NotFound();
            }

            _logger.LogDebug("Текущие данные пользователя: Name={CurrentName}, Email={CurrentEmail}",
                    user.UserName, user.Email);

            // Обновляем только измененные поля
            if (user.UserName != model.UserName)
            {
                _logger.LogInformation("Изменение имени пользователя с {OldName} на {NewName}",
                        user.UserName, model.UserName);
                user.UserName = model.UserName;
            }

            if (user.Email != model.Email)
            {
                _logger.LogInformation("Изменение email пользователя с {OldEmail} на {NewEmail}",
                        user.Email, model.Email);
                user.Email = model.Email;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Ошибка обновления пользователя {UserId}: {Errors}",
                            id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                AddErrors(updateResult);
                return View("~/Views/Account/EditUser.cshtml", model);
            }

            // Обновление ролей
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>());
            var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(currentRoles);

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded) AddErrors(removeResult);
            }

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded) AddErrors(addResult);
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/EditUser.cshtml", model);
            }

            return RedirectToAction(nameof(Index));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task UpdateUserRoles(ApplicationUser user, List<string> selectedRoles)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Удалить невыбранные роли
            var rolesToRemove = currentRoles.Except(selectedRoles ?? new List<string>());
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            // Добавить новые роли
            var rolesToAdd = (selectedRoles ?? new List<string>()).Except(currentRoles);
            await _userManager.AddToRolesAsync(user, rolesToAdd);
        }


        // Удалять пользователей может только админ
        [Authorize(Roles = "Admin")]
        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Попытка удаления пользователя {UserId} администратором.",
                id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Пользователь {UserId} не найден при попытке удаления", id);
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Пользователь {UserId} успешно удален", id);
                return RedirectToAction(nameof(Index)); // <-- вот так нужно
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(Index));
        }

        // Смена пароля — авторизованный пользователь или админ
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            if (user.Id.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Admin"))
                return Forbid();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "Пароль успешно изменен" });
        }
    }
}
