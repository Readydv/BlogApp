using BlogApp.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class EditPostViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Заголовок обязателен")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Содержание обязательно")]
        public string Content { get; set; }

        public List<Guid> SelectedTagId { get; set; } = new List<Guid>();
        public IEnumerable<Tag>? AvailableTags { get; set; } = new List<Tag>();
    }
}
