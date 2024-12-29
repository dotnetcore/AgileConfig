using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models
{
    [ExcludeFromCodeCoverage]
    public class SaveJsonVM
    {
        /// <summary>
        /// 指示是否用补丁模式更新现有配置
        /// <para>true(补丁模式): 只修改本次提交包含的配置项.</para>
        /// <para>false(全量模式,默认):所有不存在于本次提交中的现有配置项都会被删除.</para>
        /// </summary>
        public bool isPatch { get; set; }
        public string json { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SaveKVListVM
    {
        /// <summary>
        /// 指示是否用补丁模式更新现有配置
        /// <para>true(补丁模式): 只修改本次提交包含的配置项.</para>
        /// <para>false(全量模式,默认):所有不存在于本次提交中的现有配置项都会被删除.</para>
        /// </summary>
        public bool isPatch { get; set; }
        public string str { get; set; }
    }
}