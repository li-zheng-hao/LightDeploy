using System;
using System.IO;
using System.Linq;
using LightDeployApp.Tables;
using SqlSugar;

namespace LightDeployApp;

public class DBHelper
{    public static string ConnectionString= @"DataSource=lightdeploydata.sqlite";

    public static void Init()
    {
        var db=GetClient();
        
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.SetStringDefaultLength(255).InitTables(typeof(TEnvironment),typeof(TService),typeof(TDeployHistory));
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
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            Console.WriteLine(UtilMethods.GetNativeSql(sql,pars));
        };
        return db;
    }
}