using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;
using AutoMapper;
using Domain;
using Moq;
using Persistence;
using Tests.Helpers;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.SerhanKitaplar.Queries.GetSerhanKitapList
{
    public class GetSerhanKitapListQueryHandlerTests
    {
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
        public async Task Handle_ShouldReturnMappedList()
        {
            // Arrange
            var kitaplar = new List<SerhanKitap>
            {
                new SerhanKitap { Id = "1", KitapName = "A", KitapYazar = "Y1", KitapSayfaSayisi = 100 },
                new SerhanKitap { Id = "2", KitapName = "B", KitapYazar = "Y2", KitapSayfaSayisi = 200 }
            };
            await (_context as Persistence.AppDbContext)!.SerhanKitaplar.AddRangeAsync(kitaplar);
            await (_context as Persistence.AppDbContext)!.SaveChangesAsync();

            var kitaplarDto = new List<GetSerhanKitapDto>
            {
                new GetSerhanKitapDto { Id = "1", KitapName = "A", KitapYazar = "Y1", KitapSayfaSayisi = 100 },
                new GetSerhanKitapDto { Id = "2", KitapName = "B", KitapYazar = "Y2", KitapSayfaSayisi = 200 }
            };
            _mockMapper.Setup(x => x.Map<List<GetSerhanKitapDto>>(It.IsAny<List<SerhanKitap>>())).Returns(kitaplarDto);

            var query = new GetSerhanKitapListQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0].KitapName);
            Assert.Equal("B", result[1].KitapName);
        }
    }
}
