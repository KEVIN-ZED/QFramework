//==============================================================
//create by ZED
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;
using Mono.Data.Sqlite;
using System.Data;
using System.Text.RegularExpressions;
using UnityEngine;

public class DatabaseHelper : DatabaseHelperBase
{
    private const int Version = 9;
    private const string DbName = "QData.db";

    private static DatabaseHelper mInstance;

    private Type[] ENTITIES;

    public DatabaseHelper() : base(Version, DbName)
    {
        ENTITIES = new Type[] { };
    }

    public static DatabaseHelper GetInstance()
    {
        if(mInstance == null)
        {
            mInstance = new DatabaseHelper();
        }
        return mInstance;
    }

    public override void OnCreate(SqliteConnection db)
    {
        try
        {
            BeginTransaction();

            foreach(Type clazz in ENTITIES)
            {
                ExecuteSql(PersistenceHelper.GetDDL(clazz));
            }
            
        }
        catch (Exception e)
        {
            Debug.LogError("Create Database Failed! " + e.Message);
        }
        finally
        {
            EndTransaction();
        }
    }

    public override void OnUpgrade(SqliteConnection db, int oldVersion, int newVersion)
    {
        if (oldVersion > newVersion)
        {
            //Perform downgrade.
            RecreateDatabase(dbConnection);
        }
        else if (oldVersion < newVersion)
        {
            // 所有版本数据表的内容由各自表的Entity来控制
            foreach (Type clazz in ENTITIES)
            {
                try
                {
                    PersistenceHelper.InvokeStaticMethod(clazz, "UpDateDatabase", new object[] { this, oldVersion, newVersion, clazz });
                }
                catch (Exception e)
                {
                    Debug.LogError("Update Database Failed! class is " + clazz.Name + "error = " + e.Message);
                }
            }
            //RecreateDatabase(dbConnection);
        }
    }

    public override void OnDowngrade(SqliteConnection database, int oldVersion, int newVersion)
    {
        RecreateDatabase(database);
    }

