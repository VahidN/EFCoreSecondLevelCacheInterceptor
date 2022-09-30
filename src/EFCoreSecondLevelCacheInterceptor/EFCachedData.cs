using System;
using System.Runtime.Serialization;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Cached Data
/// </summary>
[Serializable]
[DataContract]
public class EFCachedData
{
    /// <summary>
    ///     DbDataReader's result.
    /// </summary>
    [DataMember]
    public EFTableRows? TableRows { get; set; }

    /// <summary>
    ///     DbDataReader's NonQuery result.
    /// </summary>
    [DataMember]
    public int NonQuery { get; set; }

    /// <summary>
    ///     DbDataReader's Scalar result.
    /// </summary>
    [DataMember]
    public object? Scalar { get; set; }

    /// <summary>
    ///     Is result of the query null?
    /// </summary>
    [DataMember]
    public bool IsNull { get; set; }
}