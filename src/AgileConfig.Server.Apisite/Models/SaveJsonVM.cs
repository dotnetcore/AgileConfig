using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models;

[ExcludeFromCodeCoverage]
public class SaveJsonVM
{
    /// <summary>
    ///     Indicates whether to update existing configuration entries in patch mode.
    ///     <para>true (patch mode): only modify the entries included in the current submission.</para>
    ///     <para>false (full mode, default): remove any existing entries that are not present in the current submission.</para>
    /// </summary>
    public bool isPatch { get; set; }

    public string json { get; set; }
}

[ExcludeFromCodeCoverage]
public class SaveKVListVM
{
    /// <summary>
    ///     Indicates whether to update existing configuration entries in patch mode.
    ///     <para>true (patch mode): only modify the entries included in the current submission.</para>
    ///     <para>false (full mode, default): remove any existing entries that are not present in the current submission.</para>
    /// </summary>
    public bool isPatch { get; set; }

    public string str { get; set; }
}