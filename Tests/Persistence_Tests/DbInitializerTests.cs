using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    private static ApplicationUser CreateTestUser(string email, string id)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = "test",
            Email = email,
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User"
        };
    }

    [Fact]
    public async Task SeedData_Should_Add_Data_When_Db_Is_Empty()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        configMock.Setup(c => c["SeedData:Images:iPhone"]).Returns("iphone-url");
        configMock.Setup(c => c["SeedData:Images:GalaxyBuds"]).Returns("buds-url");
        configMock.Setup(c => c["SeedData:Images:StudentId"]).Returns("id-url");
        configMock.Setup(c => c["SeedData:Images:NikeBag"]).Returns("bag-url");

        // Add the test-user to the DB directly so FK constraint is satisfied
        var testUser = CreateTestUser("test-user@unilost.com", "test-user-id");
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null!);
        userManagerMock.Setup(u => u.FindByEmailAsync("test-user@unilost.com")).ReturnsAsync(testUser);
        userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var lostItemCount = await context.LostItems.CountAsync();
        lostItemCount.Should().Be(4);
    }

    [Fact]
    public async Task SeedData_Should_Not_Add_Data_When_Db_Is_Not_Empty()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var existingUser = CreateTestUser("test-user@unilost.com", "test-user-id");
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(existingUser);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);
        var initialLostItemCount = await context.LostItems.CountAsync();

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var finalLostItemCount = await context.LostItems.CountAsync();
        finalLostItemCount.Should().Be(initialLostItemCount);
    }
}
