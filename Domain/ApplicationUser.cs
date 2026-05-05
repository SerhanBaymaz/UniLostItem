using Microsoft.AspNetCore.Identity;

namespace Domain;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    public string? ProfileImageUrl { get; set; }

    //Navigation Properities
    public ICollection<LostItem> LostItems { get; set; } = [];
    public ICollection<ItemClaim> Claims { get; set; } = [];
}
