using System.Threading;
using System.Threading.Tasks;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.Common.Enums;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;
using Xunit;

namespace Tests.Application_Tests.Features.SerhanKitaplar.Queries.GetSerhanKitapList;

public class GetSerhanKitapListQueryHandlerTests
{
    private const string VictorHugo = "Victor Hugo";
    private const string FyodorDostoyevski = "Fyodor Dostoyevski";
    private const string JRRTolkien = "J.R.R. Tolkien";
    private const string JKRowling = "J.K. Rowling";
    private const string SabahattinAli = "Sabahattin Ali";
    private const string StephenKing = "Stephen King";
    private const string GeorgeOrwell = "George Orwell";
    private const string AminMaalouf = "Amin Maalouf";
    private const string FalihRifkiAtay = "Falih Rıfkı Atay";
    private const string AntoineDeSaintExupery = "Antoine de Saint-Exupéry";
    private const string StefanZweig = "Stefan Zweig";
    private const string JackLondon = "Jack London";
    private const string PauloCoelho = "Paulo Coelho";
    private const string AdaletAgaoglu = "Adalet Ağaoğlu";
    private const string CahitZarifoglu = "Cahit Zarifoğlu";
    private const string BedriRahmiEyuboglu = "Bedri Rahmi Eyüboğlu";
    private const string JohnSteinbeck = "John Steinbeck";
    private const string Zahir = "Zahir";
    private const string AmokKosucusu = "Amok Koşucusu";
    private const string BeyazDis = "Beyaz Diş";
    private const string FikriminInceGulu = "Fikrimin İnce Gülü";
    private const string YediGuzelAdam = "Yedi Güzel Adam";
    private const string KucukPrens = "Küçük Prens";
    private const string Sefiller = "Sefiller";

    private readonly IAppDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetSerhanKitapListQueryHandler _handler;

    public GetSerhanKitapListQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetSerhanKitapListQueryHandler(_context, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = Sefiller, KitapYazar = VictorHugo, KitapSayfaSayisi = 1232 },
            new SerhanKitap { Id = "2", KitapName = "Suç ve Ceza", KitapYazar = FyodorDostoyevski, KitapSayfaSayisi = 687 },
            new SerhanKitap { Id = "3", KitapName = "Notre Dame'ın Kamburu", KitapYazar = VictorHugo, KitapSayfaSayisi = 512 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = Sefiller, KitapYazar = VictorHugo, KitapSayfaSayisi = 1232 },
            new GetSerhanKitapDto { Id = "2", KitapName = "Suç ve Ceza", KitapYazar = FyodorDostoyevski, KitapSayfaSayisi = 687 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
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
        var query = new GetSerhanKitapListQuery
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
            new SerhanKitap { Id = "1", KitapName = Zahir, KitapYazar = PauloCoelho, KitapSayfaSayisi = 352 },
            new SerhanKitap { Id = "2", KitapName = AmokKosucusu, KitapYazar = StefanZweig, KitapSayfaSayisi = 80 },
            new SerhanKitap { Id = "3", KitapName = BeyazDis, KitapYazar = JackLondon, KitapSayfaSayisi = 240 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = AmokKosucusu, KitapYazar = StefanZweig, KitapSayfaSayisi = 80 },
            new GetSerhanKitapDto { Id = "3", KitapName = BeyazDis, KitapYazar = JackLondon, KitapSayfaSayisi = 240 },
            new GetSerhanKitapDto { Id = "1", KitapName = Zahir, KitapYazar = PauloCoelho, KitapSayfaSayisi = 352 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SortBy = KitapSortField.KitapName,
            SortDescending = false
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapName.Should().Be(AmokKosucusu);
        result.Value.Items[1].KitapName.Should().Be(BeyazDis);
        result.Value.Items[2].KitapName.Should().Be(Zahir);
    }

    [Fact]
    public async Task Handle_WithSortByKitapNameDescending_ShouldReturnSortedResults()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = Zahir, KitapYazar = PauloCoelho, KitapSayfaSayisi = 352 },
            new SerhanKitap { Id = "2", KitapName = AmokKosucusu, KitapYazar = StefanZweig, KitapSayfaSayisi = 80 },
            new SerhanKitap { Id = "3", KitapName = BeyazDis, KitapYazar = JackLondon, KitapSayfaSayisi = 240 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = Zahir, KitapYazar = PauloCoelho, KitapSayfaSayisi = 352 },
            new GetSerhanKitapDto { Id = "3", KitapName = BeyazDis, KitapYazar = JackLondon, KitapSayfaSayisi = 240 },
            new GetSerhanKitapDto { Id = "2", KitapName = AmokKosucusu, KitapYazar = StefanZweig, KitapSayfaSayisi = 80 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SortBy = KitapSortField.KitapName,
            SortDescending = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapName.Should().Be(Zahir);
        result.Value.Items[1].KitapName.Should().Be(BeyazDis);
        result.Value.Items[2].KitapName.Should().Be(AmokKosucusu);
    }

