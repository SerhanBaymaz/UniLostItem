using Domain;
using FluentAssertions;
using Xunit;

namespace Tests.Domain_Tests;

public class SerhanKitapTests
{
    [Fact]
    public void SerhanKitap_Should_Inherit_All_BaseEntity_Properties()
    {
        // Arrange & Act
        var kitap = new SerhanKitap
        {
            KitapName = "Test",
            KitapYazar = "Test",
            KitapSayfaSayisi = 1
        };

        // Assert - Verify all BaseEntity properties are available
        kitap.Should().NotBeNull();
        kitap.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(kitap.Id, out _).Should().BeTrue("Id should be a valid GUID");
        kitap.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        kitap.CreatedBy.Should().BeNull();
        kitap.UpdatedDate.Should().BeNull();
        kitap.UpdatedBy.Should().BeNull();
        kitap.IsDeleted.Should().BeFalse();
        kitap.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SerhanKitap_Should_Have_Its_Own_Properties()
    {
        // Arrange & Act
        var kitap = new SerhanKitap
        {
            KitapName = "Test Kitap",
            KitapYazar = "Test Yazar",
            KitapSayfaSayisi = 100
        };

        // Assert - Verify SerhanKitap-specific properties
        kitap.KitapName.Should().Be("Test Kitap");
        kitap.KitapYazar.Should().Be("Test Yazar");
        kitap.KitapSayfaSayisi.Should().Be(100);
    }

    [Fact]
    public void SerhanKitap_Should_Allow_Setting_Audit_Properties()
    {
        // Arrange
        var userId = "test-user-id";
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow.AddHours(1);

        // Act
        var kitap = new SerhanKitap
        {
            KitapName = "Test Kitap",
            KitapYazar = "Test Yazar",
            KitapSayfaSayisi = 100,
            CreatedDate = createdDate,
            CreatedBy = userId,
            UpdatedDate = updatedDate,
            UpdatedBy = userId,
            IsDeleted = true,
            IsActive = false
        };

        // Assert
        kitap.CreatedDate.Should().Be(createdDate);
        kitap.CreatedBy.Should().Be(userId);
        kitap.UpdatedDate.Should().Be(updatedDate);
        kitap.UpdatedBy.Should().Be(userId);
        kitap.IsDeleted.Should().BeTrue();
        kitap.IsActive.Should().BeFalse();
    }
}
