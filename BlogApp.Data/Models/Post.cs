namespace BlogApp.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ViewCount { get; set; } = 0;

        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}
