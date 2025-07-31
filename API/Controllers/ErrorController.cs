using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        // 403 Forbidden
        [HttpGet("403")]
        public IActionResult Error403()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning("Доступ запрещен (403) для пользователя {UserId}", userId);

            return StatusCode(403, new
            {
                error = "Доступ запрещён",
                userId = userId
            });
        }

        // 404 Not Found
        [HttpGet("404")]
        public IActionResult Error404()
        {
            _logger.LogWarning("Страница не найдена (404) по пути: {Path}", HttpContext.Request.Path);

            return NotFound(new
            {
                error = "Страница не найдена",
                path = HttpContext.Request.Path
            });
        }

        // Универсальный обработчик
        [HttpGet("{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            _logger.LogWarning("Ошибка {StatusCode} при обработке запроса", statusCode);

            return StatusCode(statusCode, new
            {
                error = $"Ошибка {statusCode}"
            });
        }

        // Общая ошибка
        [HttpGet]
        public IActionResult Error()
        {
            return StatusCode(500, new
            {
                error = "Произошла ошибка на сервере"
            });
        }

        // AccessDenied
        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning("Отказано в доступе для пользователя {UserId}", userId);

            return Forbid();
        }

        // Ошибка входа
        [HttpGet("LoginError")]
        public IActionResult LoginError([FromQuery] string message)
        {
            _logger.LogWarning("Ошибка входа: {Message}", message);

            return BadRequest(new
            {
                error = "Ошибка входа",
                details = message
            });
        }
    }
}