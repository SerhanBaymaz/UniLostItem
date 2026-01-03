using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Persistence_Tests;

public class DbInitializerTests
{
    private static Mock<UserManager<ApplicationUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions();
        idOptions.Lockout.AllowedForNewUsers = false;
        options.Setup(o => o.Value).Returns(idOptions);
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object, 
            options.Object, 
            new PasswordHasher<ApplicationUser>(), 
            userValidators, 
            passwordValidators, 
            lookupNormalizer.Object, 
            errors, 
            services.Object, 
            logger.Object);
    }

    private static Mock<RoleManager<IdentityRole>> GetMockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();
        var roleValidators = new List<IRoleValidator<IdentityRole>>();
        roleValidators.Add(new RoleValidator<IdentityRole>());
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var logger = new Mock<ILogger<RoleManager<IdentityRole>>>();

        return new Mock<RoleManager<IdentityRole>>(
            store.Object, 
            roleValidators, 
            lookupNormalizer.Object, 
            errors, 
            logger.Object);
    }

    [Fact]
    public async Task SeedData_Should_Add_Data_When_Db_Is_Empty()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();

        // Setup Mocks to succeed
        roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null!);
        userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Act
        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object);

        // Assert
        var count = await context.SerhanKitaplar.CountAsync();
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SeedData_Should_Not_Add_Data_When_Db_Is_Not_Empty()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        
        // Setup Mocks to succeed
        roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());

        // Seed manually first
        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object);
        var initialCount = await context.SerhanKitaplar.CountAsync();

        // Act
        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object);

        // Assert
        var finalCount = await context.SerhanKitaplar.CountAsync();
        finalCount.Should().Be(initialCount);
    }
}