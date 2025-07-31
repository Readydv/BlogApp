namespace BlogApp.DTOs
{
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AuthorName { get; set; }
        public Guid PostId { get; set; }
    }
}