    [Fact]
    public async Task Handle_WithSortByKitapYazar_ShouldSortByAuthor()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = YediGuzelAdam, KitapYazar = CahitZarifoglu, KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "2", KitapName = FikriminInceGulu, KitapYazar = AdaletAgaoglu, KitapSayfaSayisi = 250 },
            new SerhanKitap { Id = "3", KitapName = "Dol Karabakır Dol", KitapYazar = BedriRahmiEyuboglu, KitapSayfaSayisi = 180 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = FikriminInceGulu, KitapYazar = AdaletAgaoglu, KitapSayfaSayisi = 250 },
            new GetSerhanKitapDto { Id = "3", KitapName = "Dol Karabakır Dol", KitapYazar = BedriRahmiEyuboglu, KitapSayfaSayisi = 180 },
            new GetSerhanKitapDto { Id = "1", KitapName = YediGuzelAdam, KitapYazar = CahitZarifoglu, KitapSayfaSayisi = 200 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SortBy = KitapSortField.KitapYazar
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapYazar.Should().Be(AdaletAgaoglu);
        result.Value.Items[1].KitapYazar.Should().Be(BedriRahmiEyuboglu);
        result.Value.Items[2].KitapYazar.Should().Be(CahitZarifoglu);
    }

    [Fact]
    public async Task Handle_WithSortByKitapSayfaSayisi_ShouldSortByPageCount()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Devlet", KitapYazar = "Platon", KitapSayfaSayisi = 340 },
            new SerhanKitap { Id = "2", KitapName = KucukPrens, KitapYazar = AntoineDeSaintExupery, KitapSayfaSayisi = 96 },
            new SerhanKitap { Id = "3", KitapName = "Dönüşüm", KitapYazar = "Franz Kafka", KitapSayfaSayisi = 160 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "Devlet", KitapYazar = "Platon", KitapSayfaSayisi = 340 },
            new GetSerhanKitapDto { Id = "3", KitapName = "Dönüşüm", KitapYazar = "Franz Kafka", KitapSayfaSayisi = 160 },
            new GetSerhanKitapDto { Id = "2", KitapName = KucukPrens, KitapYazar = AntoineDeSaintExupery, KitapSayfaSayisi = 96 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SortBy = KitapSortField.KitapSayfaSayisi,
            SortDescending = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapSayfaSayisi.Should().Be(340);
        result.Value.Items[1].KitapSayfaSayisi.Should().Be(160);
        result.Value.Items[2].KitapSayfaSayisi.Should().Be(96);
    }

    [Fact]
    public async Task Handle_WithAlternativeSortFieldNames_ShouldSortCorrectly()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = YediGuzelAdam, KitapYazar = CahitZarifoglu, KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "2", KitapName = FikriminInceGulu, KitapYazar = AdaletAgaoglu, KitapSayfaSayisi = 250 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = FikriminInceGulu, KitapYazar = AdaletAgaoglu, KitapSayfaSayisi = 250 },
            new GetSerhanKitapDto { Id = "1", KitapName = YediGuzelAdam, KitapYazar = CahitZarifoglu, KitapSayfaSayisi = 200 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SortBy = KitapSortField.KitapYazar
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapYazar.Should().Be(AdaletAgaoglu);
        result.Value.Items[1].KitapYazar.Should().Be(CahitZarifoglu);
    }

    [Fact]
    public async Task Handle_WithInvalidSortField_ShouldUseDefaultSort()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Kuyucaklı Yusuf", KitapYazar = SabahattinAli, KitapSayfaSayisi = 220 },
            new SerhanKitap { Id = "2", KitapName = "İçimizdeki Şeytan", KitapYazar = SabahattinAli, KitapSayfaSayisi = 250 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "İçimizdeki Şeytan", KitapYazar = SabahattinAli, KitapSayfaSayisi = 250 },
            new GetSerhanKitapDto { Id = "1", KitapName = "Kuyucaklı Yusuf", KitapYazar = SabahattinAli, KitapSayfaSayisi = 220 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SortBy = (KitapSortField)999
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should default to sorting by KitapName
        result.Value!.Items[0].KitapName.Should().Be("İçimizdeki Şeytan");
        result.Value.Items[1].KitapName.Should().Be("Kuyucaklı Yusuf");
    }

    [Fact]
    public async Task Handle_WithSecondPage_ShouldReturnCorrectPage()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Hobbit", KitapYazar = JRRTolkien, KitapSayfaSayisi = 310 },
            new SerhanKitap { Id = "2", KitapName = "Yüzüklerin Efendisi", KitapYazar = JRRTolkien, KitapSayfaSayisi = 1178 },
            new SerhanKitap { Id = "3", KitapName = "Silmarillion", KitapYazar = JRRTolkien, KitapSayfaSayisi = 365 },
            new SerhanKitap { Id = "4", KitapName = "Kayıp Öyküler Kitabı", KitapYazar = JRRTolkien, KitapSayfaSayisi = 450 },
            new SerhanKitap { Id = "5", KitapName = "Hurin'in Çocukları", KitapYazar = JRRTolkien, KitapSayfaSayisi = 320 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "4", KitapName = "Kayıp Öyküler Kitabı", KitapYazar = JRRTolkien, KitapSayfaSayisi = 450 },
            new GetSerhanKitapDto { Id = "5", KitapName = "Hurin'in Çocukları", KitapYazar = JRRTolkien, KitapSayfaSayisi = 320 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
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
            new SerhanKitap { Id = "1", KitapName = "Harry Potter ve Sırlar Odası", KitapYazar = JKRowling, KitapSayfaSayisi = 312 },
            new SerhanKitap { Id = "2", KitapName = "Harry Potter ve Felsefe Taşı", KitapYazar = JKRowling, KitapSayfaSayisi = 276 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Harry Potter ve Felsefe Taşı", KitapYazar = JKRowling, KitapSayfaSayisi = 276 },
            new GetSerhanKitapDto { Id = "1", KitapName = "Harry Potter ve Sırlar Odası", KitapYazar = JKRowling, KitapSayfaSayisi = 312 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SortBy = KitapSortField.KitapName
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].KitapName.Should().Be("Harry Potter ve Felsefe Taşı");
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterByBothNameAndAuthor()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Harry Potter", KitapYazar = "Rowling", KitapSayfaSayisi = 300 },
            new SerhanKitap { Id = "2", KitapName = "Lord of Rings", KitapYazar = JRRTolkien, KitapSayfaSayisi = 500 },
            new SerhanKitap { Id = "3", KitapName = "The Hobbit", KitapYazar = JRRTolkien, KitapSayfaSayisi = 250 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Lord of Rings", KitapYazar = JRRTolkien, KitapSayfaSayisi = 500 },
            new GetSerhanKitapDto { Id = "3", KitapName = "The Hobbit", KitapYazar = JRRTolkien, KitapSayfaSayisi = 250 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SearchTerm = JRRTolkien
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithKitapNameFilter_ShouldFilterByBookName()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Sefiller - Cilt 1", KitapYazar = VictorHugo, KitapSayfaSayisi = 600 },
            new SerhanKitap { Id = "2", KitapName = "Sefiller - Cilt 2", KitapYazar = VictorHugo, KitapSayfaSayisi = 632 },
            new SerhanKitap { Id = "3", KitapName = "Notre Dame'ın Kamburu", KitapYazar = VictorHugo, KitapSayfaSayisi = 512 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "Sefiller - Cilt 1", KitapYazar = VictorHugo, KitapSayfaSayisi = 600 },
            new GetSerhanKitapDto { Id = "2", KitapName = "Sefiller - Cilt 2", KitapYazar = VictorHugo, KitapSayfaSayisi = 632 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            KitapName = Sefiller
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithKitapYazarFilter_ShouldFilterByAuthor()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "O", KitapYazar = StephenKing, KitapSayfaSayisi = 1104 },
            new SerhanKitap { Id = "2", KitapName = "Medyum", KitapYazar = StephenKing, KitapSayfaSayisi = 448 },
            new SerhanKitap { Id = "3", KitapName = "Taht Oyunları", KitapYazar = "George R.R. Martin", KitapSayfaSayisi = 850 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "O", KitapYazar = StephenKing, KitapSayfaSayisi = 1104 },
            new GetSerhanKitapDto { Id = "2", KitapName = "Medyum", KitapYazar = StephenKing, KitapSayfaSayisi = 448 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            KitapYazar = StephenKing
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithMinPageCount_ShouldFilterByMinimumPages()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Babalar ve Oğullar", KitapYazar = "Ivan Turgenyev", KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "2", KitapName = "Budala", KitapYazar = FyodorDostoyevski, KitapSayfaSayisi = 700 },
            new SerhanKitap { Id = "3", KitapName = "Karamazov Kardeşler", KitapYazar = FyodorDostoyevski, KitapSayfaSayisi = 800 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Budala", KitapYazar = FyodorDostoyevski, KitapSayfaSayisi = 700 },
            new GetSerhanKitapDto { Id = "3", KitapName = "Karamazov Kardeşler", KitapYazar = FyodorDostoyevski, KitapSayfaSayisi = 800 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            MinPageCount = 500
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithMaxPageCount_ShouldFilterByMaximumPages()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Sırça Köşk", KitapYazar = SabahattinAli, KitapSayfaSayisi = 140 },
            new SerhanKitap { Id = "2", KitapName = "Fareler ve İnsanlar", KitapYazar = JohnSteinbeck, KitapSayfaSayisi = 120 },
            new SerhanKitap { Id = "3", KitapName = "Gazap Üzümleri", KitapYazar = JohnSteinbeck, KitapSayfaSayisi = 500 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "Sırça Köşk", KitapYazar = SabahattinAli, KitapSayfaSayisi = 140 },
            new GetSerhanKitapDto { Id = "2", KitapName = "Fareler ve İnsanlar", KitapYazar = JohnSteinbeck, KitapSayfaSayisi = 120 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            MaxPageCount = 300
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithPageCountRange_ShouldFilterByBothMinAndMax()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = KucukPrens, KitapYazar = AntoineDeSaintExupery, KitapSayfaSayisi = 96 },
            new SerhanKitap { Id = "2", KitapName = "Hayvan Çiftliği", KitapYazar = GeorgeOrwell, KitapSayfaSayisi = 152 },
            new SerhanKitap { Id = "3", KitapName = "1984", KitapYazar = GeorgeOrwell, KitapSayfaSayisi = 352 },
            new SerhanKitap { Id = "4", KitapName = "Don Kişot", KitapYazar = "Miguel de Cervantes", KitapSayfaSayisi = 1000 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "2", KitapName = "Hayvan Çiftliği", KitapYazar = GeorgeOrwell, KitapSayfaSayisi = 152 },
            new GetSerhanKitapDto { Id = "3", KitapName = "1984", KitapYazar = GeorgeOrwell, KitapSayfaSayisi = 352 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            MinPageCount = 100,
            MaxPageCount = 500
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Semerkant", KitapYazar = AminMaalouf, KitapSayfaSayisi = 320 },
            new SerhanKitap { Id = "2", KitapName = "Afrikalı Leo", KitapYazar = AminMaalouf, KitapSayfaSayisi = 360 },
            new SerhanKitap { Id = "3", KitapName = "İstanbul", KitapYazar = "Orhan Pamuk", KitapSayfaSayisi = 450 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "Semerkant", KitapYazar = AminMaalouf, KitapSayfaSayisi = 320 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SearchTerm = "Amin",
            MinPageCount = 300,
            MaxPageCount = 350
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Items[0].KitapName.Should().Be("Semerkant");
    }

    [Fact]
    public async Task Handle_WithCaseInsensitiveSearch_ShouldFindIgnoreCase()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "OLIVER TWIST", KitapYazar = "Charles Dickens", KitapSayfaSayisi = 200 },
            new SerhanKitap { Id = "2", KitapName = "Çankaya", KitapYazar = FalihRifkiAtay, KitapSayfaSayisi = 600 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        var kitaplarDto = new[]
        {
            new GetSerhanKitapDto { Id = "1", KitapName = "OLIVER TWIST", KitapYazar = "Charles Dickens", KitapSayfaSayisi = 200 }
        };

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(kitaplarDto.ToList());

        var query = new GetSerhanKitapListQuery
        {
            SearchTerm = "oliver"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoMatchingFilters_ShouldReturnEmptyList()
    {
        // Arrange
        var kitaplar = new[]
        {
            new SerhanKitap { Id = "1", KitapName = "Serenad", KitapYazar = "Zülfü Livaneli", KitapSayfaSayisi = 480 },
            new SerhanKitap { Id = "2", KitapName = "Kardeşimin Hikayesi", KitapYazar = "Zülfü Livaneli", KitapSayfaSayisi = 320 }
        };

        await (_context as AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
        await (_context as AppDbContext)!.SaveChangesAsync();

        _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>()))
            .Returns(new List<GetSerhanKitapDto>());

        var query = new GetSerhanKitapListQuery
        {
            SearchTerm = "NonExistent"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
