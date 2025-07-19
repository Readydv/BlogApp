using BlogApp.Models;

namespace BlogApp.ViewModels
{
    public class UserListViewModel
    {
        public IEnumerable<UserItemViewModel> Users { get; set; }
        public List<Role> AvailableRoles { get; set; }
    }

    public class UserItemViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public Dictionary<string, string> RoleDescriptions { get; set; } // Описания ролей
    }
}
