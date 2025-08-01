using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class EditPostDto
    {
        [Required(ErrorMessage = "ID поста обязателен")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(100, ErrorMessage = "Заголовок не должен превышать 100 символов")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Содержание обязательно")]
        public string Content { get; set; }

        public List<Guid> SelectedTagId { get; set; } = new List<Guid>();
    }
}
