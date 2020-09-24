using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Freesql
{
    public class FluentApi
    {
        public static void Config(IFreeSql freeSql)
        {
            freeSql.CodeFirst.Entity<Config>(eb => {
                eb.Property(a => a.Value).HasColumnName("v");
                if (freeSql.Ado.DataType == FreeSql.DataType.Oracle)
                {
                    //因为oracle的nvarchar2的长度不支持4000，使用-1使用clob
                    eb.Property(a => a.Value).HasMaxLength(-1);
                } else
                {
                    eb.Property(a => a.Value).HasMaxLength(4000);
                }
            });
            freeSql.CodeFirst.Entity<ModifyLog>(eb => {
                eb.Property(a => a.Value).HasColumnName("v");
                if (freeSql.Ado.DataType == FreeSql.DataType.Oracle)
                {
                    //因为oracle的nvarchar2的长度不支持4000，使用-1使用clob
                    eb.Property(a => a.Value).HasMaxLength(-1);
                }
                else
                {
                    eb.Property(a => a.Value).HasMaxLength(4000);
                }
            });
        }
    }
}
