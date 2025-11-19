namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;

public class EngineVersion
{
    public int Id { get; set; }

    public required EngineProductVersion Commercial { get; set; }

    public required EngineProductVersion Retail { get; set; }
}