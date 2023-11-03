using LightDeploy.DeployCli.Tables;
using SqlSugar;

namespace LightDeploy.DeployCli;

public class DBHelper
{    public static string ConnectionString= @"DataSource=lightdeploydata.sqlite";

    public static void Init()
    {
        var db=GetClient();
        
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.SetStringDefaultLength(255).InitTables(typeof(TEnvironment),typeof(TService));
    }

    public static SqlSugarClient GetClient()
    {
        SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = ConnectionString,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
            MoreSettings=new ConnMoreSettings()
            { 
                SqliteCodeFirstEnableDefaultValue = true //启用默认值
            }
        });
        return db;
    }
}