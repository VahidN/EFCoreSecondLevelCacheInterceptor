namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[TestClass]
public class LockProviderTests
{
    [TestMethod]
    public void Lock_ReturnsNonNullReleaser()
    {
        // Arrange
        using var lockProvider = new LockProvider();

        // Act
        using var releaser = lockProvider.Lock();

        // Assert
        Assert.IsInstanceOfType<IDisposable>(releaser);
    }

    [TestMethod]
    public async Task LockAsync_ReturnsNonNullReleaser()
    {
        // Arrange
        using var lockProvider = new LockProvider();

        // Act
        var releaser = await lockProvider.LockAsync();

        // Assert
        Assert.IsInstanceOfType<IDisposable>(releaser);
    }

    [TestMethod]
    public void Lock_CanBeDisposed()
    {
        // Arrange
        using var lockProvider = new LockProvider();
        var releaser = lockProvider.Lock();

        // Act
        releaser?.Dispose();

        // Assert
        // If no exception is thrown, the test passes
    }

    [TestMethod]
    public async Task LockAsync_CanBeDisposed()
    {
        // Arrange
        using var lockProvider = new LockProvider();
        var releaser = await lockProvider.LockAsync();

        // Act
        releaser?.Dispose();

        // Assert
        // If no exception is thrown, the test passes
    }

    [TestMethod]
    public void Dispose_DisposesLockProvider()
    {
        using (var lockProvider = new LockProvider())
        {
            // Assert
            // If no exception is thrown, the test passes
        }
    }
}