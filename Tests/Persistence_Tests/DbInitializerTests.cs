using Domain;
using Domain.Common.Enums;
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

    private static void SetupCommonMocks(
        Mock<UserManager<ApplicationUser>> userManagerMock,
        Mock<RoleManager<IdentityRole>> roleManagerMock,
        Mock<IConfiguration> configMock,
        bool rolesExist = true,
        bool adminExists = true,
        bool testUserExists = true)
    {
        roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(rolesExist);
        userManagerMock.Setup(u => u.FindByEmailAsync("admin@admin.com")).ReturnsAsync(
            adminExists ? CreateTestUser("admin@admin.com", "admin-id") : null);
        userManagerMock.Setup(u => u.FindByEmailAsync("ahmetkuyuldar@gmail.com")).ReturnsAsync(
            testUserExists ? CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id") : null);
        userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);
        configMock.Setup(c => c["SeedData:Images:iPhone"]).Returns("iphone-url");
        configMock.Setup(c => c["SeedData:Images:GalaxyBuds"]).Returns("buds-url");
        configMock.Setup(c => c["SeedData:Images:StudentId"]).Returns("id-url");
        configMock.Setup(c => c["SeedData:Images:NikeBag"]).Returns("bag-url");
    }

    [Fact]
    public async Task SeedData_Should_Add_Data_When_Db_Is_Empty()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock);

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

        var existingUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);
        var initialLostItemCount = await context.LostItems.CountAsync();

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var finalLostItemCount = await context.LostItems.CountAsync();
        finalLostItemCount.Should().Be(initialLostItemCount);
    }

    [Fact]
    public async Task SeedData_Should_Create_Roles_When_Not_Exist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });

        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, rolesExist: false);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        roleManagerMock.Verify(r => r.CreateAsync(It.Is<IdentityRole>(role => role.Name == "Admin")), Times.Once);
        roleManagerMock.Verify(r => r.CreateAsync(It.Is<IdentityRole>(role => role.Name == "BaseUser")), Times.Once);
    }

    [Fact]
    public async Task SeedData_Should_Not_Create_Roles_When_Exist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });

        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, rolesExist: true);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }

    [Fact]
    public async Task SeedData_Should_Create_Admin_User_When_Not_Exist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });

        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, adminExists: false);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        userManagerMock.Verify(u => u.CreateAsync(It.Is<ApplicationUser>(user => user.Email == "admin@admin.com"), It.IsAny<string>()), Times.Once);
        userManagerMock.Verify(u => u.AddToRoleAsync(It.Is<ApplicationUser>(user => user.Email == "admin@admin.com"), "Admin"), Times.Once);
    }

    [Fact]
    public async Task SeedData_Should_Not_Create_Admin_User_When_Exists()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var existingAdmin = CreateTestUser("admin@admin.com", "admin-id");
        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(existingAdmin);
        context.Users.Add(testUser);

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });

        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, adminExists: true);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SeedData_Should_Create_Test_User_When_Not_Exist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var adminUser = CreateTestUser("admin@admin.com", "admin-id");
        context.Users.Add(adminUser);

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });

        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, testUserExists: false);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        userManagerMock.Verify(u => u.CreateAsync(It.Is<ApplicationUser>(user => user.Email == "ahmetkuyuldar@gmail.com"), It.IsAny<string>()), Times.Once);
        userManagerMock.Verify(u => u.AddToRoleAsync(It.Is<ApplicationUser>(user => user.Email == "ahmetkuyuldar@gmail.com"), "BaseUser"), Times.Once);
    }

    [Fact]
    public async Task SeedData_Should_Not_Create_Test_User_When_Exists()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var adminUser = CreateTestUser("admin@admin.com", "admin-id");
        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(adminUser);
        context.Users.Add(testUser);

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });

        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, testUserExists: true);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SeedData_Should_Create_Admin_User_With_Correct_Properties()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });
        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, adminExists: false);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        userManagerMock.Verify(u => u.CreateAsync(It.Is<ApplicationUser>(user =>
            user.Email == "admin@admin.com" &&
            user.UserName == "admin@admin.com" &&
            user.FirstName == "System" &&
            user.LastName == "Admin" &&
            user.EmailConfirmed
        ), "Admin.1234"), Times.Once);
    }

    [Fact]
    public async Task SeedData_Should_Not_Add_To_Role_If_Admin_User_Creation_Fails()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });
        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, adminExists: false);
        userManagerMock.Setup(u => u.CreateAsync(It.Is<ApplicationUser>(u => u.Email == "admin@admin.com"), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        userManagerMock.Verify(u => u.AddToRoleAsync(It.Is<ApplicationUser>(u => u.Email == "admin@admin.com"), "Admin"), Times.Never);
    }

    [Fact]
    public async Task SeedData_Should_Create_Test_User_With_Correct_Properties()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        // Add user for the dummy lost item to satisfy FK
        context.Users.Add(new ApplicationUser { Id = "dummy-id", UserName = "dummy", Email = "dummy@test.com" });
        context.LostItems.Add(new LostItem
        {
            Title = "Dummy",
            Description = "Dummy",
            ContactInfo = "dummy@test.com",
            LocationLabel = "Dummy",
            UserId = "dummy-id"
        });
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock, testUserExists: false);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        userManagerMock.Verify(u => u.CreateAsync(It.Is<ApplicationUser>(user =>
            user.Email == "ahmetkuyuldar@gmail.com" &&
            user.UserName == "ahmetkuyuldar@gmail.com" &&
            user.FirstName == "Ahmet" &&
            user.LastName == "Kuyuldar" &&
            user.EmailConfirmed
        ), "Ahmet.1234"), Times.Once);
    }

    [Fact]
    public async Task SeedData_Should_Perform_Full_Seed_When_Empty()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        // Setup mock to return null first, then the user (simulating creation)
        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");

        // Also add the user to context to satisfy FK when items are saved
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        userManagerMock.SetupSequence(u => u.FindByEmailAsync("ahmetkuyuldar@gmail.com"))
            .ReturnsAsync((ApplicationUser)null!)
            .ReturnsAsync(testUser);

        userManagerMock.Setup(u => u.FindByEmailAsync("admin@admin.com"))
            .ReturnsAsync((ApplicationUser)null!);

        userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

        configMock.Setup(c => c["SeedData:Images:iPhone"]).Returns("iphone-url");
        configMock.Setup(c => c["SeedData:Images:GalaxyBuds"]).Returns("buds-url");
        configMock.Setup(c => c["SeedData:Images:StudentId"]).Returns("id-url");
        configMock.Setup(c => c["SeedData:Images:NikeBag"]).Returns("bag-url");

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Exactly(2));
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Exactly(2));
        var lostItemCount = await context.LostItems.CountAsync();
        lostItemCount.Should().Be(4);
    }

    [Fact]
    public async Task SeedData_Should_Create_LostItems_With_Expected_Titles()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var titles = await context.LostItems.Select(x => x.Title).ToListAsync();
        titles.Should().BeEquivalentTo(
            "iPhone 15 Pro Max",
            "Samsung Galaxy Buds",
            "Öğrenci Kimliği",
            "Siyah Sırt Çantası");
    }

    [Fact]
    public async Task SeedData_Should_Create_LostItems_With_Expected_Categories()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var items = await context.LostItems.OrderBy(x => x.Title).ToListAsync();
        items.Should().Contain(x => x.Title == "iPhone 15 Pro Max" && x.Category == ItemCategory.Electronics);
        items.Should().Contain(x => x.Title == "Samsung Galaxy Buds" && x.Category == ItemCategory.Electronics);
        items.Should().Contain(x => x.Title == "Öğrenci Kimliği" && x.Category == ItemCategory.IdentificationCard);
        items.Should().Contain(x => x.Title == "Siyah Sırt Çantası" && x.Category == ItemCategory.BagWallet);
    }

    [Fact]
    public async Task SeedData_Should_Create_LostItems_With_Expected_ItemTypes()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var items = await context.LostItems.OrderBy(x => x.Title).ToListAsync();
        items.Should().Contain(x => x.Title == "iPhone 15 Pro Max" && x.ItemType == ItemType.Lost);
        items.Should().Contain(x => x.Title == "Samsung Galaxy Buds" && x.ItemType == ItemType.Found);
        items.Should().Contain(x => x.Title == "Öğrenci Kimliği" && x.ItemType == ItemType.Lost);
        items.Should().Contain(x => x.Title == "Siyah Sırt Çantası" && x.ItemType == ItemType.Found);
    }

    [Fact]
    public async Task SeedData_Should_Assign_All_LostItems_To_TestUser()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var allItems = await context.LostItems.ToListAsync();
        allItems.Should().AllSatisfy(item => item.UserId.Should().Be("test-user-id"));
    }

    [Fact]
    public async Task SeedData_Should_Use_Configuration_Image_Urls()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var items = await context.LostItems.OrderBy(x => x.Title).ToListAsync();
        items.Should().Contain(x => x.Title == "iPhone 15 Pro Max" && x.ImageUrl == "iphone-url");
        items.Should().Contain(x => x.Title == "Samsung Galaxy Buds" && x.ImageUrl == "buds-url");
        items.Should().Contain(x => x.Title == "Öğrenci Kimliği" && x.ImageUrl == "id-url");
        items.Should().Contain(x => x.Title == "Siyah Sırt Çantası" && x.ImageUrl == "bag-url");
    }

    [Fact]
    public async Task SeedData_Should_Create_LostItems_With_Active_Status()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var userManagerMock = GetMockUserManager();
        var roleManagerMock = GetMockRoleManager();
        var configMock = new Mock<IConfiguration>();

        var testUser = CreateTestUser("ahmetkuyuldar@gmail.com", "test-user-id");
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        SetupCommonMocks(userManagerMock, roleManagerMock, configMock);

        await DbInitializer.SeedData(context, userManagerMock.Object, roleManagerMock.Object, configMock.Object);

        var items = await context.LostItems.ToListAsync();
        items.Should().AllSatisfy(item => item.Status.Should().Be(ItemStatus.Active));
    }
}
