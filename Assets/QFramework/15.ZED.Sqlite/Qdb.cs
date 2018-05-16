using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QdbKit;
using UnityEngine.UI;
using Mono.Data.Sqlite;

public class Qdb : MonoBehaviour {

    public Text text;

    QdbConnection connect;
    private void Start()
    {
        ///创建或者链接一个数据库
        connect = new QdbConnection("Qdb");
        ///在此数据库下创建一张表(不要多次执行创建和插入命令，会报错)
        //connect.CreateTable("userinfo", new string[] { "name", "age", "email", "phone" }, new string[] { "TEXT", "INTEGER", "TEXT", "TEXT" });
        /////在指定的表中插入数据
        //connect.InsertData("userinfo", new string[] { "'panghu'", "'12'", "'1111'", "'11111'" });
        //connect.InsertData("userinfo", new string[] { "'xiaofu'", "'11'", "'2222'", "'11112'" });
        //connect.InsertData("userinfo", new string[] { "'daxiong'", "'10'", "'3333'", "'11113'" });
        //connect.InsertData("userinfo", new string[] { "'duola'", "'13'", "'4444'", "'11114'" });
        //connect.InsertData("userinfo", new string[] { "'pangmei'", "'9'", "'5555'", "'11115'" });
        ////查询胖虎（panghu的年龄）
        SqliteDataReader reader = connect.ReadDataInTable("userinfo", new string[] { "name" }, new string[] { "age" }, new string[] { "==" }, new string[] { "'12'" });
        while (reader.Read())
        {
            text.text = reader.GetString(reader.GetOrdinal("name"));
        }
        ///关闭数据库链接
        connect.CloseQdb();
    }
}
