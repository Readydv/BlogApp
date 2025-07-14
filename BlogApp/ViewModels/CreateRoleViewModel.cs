using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Введите название роли")]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
