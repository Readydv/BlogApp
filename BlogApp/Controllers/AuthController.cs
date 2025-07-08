using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            //Назначаем роль "User" новому пользователю
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { Message = "Регистрация прошла успешно" });
        }

        /// <summary>
        /// Логи пользователя   
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false
                );

            if (!result.Succeeded)
                return Unauthorized();

            return Ok(new {Message = "Вы успешно вошли в систему"});
        }

        /// <summary>
        /// Получение текущего пользователя
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task <IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
            });
        }
    }
}
