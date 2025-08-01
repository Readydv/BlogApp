using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CommentCreateDto
    {
        [Required]
        [StringLength(200, ErrorMessage = "Комментарий должен быть до 200 символов")]
        public string Content { get; set; }

        [Required]
        public Guid PostId { get; set; }
    }
}
