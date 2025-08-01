using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/403")]
        public IActionResult Error403()
        {
            _logger.LogWarning("Доступ запрещен (403) для пользователя {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return View();
        }

        [Route("Error/404")]
        public IActionResult Error404()
        {
            _logger.LogWarning("Страница не найдена (404) по пути: {Path}", HttpContext.Request.Path);
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            _logger.LogWarning("Ошибка {StatusCode} при обработке запроса", statusCode);

            return statusCode switch
            {
                403 => View("Error403"),
                404 => View("Error404"),
                _ => View("Error")
            };
        }

        [Route("Error")]
        public IActionResult Error() => View("Error");

        [Route("Error/AccessDenied")]
        public IActionResult AccessDenied()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning("Отказано в доступе для пользователя {UserId}", userId);
            return View();
        }

        [Route("Error/LoginError")]
        public IActionResult LoginError(string message)
        {
            _logger.LogWarning("Ошибка входа: {Message}", message);
            ViewBag.ErrorMessage = message;
            return View();
        }
    }
}
