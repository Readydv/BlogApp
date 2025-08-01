using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class TagViewModel
    {
        public Guid Id { get; set; }

        [Required (ErrorMessage = "Название тега обязательно")]
        [StringLength (50, ErrorMessage = "Название тега должно быть от 2 до 50 символов", MinimumLength = 2)]
        public string Name { get; set; }
        public int PostCount { get; set; } // Добавляем свойство для количества статей
    }
}
