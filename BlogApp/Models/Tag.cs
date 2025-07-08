namespace BlogApp.Models
{
    public class Tag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        
        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}
