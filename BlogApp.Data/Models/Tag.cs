namespace BlogApp.Data.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}
