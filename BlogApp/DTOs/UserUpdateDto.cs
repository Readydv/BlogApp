using System.ComponentModel.DataAnnotations;

namespace BlogApp.DTOs
{
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "Id пользователя обязательно")]
        public string Id { get; set; }

        [StringLength(50, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов", MinimumLength = 3)]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Необходимо указать хотя бы одну роль")]
        public List<string> Roles { get; set; } = new List<string>();
    }
}
