using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Введите название роли")]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
