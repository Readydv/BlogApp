using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CommentEditDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Комментарий не может быть пустым")]
        [StringLength(100, ErrorMessage = "Комментарий слишком длинный")]
        public string Content { get; set; }

        public Guid PostId { get; set; }
    }
}
