using API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Tests.API_Tests.Controllers;

/// <summary>
/// Unit tests for SerhanKitaplarController authorization.
/// Tests verify that the [Authorize] attribute is properly applied.
/// </summary>
public class SerhanKitaplarControllerAuthorizationTests
{
    #region Controller-Level Authorization Tests

    [Fact]
    public void SerhanKitaplarController_ShouldHaveAuthorizeAttribute()
    {
        // Arrange & Act
        var controllerType = typeof(SerhanKitaplarController);
        var authorizeAttribute = controllerType.GetCustomAttributes(
            typeof(AuthorizeAttribute), true)
            .FirstOrDefault() as AuthorizeAttribute;

        // Assert
        authorizeAttribute.Should().NotBeNull(
            "Controller should have [Authorize] attribute to require authentication");
        authorizeAttribute!.Policy.Should().BeNull(
            "Authorization should not require a specific policy, just authentication");
    }

    [Fact]
    public void SerhanKitaplarController_ShouldNotHaveAllowAnonymousAttribute()
    {
        // Arrange & Act
        var controllerType = typeof(SerhanKitaplarController);
        var allowAnonymousAttribute = controllerType.GetCustomAttributes(
            typeof(AllowAnonymousAttribute), true)
            .FirstOrDefault();

        // Assert
        allowAnonymousAttribute.Should().BeNull(
            "Controller should not have [AllowAnonymous] attribute as it requires authentication");
    }

    #endregion

    #region Action Method Authorization Tests

    [Fact]
    public void GetSerhanKitaplar_Action_ShouldInheritControllerAuthorization()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.GetSerhanKitaplar));
        var allowAnonymousAttribute = actionMethod?.GetCustomAttributes(
            typeof(AllowAnonymousAttribute), true)
            .FirstOrDefault();

        // Assert
        allowAnonymousAttribute.Should().BeNull(
            "GET action should inherit [Authorize] from controller and not allow anonymous access");
    }

    [Fact]
    public void GetSerhanKitaplarDetail_Action_ShouldInheritControllerAuthorization()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.GetSerhanKitapDetail));
        var allowAnonymousAttribute = actionMethod?.GetCustomAttributes(
            typeof(AllowAnonymousAttribute), true)
            .FirstOrDefault();

        // Assert
        allowAnonymousAttribute.Should().BeNull(
            "GET {id} action should inherit [Authorize] from controller");
    }

    [Fact]
    public void CreateSerhanKitap_Action_ShouldInheritControllerAuthorization()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.CreateSerhanKitap));
        var allowAnonymousAttribute = actionMethod?.GetCustomAttributes(
            typeof(AllowAnonymousAttribute), true)
            .FirstOrDefault();

        // Assert
        allowAnonymousAttribute.Should().BeNull(
            "POST action should inherit [Authorize] from controller");
    }

    [Fact]
    public void EditSerhanKitap_Action_ShouldInheritControllerAuthorization()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.EditSerhanKitap));
        var allowAnonymousAttribute = actionMethod?.GetCustomAttributes(
            typeof(AllowAnonymousAttribute), true)
            .FirstOrDefault();

        // Assert
        allowAnonymousAttribute.Should().BeNull(
            "PUT action should inherit [Authorize] from controller");
    }

    [Fact]
    public void DeleteSerhanKitap_Action_ShouldInheritControllerAuthorization()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.DeleteSerhanKitap));
        var allowAnonymousAttribute = actionMethod?.GetCustomAttributes(
            typeof(AllowAnonymousAttribute), true)
            .FirstOrDefault();

        // Assert
        allowAnonymousAttribute.Should().BeNull(
            "DELETE action should inherit [Authorize] from controller");
    }

    #endregion

    #region HttpVerb Attributes Tests

    [Fact]
    public void GetSerhanKitaplar_ShouldHaveHttpGetAttribute()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.GetSerhanKitaplar));
        var httpGetAttribute = actionMethod?.GetCustomAttributes(
            typeof(HttpGetAttribute), true)
            .FirstOrDefault() as HttpGetAttribute;

        // Assert
        httpGetAttribute.Should().NotBeNull(
            "Action should have [HttpGet] attribute");
        httpGetAttribute!.Template.Should().BeNull(
            "GET action should respond to root path");
    }

    [Fact]
    public void GetSerhanKitapDetail_ShouldHaveHttpGetAttribute_WithIdParameter()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.GetSerhanKitapDetail));
        var httpGetAttribute = actionMethod?.GetCustomAttributes(
            typeof(HttpGetAttribute), true)
            .FirstOrDefault() as HttpGetAttribute;

        // Assert
        httpGetAttribute.Should().NotBeNull(
            "Action should have [HttpGet] attribute");
        httpGetAttribute!.Template.Should().Be("{id}",
            "GET action should have '{id}' route template");
    }

    [Fact]
    public void CreateSerhanKitap_ShouldHaveHttpPostAttribute()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.CreateSerhanKitap));
        var httpPostAttribute = actionMethod?.GetCustomAttributes(
            typeof(HttpPostAttribute), true)
            .FirstOrDefault() as HttpPostAttribute;

        // Assert
        httpPostAttribute.Should().NotBeNull(
            "Action should have [HttpPost] attribute");
        httpPostAttribute!.Template.Should().BeNull(
            "POST action should respond to root path");
    }

    [Fact]
    public void EditSerhanKitap_ShouldHaveHttpPutAttribute_WithIdParameter()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.EditSerhanKitap));
        var httpPutAttribute = actionMethod?.GetCustomAttributes(
            typeof(HttpPutAttribute), true)
            .FirstOrDefault() as HttpPutAttribute;

        // Assert
        httpPutAttribute.Should().NotBeNull(
            "Action should have [HttpPut] attribute");
        httpPutAttribute!.Template.Should().Be("{id}",
            "PUT action should have '{id}' route template");
    }

    [Fact]
    public void DeleteSerhanKitap_ShouldHaveHttpDeleteAttribute_WithIdParameter()
    {
        // Arrange & Act
        var actionMethod = typeof(SerhanKitaplarController)
            .GetMethod(nameof(SerhanKitaplarController.DeleteSerhanKitap));
        var httpDeleteAttribute = actionMethod?.GetCustomAttributes(
            typeof(HttpDeleteAttribute), true)
            .FirstOrDefault() as HttpDeleteAttribute;

        // Assert
        httpDeleteAttribute.Should().NotBeNull(
            "Action should have [HttpDelete] attribute");
        httpDeleteAttribute!.Template.Should().Be("{id}",
            "DELETE action should have '{id}' route template");
    }

    #endregion

    #region Route Attribute Tests

    [Fact]
    public void SerhanKitaplarController_ShouldHaveCorrectRouteAttribute()
    {
        // Arrange & Act
        var controllerType = typeof(SerhanKitaplarController);
        var routeAttribute = controllerType.GetCustomAttributes(
            typeof(RouteAttribute), true)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull(
            "Controller should have [Route] attribute");
        routeAttribute!.Template.Should().Be("api/v1/serhan-kitaplar",
            "Controller should have correct API versioned route");
    }

    #endregion
}
