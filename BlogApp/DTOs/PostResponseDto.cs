using API.DTOs;

namespace BlogApp.DTOs
{
    public class PostResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ViewCount { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<CommentDto> Comments { get; set; } = new();
    }
}
