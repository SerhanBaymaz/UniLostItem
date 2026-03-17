using System;
using Application.Core;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Tests.Application_Tests.Core;

public class MappingProfilesTests
{
    private readonly IMapper _mapper;

    public MappingProfilesTests()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfiles>(),
            NullLoggerFactory.Instance);
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

    // ==================== AUTO MAPPER EXTENSIONS TESTS ====================

    [Fact]
    public void IgnoreBaseEntityAuditFields_ShouldConfigureValidationToIgnoreAuditFields()
    {
        // Arrange & Act
        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<SerhanKitap, GetSerhanKitapDto>()
                    .IgnoreBaseEntityAuditFields();
            },
            NullLoggerFactory.Instance);

        // Assert - Configuration should be valid even if destination has audit fields
        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void IgnoreAllBaseEntityProperties_ShouldIgnoreAllBaseEntityPropertiesWhenMappingToEntity()
    {
        // Arrange
        var dto = new CreateSerhanKitapDto
        {
            KitapName = "Test Book",
            KitapYazar = "Test Author",
            KitapSayfaSayisi = 100
        };

        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<CreateSerhanKitapDto, SerhanKitap>()
                    .IgnoreAllBaseEntityProperties();
            },
            NullLoggerFactory.Instance);

        var mapper = configuration.CreateMapper();

        // Act
        var result = mapper.Map<SerhanKitap>(dto);

        // Assert - BaseEntity properties should have their default values from BaseEntity class
        // Id should be auto-generated by BaseEntity default
        result.Id.Should().NotBeNullOrEmpty();
        // CreatedDate should be UtcNow from BaseEntity default
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        // CreatedBy should be null (not mapped from DTO)
        result.CreatedBy.Should().BeNull();
        // UpdatedDate should be null (not mapped from DTO)
        result.UpdatedDate.Should().BeNull();
        // UpdatedBy should be null (not mapped from DTO)
        result.UpdatedBy.Should().BeNull();
        // IsDeleted should be false (default from BaseEntity)
        result.IsDeleted.Should().BeFalse();
        // IsActive should be true (default from BaseEntity)
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IgnoreAllBaseEntityProperties_ShouldAllowMappingToEntityWithExistingValues()
    {
        // Arrange
        var existingId = Guid.NewGuid().ToString();
        var existingCreatedDate = DateTime.UtcNow.AddDays(-10);
        const string existingCreatedBy = "original-user";

        var dto = new EditSerhanKitapDto
        {
            KitapName = "Updated Book",
            KitapYazar = "Updated Author",
            KitapSayfaSayisi = 200
        };

        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<EditSerhanKitapDto, SerhanKitap>()
                    .IgnoreAllBaseEntityProperties();
            },
            NullLoggerFactory.Instance);

        var mapper = configuration.CreateMapper();

        var existingEntity = new SerhanKitap
        {
            Id = existingId,
            KitapName = "Old Book",
            KitapYazar = "Old Author",
            KitapSayfaSayisi = 50,
            CreatedDate = existingCreatedDate,
            CreatedBy = existingCreatedBy,
            UpdatedDate = DateTime.UtcNow.AddDays(-5),
            UpdatedBy = "previous-user",
            IsDeleted = false,
            IsActive = true
        };

        // Act - Map DTO onto existing entity
        mapper.Map(dto, existingEntity);

        // Assert - BaseEntity properties should be preserved
        existingEntity.Id.Should().Be(existingId);
        existingEntity.CreatedDate.Should().Be(existingCreatedDate);
        existingEntity.CreatedBy.Should().Be(existingCreatedBy);
        // UpdatedDate and UpdatedBy should also be preserved (not set to null)
        existingEntity.UpdatedDate.Should().NotBeNull();
        existingEntity.UpdatedBy.Should().Be("previous-user");
        existingEntity.IsDeleted.Should().BeFalse();
        existingEntity.IsActive.Should().BeTrue();
        // But business properties should be updated
        existingEntity.KitapName.Should().Be("Updated Book");
        existingEntity.KitapYazar.Should().Be("Updated Author");
        existingEntity.KitapSayfaSayisi.Should().Be(200);
    }

    [Fact]
    public void IgnoreAllBaseEntityProperties_ShouldBeReusableAcrossDifferentMappings()
    {
        // Arrange & Act - Create multiple mappings using the extension
        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<CreateSerhanKitapDto, SerhanKitap>()
                    .IgnoreAllBaseEntityProperties();

                cfg.CreateMap<EditSerhanKitapDto, SerhanKitap>()
                    .IgnoreAllBaseEntityProperties();
            },
            NullLoggerFactory.Instance);

        // Assert - Both mappings should be valid
        configuration.AssertConfigurationIsValid();

        var mapper = configuration.CreateMapper();

        // Test Create mapping
        var createDto = new CreateSerhanKitapDto
        {
            KitapName = "Create Test",
            KitapYazar = "Create Author",
            KitapSayfaSayisi = 111
        };
        var createResult = mapper.Map<SerhanKitap>(createDto);
        createResult.KitapName.Should().Be("Create Test");
        createResult.Id.Should().NotBeNullOrEmpty(); // BaseEntity default preserved

        // Test Edit mapping
        var editDto = new EditSerhanKitapDto
        {
            KitapName = "Edit Test",
            KitapYazar = "Edit Author",
            KitapSayfaSayisi = 222
        };
        var editResult = mapper.Map<SerhanKitap>(editDto);
        editResult.KitapName.Should().Be("Edit Test");
        editResult.Id.Should().NotBeNullOrEmpty(); // BaseEntity default preserved
    }
}
