using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Helpers;

namespace Tests.Persistence_Tests;

public class DbInitializerTests
{
    [Fact]
    public async Task SeedData_Should_Add_Data_When_Db_Is_Empty()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());

        // Act
        await DbInitializer.SeedData(context);

        // Assert
        var count = await context.SerhanKitaplar.CountAsync();
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SeedData_Should_Not_Add_Data_When_Db_Is_Not_Empty()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        
        // Seed manually first
        await DbInitializer.SeedData(context);
        var initialCount = await context.SerhanKitaplar.CountAsync();

        // Act
        await DbInitializer.SeedData(context);

        // Assert
        var finalCount = await context.SerhanKitaplar.CountAsync();
        finalCount.Should().Be(initialCount);
    }
}
