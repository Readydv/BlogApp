using BlogApp.Models;

namespace BlogApp.ViewModels
{
    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Post> Posts { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Comment> Comments { get; set; }
        public int TotalPosts => Posts?.Count ?? 0;
        public int TotalTags => Tags?.Count ?? 0;
        public int TotalComments => Comments?.Count ?? 0;
    }
}
