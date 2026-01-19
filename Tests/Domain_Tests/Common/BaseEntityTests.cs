using Domain.Common;
using FluentAssertions;
using Xunit;

namespace Tests.Domain_Tests.Common;

public class TestEntity : BaseEntity
{
    // Concrete implementation for testing abstract class
}

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(entity.Id, out _).Should().BeTrue("Id should be a valid GUID");
        entity.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.CreatedBy.Should().BeNull();
        entity.UpdatedDate.Should().BeNull();
        entity.UpdatedBy.Should().BeNull();
        entity.IsDeleted.Should().BeFalse();
        entity.IsActive.Should().BeTrue();
    }
}
