using BlogApp.InterfaceServices;
using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUserProfileService userProfileService, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userProfileService = userProfileService;
            _logger = logger;
        }

        [HttpGet]
        [Route("Register")]
        public IActionResult Register()
        {
            _logger.LogInformation("Открыта форма регистрации.");
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Регистрация не удалась: модель не валидна");
                // Возвращаем модель с ошибками в то же представление
                return View("~/Views/Home/Index.cshtml", new MainViewModel
                {
                    RegisterViewModel = model,
                    LoginViewModel = new LoginViewModel(),
                    ActiveForm = "register"
                });
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Регистрация пользователя {UserName} не удалась: {Errors}", model.UserName);
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("~/Views/Home/Index.cshtml", new MainViewModel
                {
                    RegisterViewModel = model,
                    LoginViewModel = new LoginViewModel(),
                    ActiveForm = "register"
                });
            }

            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("Пользователь {UserName} успешно зарегистрирован и вошел в систему.", model.UserName);

            // Перенаправляем на главную страницу после успешной регистрации
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            _logger.LogInformation("Открыта форма входа.");
            return View();
        }

        [HttpPost]
        [Route("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Вход пользователя не удался: модель не валидна.");
                return View("~/Views/Home/Index.cshtml", new MainViewModel
                {
                    LoginViewModel = model,
                });
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Неудачная попытка входа для пользователя: {UserName}.", model.UserName);
                ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль");
                return View("~/Views/Home/Index.cshtml", new MainViewModel
                {
                    LoginViewModel = model,
                });
            }

            _logger.LogInformation("Пользователь {UserName} успешно вошёл в систему.", model.UserName);
            // Перенаправляем на главную страницу (Index)
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Пользователь {UserName} вышел из системы.", userName);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet("Me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Пользователь {UserId} открыл свой профиль.", userId);
            var model = await _userProfileService.GetUserProfileAsync(userId);
            return View("~/Views/Account/Profile.cshtml", model);
        }

        [Authorize]
        [HttpGet("Profile/{userId?}")]
        public async Task<IActionResult> Profile(string userId = null)
        {
            userId ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await _userProfileService.GetUserProfileAsync(userId);

            if (model.User == null)
            {
                _logger.LogWarning("Профиль пользователя с ID {UserId} не найден.", userId);
                return NotFound();
            }

            _logger.LogInformation("Профиль пользователя с ID {UserId} просмотрен.", userId);
            return View("~/Views/Account/Profile.cshtml", model);
        }
    }
}