    private void RecreateDatabase(SqliteConnection database)
    {
        try
        {
            BeginTransaction();
            String tableName;
            foreach (Type clazz in ENTITIES)
            {
                tableName = PersistenceHelper.GetTableName(clazz);
                if (tableName.Equals("account"))
                {
                    continue;
                }
                ExecuteSql("DROP TABLE IF EXISTS " + tableName);
                ExecuteSql(PersistenceHelper.GetDDL(clazz));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Recreate Database Failed! ");
        }
        finally
        {
            EndTransaction();
        }
    }

    /// <summary>
    /// 清除表数据
    /// </summary>
    /// <param name="deleteTables"></param>
    private void DoClean(List<Type> deleteTables)
    {
        foreach (Type aClass in ENTITIES)
        {
            try
            {
                if (deleteTables.Contains(aClass))
                {
                    ExecuteSql("delete from " + PersistenceHelper.GetTableName(aClass));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception occurred while cleaning database: " + e);
            }
        }
        ExecuteSql("VACUUM");
    }

    private void PrintCommand(string commandText, object[] args)
    {
        Debug.Log(commandText);
        if(args != null)
        {
            foreach (object obj in args)
            {
                if(obj != null)
                    Debug.Log(obj.ToString());
            }
        }
    }

    public SqliteDataReader ExecuteSql(string commandText, object[] args = null)
    {
        SqliteCommand command = GetCommand();
        command.CommandText = commandText;
        AttachParameters(command, args);
        return command.ExecuteReader();
    }

    public SqliteDataReader ExecuteDelete(string table, string whereClause, string[] whereArgs)
    {
        string commandText = "DELETE FROM " + table + (!string.IsNullOrEmpty(whereClause) ? " WHERE " + whereClause : "");
        SqliteCommand command = GetCommand();
        command.CommandText = commandText;
        AttachParameters(command, whereArgs);
        return command.ExecuteReader();
    }

    public SqliteDataReader ExecuteUpdate(string table, Dictionary<string, object> values, string whereClause, string[] whereArgs)
    {
        StringBuilder sql = new StringBuilder(120);
        sql.Append("UPDATE ");
        sql.Append(table);
        sql.Append(" SET ");

        // move all bind args to one array
        int setValuesSize = values.Count;
        int bindArgsSize = (whereArgs == null) ? setValuesSize : (setValuesSize + whereArgs.Length);
        object[] bindArgs = new object[bindArgsSize];
        int i = 0;
        foreach (string colName in values.Keys)
        {
            sql.Append((i > 0) ? "," : "");
            sql.Append(colName);
            bindArgs[i++] = values[colName];
            sql.Append("=?");
        }
        if (whereArgs != null)
        {
            for (i = setValuesSize; i < bindArgsSize; i++)
            {
                bindArgs[i] = whereArgs[i - setValuesSize];
            }
        }
        if (!string.IsNullOrEmpty(whereClause))
        {
            sql.Append(" WHERE ");
            sql.Append(whereClause);
        }
        string formatSql = sql.ToString();
        SqliteCommand command = GetCommand();
        command.CommandText = formatSql;
        AttachParameters(command, bindArgs);
        return command.ExecuteReader();
    }


    public SqliteDataReader ExecuteInsert(string table, Dictionary<string, object> initialValues, string nullColumnHack = "")
    {
        StringBuilder sql = new StringBuilder();
        sql.Append("INSERT");
        sql.Append(" INTO ");
        sql.Append(table);
        sql.Append('(');

        object[] bindArgs = null;
        int size = (initialValues != null && initialValues.Count > 0)
                ? initialValues.Count : 0;
        if (size > 0)
        {
            bindArgs = new object[size];
            int i = 0;
            foreach (string colName in initialValues.Keys)
            {
                sql.Append((i > 0) ? "," : "");
                sql.Append(colName);
                bindArgs[i++] = initialValues[colName];
            }
            sql.Append(')');
            sql.Append(" VALUES (");
            for (i = 0; i < size; i++)
            {
                sql.Append((i > 0) ? ",?" : "?");
            }
        }
        else
        {
            sql.Append(nullColumnHack + ") VALUES (NULL");
        }
        sql.Append(')');
        string formatSql = sql.ToString();
        SqliteCommand command = GetCommand();
        command.CommandText = formatSql;
        AttachParameters(command, bindArgs);
        return command.ExecuteReader();
    }

    public SqliteDataReader ExecuteQuery(string tables, string[] columns, bool distinct, string where, object[] bindArgs, string groupBy, string having, string orderBy, string limit)
    {
        if (string.IsNullOrEmpty(groupBy) && !string.IsNullOrEmpty(having))
        {
            throw new Exception(
                    "HAVING clauses are only permitted when using a groupBy clause");
        }
        Regex ex = new Regex("\\s*\\d+\\s*(,\\s*\\d+\\s*)?", RegexOptions.IgnoreCase);
        if (!string.IsNullOrEmpty(limit) && !ex.IsMatch(limit))
        {
            throw new Exception("invalid LIMIT clauses:" + limit);
        }

        StringBuilder query = new StringBuilder(120);

        query.Append("SELECT ");
        if (distinct)
        {
            query.Append("DISTINCT ");
        }
        if (columns != null && columns.Length != 0)
        {
            AppendColumns(query, columns);
        }
        else
        {
            query.Append("* ");
        }
        query.Append("FROM ");
        query.Append(tables);
        AppendClause(query, " WHERE ", where);
        AppendClause(query, " GROUP BY ", groupBy);
        AppendClause(query, " HAVING ", having);
        AppendClause(query, " ORDER BY ", orderBy);
        AppendClause(query, " LIMIT ", limit);

        string formatSql = query.ToString();
        SqliteCommand command = GetCommand();
        command.CommandText = formatSql;
        AttachParameters(command, bindArgs);
        return command.ExecuteReader();
    }

    private static void AppendClause(StringBuilder s, string name, string clause)
    {
        if (!string.IsNullOrEmpty(clause))
        {
            s.Append(name);
            s.Append(clause);
        }
    }

    private static void AppendColumns(StringBuilder s, string[] columns)
    {
        int n = columns.Length;

        for (int i = 0; i < n; i++)
        {
            String column = columns[i];

            if (column != null)
            {
                if (i > 0)
                {
                    s.Append(", ");
                }
                s.Append(column);
            }
        }
        s.Append(' ');
    }

    private void AttachParameters(SqliteCommand cmd, params object[] paramList)
    {
        //PrintCommand(cmd.CommandText, paramList);
        if(paramList != null)
        {
            SqliteParameterCollection collection = cmd.Parameters;
            foreach (object obj in paramList)
            {
                SqliteParameter parm = new SqliteParameter();
                parm.Value = obj;
                collection.Add(parm);
            }
        }
    }
}
