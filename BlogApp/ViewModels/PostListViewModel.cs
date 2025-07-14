namespace BlogApp.ViewModels
{
    public class PostListViewModel
    {
        public IEnumerable<PostItemViewModel> Posts { get; set; }

        public class PostItemViewModel
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int ViewCount { get; set; }
            public IEnumerable<string> Tags { get; set; }
        }
    }
}
