using Application.Features.Auth.Queries.GetCurrentUser;

namespace Application.Features.Auth.Commands.UpdateUserProfile;

public class UpdateUserProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
