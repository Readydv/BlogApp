using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/403")]
        public IActionResult Error403() => View();

        [Route("Error/404")]
        public IActionResult Error404() => View();

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
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
            return View();
        }

        [Route("Error/LoginError")]
        public IActionResult LoginError(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }
    }
}
