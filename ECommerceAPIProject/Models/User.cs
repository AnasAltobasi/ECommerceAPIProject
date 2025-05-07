using Microsoft.AspNetCore.Identity;

namespace ECommerceAPIProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
