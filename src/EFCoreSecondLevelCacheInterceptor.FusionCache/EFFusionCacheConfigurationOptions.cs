namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Custom FusionCache options.
/// </summary>
public class EFFusionCacheConfigurationOptions
{
    /// <summary>
    ///     Name of the named cache! If it's not specified, the default cache will be used.
    ///     It must match the one provided during registration (services.AddFusionCache("__name__")).
    /// </summary>
    public string? NamedCache { set; get; }
}