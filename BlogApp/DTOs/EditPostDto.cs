using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class EditPostDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Заголовок обязателен")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Содержание обязательно")]
        public string Content { get; set; }

        public List<Guid> SelectedTagId { get; set; } = new List<Guid>();
    }
}
