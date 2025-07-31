using System.Text.Json.Serialization;

namespace BlogApp.Data.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }

        [JsonIgnore]
        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }

        [JsonIgnore]
        public Guid PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}
