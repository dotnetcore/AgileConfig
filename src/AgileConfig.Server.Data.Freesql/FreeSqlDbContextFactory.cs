using AgileConfig.Server.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Freesql
{
    public class FreeSqlDbContextFactory
    {
        /// <summary>
        /// 根据环境构建 dbcontext
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static FreeSqlContext Create(string env)
        {
            if (string.IsNullOrEmpty(env))
            {
                //如果没有环境，使用默认连接
                return new FreeSqlContext(FreeSQL.Instance);
            }

            //是否配置的环境的连接
            var envDbProvider = Global.Config[$"db:env:{env}:provider"];
            if (string.IsNullOrEmpty(envDbProvider))
            {
                //如果没有配置对应环境的连接，使用默认连接
                return new FreeSqlContext(FreeSQL.Instance);
            }

            Console.WriteLine("create env:{env} freesql dbcontext instance .");
            return new FreeSqlContext(FreeSQL.GetInstance(env));
        }
    }
}
