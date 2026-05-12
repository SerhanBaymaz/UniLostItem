using Application.Interfaces;
using FluentAssertions;

namespace Tests.Application_Tests.Services;

public class ImageUploadResultTests
{
    [Fact]
    public void ImageUploadResult_ShouldHoldUrlAndPublicId()
    {
        var result = new ImageUploadResult
        {
            Url = "https://res.cloudinary.com/demo/image/upload/v1/test.jpg",
            PublicId = "unilostitem/test123"
        };

        result.Url.Should().Be("https://res.cloudinary.com/demo/image/upload/v1/test.jpg");
        result.PublicId.Should().Be("unilostitem/test123");
    }
}
