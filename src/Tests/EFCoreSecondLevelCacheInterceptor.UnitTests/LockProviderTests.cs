using AsyncKeyedLock;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class LockProviderTests
{
    [Fact]
    public void Lock_ReturnsNonNullReleaser()
    {
        // Arrange
        var lockProvider = new LockProvider();

        // Act
        var releaser = lockProvider.Lock();

        // Assert
        Assert.IsType<AsyncNonKeyedLockReleaser>(releaser);
    }

    [Fact]
    public async Task LockAsync_ReturnsNonNullReleaser()
    {
        // Arrange
        var lockProvider = new LockProvider();

        // Act
        var releaser = await lockProvider.LockAsync();

        // Assert
        Assert.IsType<AsyncNonKeyedLockReleaser>(releaser);
    }

    [Fact]
    public void Lock_CanBeDisposed()
    {
        // Arrange
        var lockProvider = new LockProvider();
        var releaser = lockProvider.Lock();

        // Act
        releaser.Dispose();

        // Assert
        Assert.True(true); // If no exception is thrown, the test passes
    }

    [Fact]
    public async Task LockAsync_CanBeDisposed()
    {
        // Arrange
        var lockProvider = new LockProvider();
        var releaser = await lockProvider.LockAsync();

        // Act
        releaser.Dispose();

        // Assert
        Assert.True(true); // If no exception is thrown, the test passes
    }

    [Fact]
    public void Dispose_DisposesLockProvider()
    {
        // Arrange
        var lockProvider = new LockProvider();

        // Act
        lockProvider.Dispose();

        // Assert
        Assert.True(true); // If no exception is thrown, the test passes
    }
}