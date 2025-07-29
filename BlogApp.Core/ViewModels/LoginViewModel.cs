using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите свое имя пользователя")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
