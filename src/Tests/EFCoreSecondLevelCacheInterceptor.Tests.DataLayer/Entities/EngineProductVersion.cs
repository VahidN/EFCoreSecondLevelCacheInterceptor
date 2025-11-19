namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;

public class EngineProductVersion
{
    public int Major { get; set; }

    public int Minor { get; set; }

    public int Revision { get; set; }

    public int Patch { get; set; }

    public override string ToString() => $"{Major}.{Minor}.{Revision}.{Patch}";
}