using Application.Core;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using AutoMapper;
using Domain;
using FluentAssertions;

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
            KitapSayfaSayisi = 123
        };

        // Act
        var result = _mapper.Map<GetSerhanKitapDto>(entity);

        // Assert
        result.Id.Should().Be(entity.Id);
        result.KitapName.Should().Be(entity.KitapName);
        result.KitapYazar.Should().Be(entity.KitapYazar);
        result.KitapSayfaSayisi.Should().Be(entity.KitapSayfaSayisi);
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
}
