using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Введите название роли")]
        [StringLength(50, ErrorMessage = "Название роли должно быть от 2 до 50 символов", MinimumLength = 2)]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
