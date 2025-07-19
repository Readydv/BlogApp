namespace BlogApp.ViewModels
{
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }  
        public DateTime CreatedDate { get; set; }
        public Guid PostId { get; set; }
        public string PostTitle { get; set; } // Добавляем заголовок статьи для удобства отображения
        public bool CanDelete { get; set; } // Свойство для проверки возможности удаления комментария
        public string AuthorId { get; set; } // Добавляем для проверки авторства
    }
}
