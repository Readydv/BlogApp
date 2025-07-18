using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class CommentCreateViewModel
    {
        [Required]
        public string Content { get; set; }
        [Required]
        public Guid PostId { get; set; }
    }
}
