using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required (ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя долнжо содержать от 3 до 50 символов")]
        public string UserName { get; set; }
        [Required (ErrorMessage = "Email обязателен")]
        [EmailAddress (ErrorMessage = "Некорректный формат Email")]
        public string Email { get; set; }
        [Required (ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        public string Password { get; set; }
        [Required (ErrorMessage = "Подтверждение пароля обязательно")]
        [Compare("Password", ErrorMessage = "Пароли должны совпадать")]
        public string ConfirmPassword { get; set; }
    }
}
