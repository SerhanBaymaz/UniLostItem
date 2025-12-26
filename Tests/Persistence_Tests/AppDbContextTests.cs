using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Helpers;

namespace Tests.Persistence_Tests;

public class AppDbContextTests
{
    private readonly AppDbContext _context;

    public AppDbContextTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
    }

    [Fact]
    public async Task Can_Add_And_Get_SerhanKitap()
    {
        // Arrange
        var kitap = new SerhanKitap
        {
            KitapName = "Test Kitap",
            KitapYazar = "Test Yazar",
            KitapSayfaSayisi = 123
        };

        // Act
        _context.SerhanKitaplar.Add(kitap);
        await _context.SaveChangesAsync();

        // Assert
        var savedKitap = await _context.SerhanKitaplar.FirstOrDefaultAsync();
        savedKitap.Should().NotBeNull();
        savedKitap!.KitapName.Should().Be("Test Kitap");
        savedKitap.KitapYazar.Should().Be("Test Yazar");
        savedKitap.KitapSayfaSayisi.Should().Be(123);
        savedKitap.Id.Should().NotBeEmpty();
    }
}
