namespace API.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid PostId { get; set; }
        public string PostTitle { get; set; }
    }
}
