using Microsoft.AspNetCore.Identity;

namespace BlogApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
