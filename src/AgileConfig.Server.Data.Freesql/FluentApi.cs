using AgileConfig.Server.Data.Entity;
using FreeSql;

namespace AgileConfig.Server.Data.Freesql;

public class FluentApi
{
    public static void Config(IFreeSql freeSql)
    {
        freeSql.CodeFirst.Entity<Config>(eb =>
        {
            eb.Property(a => a.Value).HasColumnName("v");
            if (freeSql.Ado.DataType == DataType.Oracle)
                // Oracle nvarchar2 does not support length 4000; use -1 to map to CLOB.
                eb.Property(a => a.Value).HasMaxLength(-1);
            else
                eb.Property(a => a.Value).HasMaxLength(4000);
        });
        freeSql.CodeFirst.Entity<ConfigPublished>(eb =>
        {
            eb.Property(a => a.Value).HasColumnName("v");
            if (freeSql.Ado.DataType == DataType.Oracle)
                // Oracle nvarchar2 does not support length 4000; use -1 to map to CLOB.
                eb.Property(a => a.Value).HasMaxLength(-1);
            else
                eb.Property(a => a.Value).HasMaxLength(4000);
        });
        freeSql.CodeFirst.Entity<PublishDetail>(eb =>
        {
            eb.Property(a => a.Value).HasColumnName("v");
            if (freeSql.Ado.DataType == DataType.Oracle)
                // Oracle nvarchar2 does not support length 4000; use -1 to map to CLOB.
                eb.Property(a => a.Value).HasMaxLength(-1);
            else
                eb.Property(a => a.Value).HasMaxLength(4000);
        });
    }
}