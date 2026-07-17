using Microsoft.Extensions.Options;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[TestClass]
public class LockProviderTests
{
    private readonly IOptions<EFCoreSecondLevelCacheSettings> _cacheSettings;

    public LockProviderTests()
    {
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        var settings = new EFCoreSecondLevelCacheSettings();
        cacheSettingsMock.Setup(c => c.Value).Returns(settings);

        _cacheSettings = cacheSettingsMock.Object;
    }

    [TestMethod]
    public void Dispose_DisposesLockProvider()
    {
        using (var lockProvider = new LockProvider(_cacheSettings))
        {
            // Assert
            // If no exception is thrown, the test passes
        }
    }
}