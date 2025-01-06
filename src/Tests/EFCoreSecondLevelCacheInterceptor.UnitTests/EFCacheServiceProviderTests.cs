using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class EFCacheServiceProviderTests
{
    [Fact]
    public void TestCacheInvalidationWithTwoRoots()
    {
        var efCacheServiceProvider = CreateMemoryCacheServiceProvider();

        var efCachePolicy = new EFCachePolicy().Timeout(TimeSpan.FromMinutes(value: 10))
            .ExpirationMode(CacheExpirationMode.Absolute);

        var key1 = new EFCacheKey(new HashSet<string>
        {
            "entity1.model",
            "entity2.model"
        })
        {
            KeyHash = "EF_key1"
        };

        efCacheServiceProvider.InsertValue(key1, new EFCachedData
        {
            Scalar = "value1"
        }, efCachePolicy);

        var key2 = new EFCacheKey(new HashSet<string>
        {
            "entity1.model",
            "entity2.model"
        })
        {
            KeyHash = "EF_key2"
        };

        efCacheServiceProvider.InsertValue(key2, new EFCachedData
        {
            Scalar = "value2"
        }, efCachePolicy);

        var value1 = efCacheServiceProvider.GetValue(key1, efCachePolicy);
        Assert.NotNull(value1);

        var value2 = efCacheServiceProvider.GetValue(key2, efCachePolicy);
        Assert.NotNull(value2);

        efCacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>
        {
            "entity2.model"
        })
        {
            KeyHash = "EF_key1"
        });

        value1 = efCacheServiceProvider.GetValue(key1, efCachePolicy);
        Assert.Null(value1);

        value2 = efCacheServiceProvider.GetValue(key2, efCachePolicy);
        Assert.Null(value2);
    }

    [Fact]
    public void TestCacheInvalidationWithOneRoot()
    {
        var efCacheServiceProvider = CreateMemoryCacheServiceProvider();

        var efCachePolicy = new EFCachePolicy().Timeout(TimeSpan.FromMinutes(value: 10))
            .ExpirationMode(CacheExpirationMode.Absolute);

        var key1 = new EFCacheKey(new HashSet<string>
        {
            "entity1"
        })
        {
            KeyHash = "EF_key1"
        };

        efCacheServiceProvider.InsertValue(key1, new EFCachedData
        {
            Scalar = "value1"
        }, efCachePolicy);

        var key2 = new EFCacheKey(new HashSet<string>
        {
            "entity1"
        })
        {
            KeyHash = "EF_key2"
        };

        efCacheServiceProvider.InsertValue(key2, new EFCachedData
        {
            Scalar = "value2"
        }, efCachePolicy);

        var value1 = efCacheServiceProvider.GetValue(key1, efCachePolicy);
        Assert.NotNull(value1);

        var value2 = efCacheServiceProvider.GetValue(key2, efCachePolicy);
        Assert.NotNull(value2);

        efCacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>
        {
            "entity1"
        })
        {
            KeyHash = "EF_key2"
        });

        value1 = efCacheServiceProvider.GetValue(key1, efCachePolicy);
        Assert.Null(value1);

        value2 = efCacheServiceProvider.GetValue(key2, efCachePolicy);
        Assert.Null(value2);
    }

    [Fact]
    public void TestObjectCacheInvalidationWithOneRoot()
    {
        var efCacheServiceProvider = CreateMemoryCacheServiceProvider();

        var efCachePolicy = new EFCachePolicy().Timeout(TimeSpan.FromMinutes(value: 10))
            .ExpirationMode(CacheExpirationMode.Absolute);

        const string rootCacheKey = "EFSecondLevelCache.Core.AspNetCoreSample.DataLayer.Entities.Product";

        efCacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>
        {
            rootCacheKey
        })
        {
            KeyHash = "EF_key1"
        });

        var key11888622 = new EFCacheKey(new HashSet<string>
        {
            rootCacheKey
        })
        {
            KeyHash = "11888622"
        };

        var val11888622 = efCacheServiceProvider.GetValue(key11888622, efCachePolicy);
        Assert.Null(val11888622);

        efCacheServiceProvider.InsertValue(key11888622, new EFCachedData
        {
            Scalar = "Test1"
        }, efCachePolicy);

        var key44513A63 = new EFCacheKey(new HashSet<string>
        {
            rootCacheKey
        })
        {
            KeyHash = "44513A63"
        };

        var val44513A63 = efCacheServiceProvider.GetValue(key44513A63, efCachePolicy);
        Assert.Null(val44513A63);

        efCacheServiceProvider.InsertValue(key44513A63, new EFCachedData
        {
            Scalar = "Test1"
        }, efCachePolicy);

        efCacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>
        {
            rootCacheKey
        })
        {
            KeyHash = "44513A63"
        });

        val11888622 = efCacheServiceProvider.GetValue(key11888622, efCachePolicy);
        Assert.Null(val11888622);

        val44513A63 = efCacheServiceProvider.GetValue(key44513A63, efCachePolicy);
        Assert.Null(val44513A63);
    }

    [Fact]
    public void TestCacheInvalidationWithSimilarRoots()
    {
        var efCacheServiceProvider = CreateMemoryCacheServiceProvider();

        var efCachePolicy = new EFCachePolicy().Timeout(TimeSpan.FromMinutes(value: 10))
            .ExpirationMode(CacheExpirationMode.Absolute);

        var key1 = new EFCacheKey(new HashSet<string>
        {
            "entity1",
            "entity2"
        })
        {
            KeyHash = "EF_key1"
        };

        efCacheServiceProvider.InsertValue(key1, new EFCachedData
        {
            Scalar = "value1"
        }, efCachePolicy);

        var key2 = new EFCacheKey(new HashSet<string>
        {
            "entity2"
        })
        {
            KeyHash = "EF_key2"
        };

        efCacheServiceProvider.InsertValue(key2, new EFCachedData
        {
            Scalar = "value2"
        }, efCachePolicy);

        var value1 = efCacheServiceProvider.GetValue(key1, efCachePolicy);
        Assert.NotNull(value1);

        var value2 = efCacheServiceProvider.GetValue(key2, efCachePolicy);
        Assert.NotNull(value2);

        efCacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>
        {
            "entity2"
        })
        {
            KeyHash = "EF_key2"
        });

        value1 = efCacheServiceProvider.GetValue(key1, efCachePolicy);
        Assert.Null(value1);

        value2 = efCacheServiceProvider.GetValue(key2, efCachePolicy);
        Assert.Null(value2);
    }

    [Fact]
    public void TestInsertingNullValues()
    {
        var efCacheServiceProvider = CreateMemoryCacheServiceProvider();

        var efCachePolicy = new EFCachePolicy().Timeout(TimeSpan.FromMinutes(value: 10))
            .ExpirationMode(CacheExpirationMode.Absolute);

        var key1 = new EFCacheKey(new HashSet<string>
        {
            "entity1",
            "entity2"
        })
        {
            KeyHash = "EF_key1"
        };

        efCacheServiceProvider.InsertValue(key1, value: null, efCachePolicy);

        var value1 = efCacheServiceProvider.GetValue(key1, efCachePolicy);
        Assert.True(value1.IsNull, $"value1 is `{value1}`");
    }

    [Fact]
    public async Task TestConcurrentCacheInsertAndInvalidation()
    {
        const string rootKey = "entity1";
        var efCacheServiceProvider = CreateMemoryCacheServiceProvider();

        var efCachePolicy = new EFCachePolicy().Timeout(TimeSpan.FromMinutes(value: 10))
            .ExpirationMode(CacheExpirationMode.Absolute);

        await Task.WhenAll(Task.Run(InsertValues), Task.Run(InvalidateCacheDependencies));

        void InsertValues()
        {
            for (var i = 0; i < 10000; i++)
            {
                var key = new EFCacheKey(new HashSet<string>
                {
                    rootKey
                })
                {
                    KeyHash = $"EF_key{i}"
                };

                efCacheServiceProvider.InsertValue(key, new EFCachedData
                {
                    Scalar = $"value{i}"
                }, efCachePolicy);
            }
        }

        void InvalidateCacheDependencies()
        {
            var defaultKey = new EFCacheKey(new HashSet<string>
            {
                rootKey
            })
            {
                KeyHash = "EF_key"
            };

            efCacheServiceProvider.InsertValue(defaultKey, new EFCachedData
            {
                Scalar = "value"
            }, efCachePolicy);

            for (var i = 0; i < 5000; i++)
            {
                efCacheServiceProvider.InvalidateCacheDependencies(defaultKey);
            }
        }
    }

    [Fact]
    public void TestParallelCacheInsertAndInvalidation()
    {
        const string rootKey = "entity1";
        var efCacheServiceProvider = CreateMemoryCacheServiceProvider();

        var efCachePolicy = new EFCachePolicy().Timeout(TimeSpan.FromMinutes(value: 10))
            .ExpirationMode(CacheExpirationMode.Absolute);

        var defaultKey = new EFCacheKey(new HashSet<string>
        {
            rootKey
        })
        {
            KeyHash = "EF_key"
        };

        efCacheServiceProvider.InsertValue(defaultKey, new EFCachedData
        {
            Scalar = "value"
        }, efCachePolicy);

        var tests = new List<Action>();

        for (var i = 0; i < 10000; i++)
        {
            var i1 = i;

            tests.Add(() =>
            {
                var key = new EFCacheKey(new HashSet<string>
                {
                    rootKey
                })
                {
                    KeyHash = $"EF_key{i1}"
                };

                efCacheServiceProvider.InsertValue(key, new EFCachedData
                {
                    Scalar = $"value{i1}"
                }, efCachePolicy);
            });

            tests.Add(() => efCacheServiceProvider.InvalidateCacheDependencies(defaultKey));
        }

        Parallel.Invoke(tests.OrderBy(a => Random.Shared.Next()).ToArray());
    }

    [Fact]
    public void TestParallelInsertsAndRemoves()
    {
        var efCacheServiceProvider = CreateMemoryCacheServiceProvider();

        var efCachePolicy = new EFCachePolicy().Timeout(TimeSpan.FromMinutes(value: 10))
            .ExpirationMode(CacheExpirationMode.Absolute);

        var tests = new List<Action>();

        for (var i = 0; i < 4000; i++)
        {
            var i1 = i;

            tests.Add(() => efCacheServiceProvider.InsertValue(new EFCacheKey(new HashSet<string>
            {
                "entity1",
                "entity2"
            })
            {
                KeyHash = $"EF_key{i1}"
            }, new EFCachedData
            {
                NonQuery = i1
            }, efCachePolicy));
        }

        for (var i = 0; i < 400; i++)
        {
            var i1 = i;

            if (i % 2 == 0)
            {
                tests.Add(() => efCacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>
                {
                    "entity1"
                })
                {
                    KeyHash = $"EF_key{i1}"
                }));
            }
            else
            {
                tests.Add(() => efCacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>
                {
                    "entity2"
                })
                {
                    KeyHash = $"EF_key{i1}"
                }));
            }
        }

        Parallel.Invoke(tests.OrderBy(a => Random.Shared.Next()).ToArray());

        var value1 = efCacheServiceProvider.GetValue(new EFCacheKey(new HashSet<string>
        {
            "entity1",
            "entity2"
        })
        {
            KeyHash = "EF_key1"
        }, efCachePolicy);

        Assert.Null(value1);
    }

    private static IEFCacheServiceProvider CreateMemoryCacheServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton<IMemoryCacheChangeTokenProvider, EFMemoryCacheChangeTokenProvider>();
        services.AddSingleton<IEFDebugLogger, EFDebugLogger>();
        var serviceProvider = services.BuildServiceProvider();

        var loggerMock = new Mock<IEFDebugLogger>();

        return new EFMemoryCacheServiceProvider(serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<IMemoryCacheChangeTokenProvider>(), loggerMock.Object);
    }
}