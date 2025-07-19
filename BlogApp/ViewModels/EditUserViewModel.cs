using BlogApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        public string Email { get; set; }

        public List<Role> AvailableRoles { get; set; } = new List<Role>();

        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}
