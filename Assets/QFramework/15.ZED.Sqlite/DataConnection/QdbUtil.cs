using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QdbKit
{
    public class QdbUtil
    {

        private static readonly string endDbName = ".db";

        public static bool IsExist(string dbName)
        {
            string path = string.Empty;
#if UNITY_EDITOR
            path = Application.streamingAssetsPath + "/" + dbName;
#elif UNITY_ANDROID
        path = Application.persistentDataPath + "/" + dbName;
#elif UNITY_IPHONE
        path = Application.persistentDataPath + "/" + dbName;
#elif UNITY_STANDALONE_OSX
        path = Application.dataPath + "/" + dbName;
#elif UNITY_STANDALONE_WIN
        path = Application.dataPath + "/" + dbName;
#endif
            return File.Exists(path);
        }

        public static string GetRowDBPath(string dbName)
        {
            dbName = dbName.Insert(dbName.Length, endDbName);
            string path = string.Empty;
#if UNITY_EDITOR
            path = "data source=" + Application.streamingAssetsPath + "/" + dbName;
#elif UNITY_ANDROID
        path = "URI=file:" + Application.persistentDataPath + "/" + dbName;
#elif UNITY_IPHONE
        path = "data source=" + Application.persistentDataPath + "/" + dbName;
#elif UNITY_STANDALONE_OSX
        path = "data source=" + Application.dataPath + "/" + dbName;
#elif UNITY_STANDALONE_WIN
        path = "data source=" + Application.dataPath + "/" + dbName;
#endif
            return path;
        }

    }
}
