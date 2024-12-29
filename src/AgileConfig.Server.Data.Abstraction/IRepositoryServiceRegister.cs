using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Abstraction
{
    /// <summary>
    /// 如果新对接一种存储，需要实现此接口
    /// </summary>
    public interface IRepositoryServiceRegister
    {
        /// <summary>
        /// 根据 provider name 判断是否适合当前注册器
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        bool IsSuit4Provider(string provider);

        /// <summary>
        /// 注册固定的仓储
        /// </summary>
        /// <param name="sc"></param>
        void AddFixedRepositories(IServiceCollection sc);

        /// <summary>
        /// 根据环境获取仓储
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        T GetServiceByEnv<T>(IServiceProvider sp, string env) where T : class;
    }
}
