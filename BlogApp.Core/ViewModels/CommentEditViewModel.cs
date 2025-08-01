using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class CommentEditViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Комментарий не может быть пустым")]
        [StringLength(100, ErrorMessage = "Комментарий слишком длинный")]
        public string Content { get; set; }
        public Guid PostId { get; set; }
    }
}
