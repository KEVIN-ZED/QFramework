//==============================================================
//create by ZED
//==============================================================

using System.Collections;
using Mono.Data.Sqlite;
using System;
using UnityEngine;
using System.Data;
using System.IO;

public abstract class DatabaseHelperBase
{
    private const string Version_Key = "Version_Database";

    private int version = 0;
    protected SqliteConnection dbConnection;
    private SqliteCommand mCommand;
    private bool mIsInitializing;
    private string mDbName;
    private SqliteTransaction mTranscation;

    public DatabaseHelperBase(int version, string dbName)
    {
        this.version = version;
        this.mDbName = dbName;
    }

    public SqliteConnection InitDatabase()
    {
        if (dbConnection != null)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection = null;
            }
            else
            {
                return dbConnection;
            }
        }

        if (mIsInitializing)
        {
            throw new Exception("getDatabase called recursively");
        }
        bool isExitDB = IsExitDB(mDbName);
        SqliteConnection db = new SqliteConnection(GetConnectionString(mDbName));
        db.Open();
        OnOpen(db);
        try
        {
            mIsInitializing = true;
            dbConnection = db;
            int oldVersion = GetVersion();
            if (oldVersion != version || !isExitDB)
            {
                if (oldVersion == 0 || !isExitDB)
                {
                    OnCreate(db);
                }
                else
                {
                    if (oldVersion > version)
                    {
                        OnDowngrade(db, oldVersion, version);
                    }
                    else
                    {
                        OnUpgrade(db, oldVersion, version);
                    }
                }
                SetVersion(version);
            }
            return db;
        }
        finally
        {
            mIsInitializing = false;
        }
    }

    private string GetConnectionString(string dbName)
    {
#if UNITY_EDITOR
        return "data source=" + Application.dataPath + "/" + dbName;
#elif UNITY_ANDROID
        return "URI=file:" + Application.persistentDataPath + "/" + dbName;
#elif UNITY_IPHONE
        return "data source=" + Application.persistentDataPath + "/" + dbName;
#elif UNITY_STANDALONE_OSX
        return "data source=" + Application.dataPath + "/" + dbName;
#elif UNITY_STANDALONE_WIN
        return "data source=" + Application.dataPath + "/" + dbName;
#endif
    }

    private bool IsExitDB(string dbName)
    {
        string path = "";
#if UNITY_EDITOR
        path = Application.dataPath + "/" + dbName;
#elif UNITY_ANDROID
        path = Application.persistentDataPath + "/" + dbName;
#elif UNITY_IPHONE
        path = Application.persistentDataPath + "/" + dbName;
#elif UNITY_STANDALONE_OSX
        path = Application.dataPath + "/" + dbName;
#elif UNITY_STANDALONE_WIN
        path = Application.dataPath + "/" + dbName;
#endif
        //Debug.Log("IsExitDB = " + path + "__" + File.Exists(path));
        return File.Exists(path);
    }

    public int GetVersion()
    {
        return PlayerPrefs.GetInt(Version_Key, 0);
    }

    private void SetVersion(int version)
    {
        PlayerPrefs.SetInt(Version_Key, version);
    }

    public void OnOpen(SqliteConnection db) { }

    public abstract void OnDowngrade(SqliteConnection db, int oldVersion, int newVersion);

    public abstract void OnUpgrade(SqliteConnection db, int oldVersion, int newVersion);

    public abstract void OnCreate(SqliteConnection db);

    public SqliteCommand GetCommand()
    {
        mCommand = dbConnection.CreateCommand();
        return mCommand;
    }

    public void BeginTransaction()
    {
        mTranscation = dbConnection.BeginTransaction();
    }

    public void EndTransaction()
    {
        if(mTranscation != null)
        {
            mTranscation.Commit();
            mTranscation.Dispose();
            mTranscation = null;
        }
    }

    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public void CloseConnection()
    {
        //销毁Command
        if (mCommand != null)
        {
            mCommand.Cancel();
            mCommand.Dispose();
        }
        mCommand = null;

        //销毁Connection
        if (dbConnection != null)
        {
            dbConnection.Close();
            dbConnection.Dispose();
        }
        dbConnection = null;
    }
}
