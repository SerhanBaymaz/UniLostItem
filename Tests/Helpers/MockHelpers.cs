using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Helpers;

public static class MockHelpers
{
    public static Mock<DbSet<T>> CreateDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new AsyncEnumerator<T>(queryable.GetEnumerator()));

        mockDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<T, bool>> predicate, CancellationToken _) => queryable.FirstOrDefault(predicate));

        return mockDbSet;
    }

    private class AsyncEnumerator<T>(IEnumerator<T> enumerator) : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator = enumerator;

        public T Current => _enumerator.Current;
        public ValueTask DisposeAsync() { _enumerator.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() { return ValueTask.FromResult(_enumerator.MoveNext()); }
    }
}
