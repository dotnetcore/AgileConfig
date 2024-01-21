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
            Console.WriteLine("create env:{env} freesql dbcontext instance .");
            return new FreeSqlContext(FreeSQL.GetInstanceByEnv(env));
        }
    }
}
