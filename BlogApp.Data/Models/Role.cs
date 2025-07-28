using Microsoft.AspNetCore.Identity;

namespace BlogApp.Data.Models
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
