using MediaHub.Application.Library;
using MediaHub.Domain.Library;

namespace MediaHub.Application.Tests.Library;

public sealed class LibraryRootCatalogTests
{
    private static readonly DateTimeOffset CurrentTime =
        new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AddAsyncValidatesAndPersistsAccessibleRoot()
    {
        var repository = new InMemoryLibraryRootRepository();
        var inspector = new StubLibraryPathInspector(
            LibraryPathInspection.Accessible(@"D:\Media", @"D:\MEDIA"));
        var catalog = new LibraryRootCatalog(
            repository,
            inspector,
            new FixedTimeProvider(CurrentTime));

        var result = await catalog.AddAsync(@"D:\Media", LibraryRootKind.Mixed);

        Assert.True(result.IsSuccess);
        var added = Assert.Single(await repository.GetAllAsync());
        Assert.Equal(LibraryRootKind.Mixed, added.Kind);
        Assert.Equal(CurrentTime, added.CreatedAtUtc);
    }

    [Fact]
    public async Task AddAsyncRejectsDuplicateNormalizedPath()
    {
        var repository = new InMemoryLibraryRootRepository();
        var inspector = new StubLibraryPathInspector(
            LibraryPathInspection.Accessible(@"D:\Media", @"D:\MEDIA"));
        var catalog = new LibraryRootCatalog(
            repository,
            inspector,
            new FixedTimeProvider(CurrentTime));

        var first = await catalog.AddAsync(@"D:\Media", LibraryRootKind.Movies);
        var second = await catalog.AddAsync(@"d:\media", LibraryRootKind.Movies);

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(AddLibraryRootFailure.Duplicate, second.Failure);
        Assert.Single(await repository.GetAllAsync());
    }

    [Theory]
    [InlineData(LibraryPathFailure.NotFound, AddLibraryRootFailure.NotFound)]
    [InlineData(LibraryPathFailure.AccessDenied, AddLibraryRootFailure.AccessDenied)]
    [InlineData(LibraryPathFailure.InputOutputError, AddLibraryRootFailure.InputOutputError)]
    public async Task AddAsyncMapsPathValidationFailure(
        LibraryPathFailure pathFailure,
        AddLibraryRootFailure expectedFailure)
    {
        var catalog = new LibraryRootCatalog(
            new InMemoryLibraryRootRepository(),
            new StubLibraryPathInspector(LibraryPathInspection.Failed(pathFailure)),
            new FixedTimeProvider(CurrentTime));

        var result = await catalog.AddAsync(@"Z:\Missing", LibraryRootKind.AutoDetect);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedFailure, result.Failure);
    }

    private sealed class StubLibraryPathInspector(
        LibraryPathInspection inspection) : ILibraryPathInspector
    {
        public Task<LibraryPathInspection> InspectAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            _ = path;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(inspection);
        }
    }

    private sealed class InMemoryLibraryRootRepository : ILibraryRootRepository
    {
        private readonly List<LibraryRoot> _roots = [];

        public Task<IReadOnlyList<LibraryRoot>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<LibraryRoot>>(_roots.ToArray());
        }

        public Task<LibraryRoot?> FindByNormalizedPathAsync(
            string normalizedPath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var root = _roots.FirstOrDefault(
                item => string.Equals(
                    item.NormalizedPath,
                    normalizedPath,
                    StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(root);
        }

        public Task AddAsync(
            LibraryRoot libraryRoot,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _roots.Add(libraryRoot);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
