using Microsoft.AspNetCore.Identity;

namespace BlogApp.Models
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
