using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace QdbKit
{
    public class QdbConnection
    {

        private SqliteConnection mQdbConnection;
        private SqliteCommand mQdbcommand;
        private SqliteDataReader mQdbReader;
        private SqliteTransaction mQdbTransaction;

        /// <summary>
        /// 数据库名称
        /// </summary>
        private readonly string strQdbName = string.Empty;

        /// <summary>
        /// 链接或创建数据库
        /// </summary>
        /// <param name="dbName"></param>
        public QdbConnection(string dbName)
        {
            if (!string.IsNullOrEmpty(dbName))
                this.strQdbName = dbName;
            try
            {
                if (null == mQdbConnection)
                    mQdbConnection = new SqliteConnection(QdbUtil.GetRowDBPath(strQdbName));
                mQdbConnection.Open();
            }
            catch (SqliteException e)
            {
                Debug.Log(e.Message);
            }
        }

        private SqliteCommand CreateCommand()
        {
            SqliteCommand command = null;
            if (null != mQdbConnection)
                command = mQdbConnection.CreateCommand();
            return command;
        }

        private SqliteDataReader DoQueryReader(string queryStr)
        {
            mQdbcommand = mQdbConnection.CreateCommand();
            mQdbcommand.CommandText = queryStr;
            mQdbReader = mQdbcommand.ExecuteReader();
            return mQdbReader;
        }

        #region 数据操作组
        /// <summary>
        /// 在指定数据库中创建数据表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="colNames"></param>
        /// <param name="colTypes"></param>
        /// <returns></returns>
        public SqliteDataReader CreateTable(string tableName, string[] colNames, string[] colTypes)
        {
            string queryString = "CREATE TABLE " + tableName + "( " + colNames[0] + " " + colTypes[0];
            for (int i = 1; i < colNames.Length; i++)
            {
                queryString += ", " + colNames[i] + " " + colTypes[i];
            }
            queryString += "  ) ";
            return DoQueryReader(queryString);
        }

        /// <summary>
        /// 在指定数据库中读取整张数据表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public SqliteDataReader ReadFullTable(string tableName)
        {
            string queryString = "SELECT * FROM " + tableName;
            return DoQueryReader(queryString);
        }

        /// <summary>
        /// 在数据表中插入数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public SqliteDataReader InsertData(string tableName, string[] values)
        {
            int fieldCount = ReadFullTable(tableName).FieldCount;
            if (values.Length != fieldCount)
            {
                throw new SqliteException("数据长度和插入数据体长度不匹配！！");
            }
            string queryString = "INSERT INTO " + tableName + " VALUES (" + values[0];
            for (int i = 1; i < values.Length; i++)
            {
                queryString += ", " + values[i];
            }
            queryString += " )";
            return DoQueryReader(queryString);
        }

        /// <summary>
        /// 在数据表中删除数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="colNames"></param>
        /// <param name="operations"></param>
        /// <param name="colValues"></param>
        /// <returns></returns>
        public SqliteDataReader DeleteData(string tableName, string[] colNames, string[] operations, string[] colValues)
        {
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new SqliteException("字段名称和字段数值不对应！");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += "OR " + colNames[i] + operations[0] + colValues[i];
            }
            return DoQueryReader(queryString);
        }

        /// <summary>
        /// 在数据表中查询数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="items"></param>
        /// <param name="colNames"></param>
        /// <param name="operations"></param>
        /// <param name="colValues"></param>
        /// <returns></returns>
        public SqliteDataReader ReadDataInTable(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues)
        {
            string queryString = "SELECT " + items[0];
            for (int i = 1; i < items.Length; i++)
            {
                queryString += ", " + items[i];
            }
            queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
            for (int i = 0; i < colNames.Length; i++)
            {
                queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
            }
            return DoQueryReader(queryString);
        }

        /// <summary>
        /// 在数据表中更新数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="colNames"></param>
        /// <param name="colValues"></param>
        /// <param name="key"></param>
        /// <param name="operation"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqliteDataReader UpdateData(string tableName, string[] colNames, string[] colValues, string key, string operation, string value)
        {
            if (colNames.Length != colValues.Length)
            {
                throw new SqliteException("字段名称和字段数值不对应！");
            }

            string queryString = "UPDATE " + tableName + " SET " + colNames[0] + "=" + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += ", " + colNames[i] + "=" + colValues[i];
            }
            queryString += " WHERE " + key + operation + value;
            return DoQueryReader(queryString);
        }
        #endregion

        /// <summary>
        /// 关闭数据库
        /// </summary>
        public void CloseQdb()
        {
            if (null != mQdbReader)
                mQdbReader.Close();
            if (null != mQdbcommand)
                mQdbcommand.Cancel();
            if (null != mQdbConnection && mQdbConnection.State == System.Data.ConnectionState.Connecting)
                mQdbConnection.Close();
            mQdbReader = null;
            mQdbcommand = null;
            mQdbConnection = null;
        }
    }
}
