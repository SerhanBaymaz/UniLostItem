using System;
using Application.Core;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using AutoMapper;
using Domain;
using FluentAssertions;
using Xunit;

namespace Tests.Application_Tests.Core;

public class MappingProfilesTests
{
    private readonly IMapper _mapper;

    public MappingProfilesTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_ShouldBeValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Should_Map_SerhanKitap_To_GetSerhanKitapDto()
    {
        // Arrange
        var entity = new SerhanKitap
        {
            Id = Guid.NewGuid().ToString(),
            KitapName = "Test Kitap",
            KitapYazar = "Test Yazar",
            KitapSayfaSayisi = 123,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "user123",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "user456",
            IsDeleted = false,
            IsActive = true
        };

        // Act
        var result = _mapper.Map<GetSerhanKitapDto>(entity);

        // Assert
        result.Id.Should().Be(entity.Id);
        result.KitapName.Should().Be(entity.KitapName);
        result.KitapYazar.Should().Be(entity.KitapYazar);
        result.KitapSayfaSayisi.Should().Be(entity.KitapSayfaSayisi);
        // Audit fields should be mapped
        result.CreatedDate.Should().Be(entity.CreatedDate);
        result.CreatedBy.Should().Be(entity.CreatedBy);
        result.UpdatedDate.Should().Be(entity.UpdatedDate);
        result.UpdatedBy.Should().Be(entity.UpdatedBy);
        result.IsDeleted.Should().Be(entity.IsDeleted);
        result.IsActive.Should().Be(entity.IsActive);
    }

    [Fact]
    public void Should_Map_CreateSerhanKitapDto_To_SerhanKitap()
    {
        // Arrange
        var dto = new CreateSerhanKitapDto
        {
            KitapName = "New Kitap",
            KitapYazar = "New Yazar",
            KitapSayfaSayisi = 456
        };

        // Act
        var result = _mapper.Map<SerhanKitap>(dto);

        // Assert
        result.KitapName.Should().Be(dto.KitapName);
        result.KitapYazar.Should().Be(dto.KitapYazar);
        result.KitapSayfaSayisi.Should().Be(dto.KitapSayfaSayisi);
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_Map_EditSerhanKitapDto_To_SerhanKitap()
    {
        // Arrange
        var dto = new EditSerhanKitapDto
        {
            KitapName = "Updated Kitap",
            KitapYazar = "Updated Yazar",
            KitapSayfaSayisi = 789
        };

        // Act
        var result = _mapper.Map<SerhanKitap>(dto);

        // Assert
        result.KitapName.Should().Be(dto.KitapName);
        result.KitapYazar.Should().Be(dto.KitapYazar);
        result.KitapSayfaSayisi.Should().Be(dto.KitapSayfaSayisi);
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_Map_AuditFields_When_Null()
    {
        // Arrange
        var entity = new SerhanKitap
        {
            Id = Guid.NewGuid().ToString(),
            KitapName = "Test Kitap",
            KitapYazar = "Test Yazar",
            KitapSayfaSayisi = 123,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = null,  // Nullable field
            UpdatedDate = null,  // Nullable field
            UpdatedBy = null,    // Nullable field
            IsDeleted = false,
            IsActive = true
        };

        // Act
        var result = _mapper.Map<GetSerhanKitapDto>(entity);

        // Assert
        result.CreatedDate.Should().Be(entity.CreatedDate);
        result.CreatedBy.Should().BeNull();
        result.UpdatedDate.Should().BeNull();
        result.UpdatedBy.Should().BeNull();
        result.IsDeleted.Should().BeFalse();
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Should_Map_AuditFields_When_SoftDeleted()
    {
        // Arrange
        var entity = new SerhanKitap
        {
            Id = Guid.NewGuid().ToString(),
            KitapName = "Deleted Kitap",
            KitapYazar = "Test Yazar",
            KitapSayfaSayisi = 123,
            CreatedDate = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "creator",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "deleter",
            IsDeleted = true,
            IsActive = true
        };

        // Act
        var result = _mapper.Map<GetSerhanKitapDto>(entity);

        // Assert
        result.IsDeleted.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        result.CreatedBy.Should().Be("creator");
        result.UpdatedBy.Should().Be("deleter");
    }

    [Fact]
    public void Should_Map_AuditFields_When_Inactive()
    {
        // Arrange
        var entity = new SerhanKitap
        {
            Id = Guid.NewGuid().ToString(),
            KitapName = "Inactive Kitap",
            KitapYazar = "Test Yazar",
            KitapSayfaSayisi = 123,
            CreatedDate = DateTime.UtcNow.AddDays(-5),
            CreatedBy = "creator",
            UpdatedDate = null,
            UpdatedBy = null,
            IsDeleted = false,
            IsActive = false
        };

        // Act
        var result = _mapper.Map<GetSerhanKitapDto>(entity);

        // Assert
        result.IsDeleted.Should().BeFalse();
        result.IsActive.Should().BeFalse();
        result.CreatedBy.Should().Be("creator");
        result.UpdatedDate.Should().BeNull();
        result.UpdatedBy.Should().BeNull();
    }
}
