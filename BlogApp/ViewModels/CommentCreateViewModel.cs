using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class CommentCreateViewModel
    {
        [Required]
        [StringLength(200, ErrorMessage = "Название тега должно быть до 200 символов")]
        public string Content { get; set; }
        [Required]
        public Guid PostId { get; set; }
    }
}
