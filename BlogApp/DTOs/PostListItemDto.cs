namespace BlogApp.DTOs
{
    public class PostListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ViewCount { get; set; }
        public List<string> TagNames { get; set; } = new();
    }
}
