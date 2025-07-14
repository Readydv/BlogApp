using BlogApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(100, ErrorMessage = "Заголовок не должен превышать 100 символов")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Содержание статьи обязательно")]
        public string Content { get; set; }
        public List<Guid> SelectedTagId { get; set; } = new List<Guid>();
        public IEnumerable<Tag>? AvailableTags { get; set; }
       
    }
}
