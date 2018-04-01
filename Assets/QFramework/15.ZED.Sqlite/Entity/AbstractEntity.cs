//==============================================================
//create by ZED
//==============================================================

using Mono.Data.Sqlite;
using System;

[Serializable]
public class AbstractEntity {

    public static string ACCOUNT_IDENTITY_COLUMN = "account_identity";

    [Column]
    public long identity;

    [Column]
    public string accountIdentity;

    [Column]
    public string jsonInfo;

    /// <summary>
    /// 对象实例转数据库数据时调用
    /// </summary>
    public virtual void Pack()
    {

    }

    /// <summary>
    /// 数据库数据转对象实例时调用
    /// </summary>
    public virtual void UnPack()
    {

    }


    public virtual void UpDateDatabase(DatabaseHelper database, int oldVersion, int newVersion, Type clazz)
    {

    }

    /// <summary>
    /// 这个返回帐号的id，以此来区分同一个客户端不同帐号的数据
    /// </summary>
    /// <returns></returns>
    public static String BuildUserIdentity()
    {
        return string.Empty;
        //...TODO
    }
}
