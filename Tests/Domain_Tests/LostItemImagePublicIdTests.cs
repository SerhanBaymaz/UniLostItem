using Domain;
using Domain.Common.Enums;
using FluentAssertions;

namespace Tests.Domain_Tests;

public class LostItemImagePublicIdTests
{
    [Fact]
    public void ImagePublicId_ShouldDefaultToNull()
    {
        var item = new LostItem
        {
            Title = "Test",
            Description = "Test",
            ContactInfo = "test@test.com",
            LocationLabel = "Test Location",
            UserId = "test-user"
        };

        item.ImagePublicId.Should().BeNull();
    }

    [Fact]
    public void ImagePublicId_ShouldAcceptValue()
    {
        var item = new LostItem
        {
            Title = "Test",
            Description = "Test",
            ContactInfo = "test@test.com",
            LocationLabel = "Test Location",
            UserId = "test-user",
            ImagePublicId = "unilostitem/abc123"
        };

        item.ImagePublicId.Should().Be("unilostitem/abc123");
    }

    [Fact]
    public void ImagePublicId_ShouldBeSettableToNull()
    {
        var item = new LostItem
        {
            Title = "Test",
            Description = "Test",
            ContactInfo = "test@test.com",
            LocationLabel = "Test Location",
            UserId = "test-user",
            ImagePublicId = "unilostitem/abc123"
        };

        item.ImagePublicId = null;

        item.ImagePublicId.Should().BeNull();
    }
}
