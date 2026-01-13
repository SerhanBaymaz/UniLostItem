using System.Threading;
using System.Threading.Tasks;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;
using Xunit;

namespace Tests.Application_Tests.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;

public class GetSerhanKitapPaginatedListQueryHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetSerhanKitapPaginatedListQueryHandler _handler;

    public GetSerhanKitapPaginatedListQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetSerhanKitapPaginatedListQueryHandler(_context, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "2", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "3", KitapName = "Book C", KitapYazar = "Author 1", KitapSayfaSayisi = 150 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 100 },
            new GetSerhanKitapDto { Id = "2", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 200 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            PageNumber = 1,
            PageSize = 2
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasPrevious.Should().BeFalse();
        result.Value.HasNext.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyDatabase_ShouldReturnEmptyPaginatedList()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(new List<GetSerhanKitapDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
        result.Value.HasPrevious.Should().BeFalse();
        result.Value.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithSortByKitapNameAscending_ShouldReturnSortedResults()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Zebra", KitapYazar = "Author 1", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "2", KitapName = "Alpha", KitapYazar = "Author 2", KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "3", KitapName = "Beta", KitapYazar = "Author 1", KitapSayfaSayisi = 150 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Alpha", KitapYazar = "Author 2", KitapSayfaSayisi = 200 },
            new GetSerhanKitapDto { Id = "3", KitapName = "Beta", KitapYazar = "Author 1", KitapSayfaSayisi = 150 },
            new GetSerhanKitapDto { Id = "1", KitapName = "Zebra", KitapYazar = "Author 1", KitapSayfaSayisi = 100 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            SortBy = "kitapname",
            SortDescending = false
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapName.Should().Be("Alpha");
        result.Value.Items[1].KitapName.Should().Be("Beta");
        result.Value.Items[2].KitapName.Should().Be("Zebra");
    }

    [Fact]
    public async Task Handle_WithSortByKitapNameDescending_ShouldReturnSortedResults()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Zebra", KitapYazar = "Author 1", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "2", KitapName = "Alpha", KitapYazar = "Author 2", KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "3", KitapName = "Beta", KitapYazar = "Author 1", KitapSayfaSayisi = 150 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "Zebra", KitapYazar = "Author 1", KitapSayfaSayisi = 100 },
            new GetSerhanKitapDto { Id = "3", KitapName = "Beta", KitapYazar = "Author 1", KitapSayfaSayisi = 150 },
            new GetSerhanKitapDto { Id = "2", KitapName = "Alpha", KitapYazar = "Author 2", KitapSayfaSayisi = 200 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            SortBy = "kitapname",
            SortDescending = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapName.Should().Be("Zebra");
        result.Value.Items[1].KitapName.Should().Be("Beta");
        result.Value.Items[2].KitapName.Should().Be("Alpha");
    }

    [Fact]
    public async Task Handle_WithSortByKitapYazar_ShouldSortByAuthor()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Book A", KitapYazar = "Charlie", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "2", KitapName = "Book B", KitapYazar = "Alice", KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "3", KitapName = "Book C", KitapYazar = "Bob", KitapSayfaSayisi = 150 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Book B", KitapYazar = "Alice", KitapSayfaSayisi = 200 },
            new GetSerhanKitapDto { Id = "3", KitapName = "Book C", KitapYazar = "Bob", KitapSayfaSayisi = 150 },
            new GetSerhanKitapDto { Id = "1", KitapName = "Book A", KitapYazar = "Charlie", KitapSayfaSayisi = 100 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            SortBy = "kitapyazar"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapYazar.Should().Be("Alice");
        result.Value.Items[1].KitapYazar.Should().Be("Bob");
        result.Value.Items[2].KitapYazar.Should().Be("Charlie");
    }

    [Fact]
    public async Task Handle_WithSortByKitapSayfaSayisi_ShouldSortByPageCount()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 300 },
            new SerhanKitap { Id = "2", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "3", KitapName = "Book C", KitapYazar = "Author 1", KitapSayfaSayisi = 200 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 300 },
            new GetSerhanKitapDto { Id = "3", KitapName = "Book C", KitapYazar = "Author 1", KitapSayfaSayisi = 200 },
            new GetSerhanKitapDto { Id = "2", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 100 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            SortBy = "kitapsayfasayisi",
            SortDescending = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapSayfaSayisi.Should().Be(300);
        result.Value.Items[1].KitapSayfaSayisi.Should().Be(200);
        result.Value.Items[2].KitapSayfaSayisi.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithAlternativeSortFieldNames_ShouldSortCorrectly()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Book A", KitapYazar = "Charlie", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "2", KitapName = "Book B", KitapYazar = "Alice", KitapSayfaSayisi = 200 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Book B", KitapYazar = "Alice", KitapSayfaSayisi = 200 },
            new GetSerhanKitapDto { Id = "1", KitapName = "Book A", KitapYazar = "Charlie", KitapSayfaSayisi = 100 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            SortBy = "yazar"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapYazar.Should().Be("Alice");
        result.Value.Items[1].KitapYazar.Should().Be("Charlie");
    }

    [Fact]
    public async Task Handle_WithInvalidSortField_ShouldUseDefaultSort()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "2", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 200 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 200 },
            new GetSerhanKitapDto { Id = "1", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 100 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            SortBy = "invalidfield"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should default to sorting by KitapName
        result.Value!.Items[0].KitapName.Should().Be("Book A");
        result.Value.Items[1].KitapName.Should().Be("Book B");
    }

    [Fact]
    public async Task Handle_WithSecondPage_ShouldReturnCorrectPage()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "2", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "3", KitapName = "Book C", KitapYazar = "Author 1", KitapSayfaSayisi = 150 },
            new SerhanKitap { Id = "4", KitapName = "Book D", KitapYazar = "Author 3", KitapSayfaSayisi = 250 },
            new SerhanKitap { Id = "5", KitapName = "Book E", KitapYazar = "Author 2", KitapSayfaSayisi = 180 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "4", KitapName = "Book D", KitapYazar = "Author 3", KitapSayfaSayisi = 250 },
            new GetSerhanKitapDto { Id = "5", KitapName = "Book E", KitapYazar = "Author 2", KitapSayfaSayisi = 180 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            PageNumber = 2,
            PageSize = 3
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(5);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(3);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasPrevious.Should().BeTrue();
        result.Value.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithCaseInsensitiveSortField_ShouldSortCorrectly()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 100 },
            new SerhanKitap { Id = "2", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 200 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Book A", KitapYazar = "Author 1", KitapSayfaSayisi = 200 },
            new GetSerhanKitapDto { Id = "1", KitapName = "Book B", KitapYazar = "Author 2", KitapSayfaSayisi = 100 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapPaginatedListQuery
        {
            SortBy = "KITAPNAME" // Uppercase
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapName.Should().Be("Book A");
    }
}
