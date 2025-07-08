using BlogApp.InterfaceServices;
using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        //[HttpPost("register")]
        //public async Task<ActionResult<User>> Register(User user)
        //{
        //    var created = await _userService.CreateAsync(user);
        //    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        //}

        /// <summary>
        /// Получить всех пользователей (только для админа)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email
            }).ToList();

            return Ok(users);
        }

        /// <summary>
        /// Получить одного пользователя по Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email
            });
        }

        /// <summary>
        /// Обновить данные пользователя
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if(user == null)
                return NotFound();

            //Только админ может редактировать других пользователей
            if (user.Id.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Admin"))
                return Forbid();

            user.UserName = model.UserName;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Удалить пользователя
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if(!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        ///<summary>
        ///Сменить пароль
        ///</summary>
        [HttpPost ("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            //Только админ может менять пароль других пользователей
            if (user.Id.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Admin"))
                return Forbid();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(new {Message = "Пароль успешно изменен" });
        }
    }
}
