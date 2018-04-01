//==============================================================
//create by ZED
//==============================================================

using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;

public class AbstractDao
{
    private static QueryBuilder AppendUserSpecificWhereClause(Type clazz, QueryBuilder queryBuilder)
    {
        string where = queryBuilder.GetWhereClause();
        if (where != null && where.Contains(AbstractEntity.ACCOUNT_IDENTITY_COLUMN))
        {
            return queryBuilder;
        }
        queryBuilder.AddEquals(AbstractEntity.ACCOUNT_IDENTITY_COLUMN, AbstractEntity.BuildUserIdentity());
        return queryBuilder;
    }

    public static List<T> GetAll<T>(QueryBuilder queryBuilder = null)
    {
        Type clazz = typeof(T);
        if (queryBuilder == null)
        {
            queryBuilder = new QueryBuilder();
        }
        AppendUserSpecificWhereClause(clazz, queryBuilder);
        return GetAll<T>(clazz,
                queryBuilder.IsDistinct(),
                queryBuilder.GetWhereClause(),
                queryBuilder.GetValues(),
                queryBuilder.GetGroupBy(),
                queryBuilder.GetHaving(),
                queryBuilder.GetOrderBy(),
                queryBuilder.GetLimit());
    }

    public static int QueryCount<T>( QueryBuilder queryBuilder = null)
    {
        Type clazz = typeof(T);
        queryBuilder = (queryBuilder == null) ? new QueryBuilder() : queryBuilder;
        AppendUserSpecificWhereClause(clazz, queryBuilder);
        return QueryCount(clazz,
                queryBuilder.IsDistinct(),
                queryBuilder.GetWhereClause(),
                queryBuilder.GetValues(),
                queryBuilder.GetGroupBy(),
                queryBuilder.GetHaving(),
                queryBuilder.GetOrderBy(),
                queryBuilder.GetLimit());
    }

    public static void DeleteAllByQuery(Type clazz, QueryBuilder queryBuilder = null)
    {
        queryBuilder = (queryBuilder == null) ? new QueryBuilder() : queryBuilder;
        AppendUserSpecificWhereClause(clazz, queryBuilder);
        String sql = "DELETE FROM " + PersistenceHelper.GetTableName(clazz);
        String where = queryBuilder.GetWhereClause();
        if (where != null && where.Length > 0)
        {
            sql += " WHERE " + where;
        }
        DatabaseHelper.GetInstance().ExecuteSql(sql, queryBuilder.GetValues());
    }

    public static  T GetObject<T>(QueryBuilder queryBuilder = null)
    {
        if (queryBuilder == null)
        {
            queryBuilder = new QueryBuilder();
        }
        Type clazz = typeof(T);
        AppendUserSpecificWhereClause(clazz, queryBuilder);
        return GetObject<T>(clazz, queryBuilder.GetWhereClause(), queryBuilder.GetValues());
    }

    /// <summary>
    /// 这个方法用的时候要小心，获取到表里面的所有数据，不能区分是哪个user的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> GetAll_<T>()
    {
        Type clazz = typeof(T);
        SqliteDataReader cursor = DatabaseHelper.GetInstance().ExecuteQuery(PersistenceHelper.GetTableName(clazz),
                PersistenceHelper.GetColumnNames(clazz), false, null, null, null, null, null, null);

        if (cursor == null)
        {
            return null;
        }

        List<T> result = null;

        result = BuildResult<T>(clazz, cursor);
        cursor.Close();
        return result;
    }


    private static List<T> GetAll<T>(Type clazz, bool distinct, string whereClause, string[] values, string groupBy, string having, string orderBy, string limit)
    {
        SqliteDataReader cursor = DatabaseHelper.GetInstance().ExecuteQuery(PersistenceHelper.GetTableName(clazz),
                PersistenceHelper.GetColumnNames(clazz), distinct, whereClause, values, groupBy, having, orderBy, limit);
        if (cursor == null)
        {
            return null;
        }

        List<T> result = null;
        result = BuildResult<T>(clazz, cursor);
        cursor.Close();
        return result;
    }

    private static int QueryCount(Type clazz, bool distinct, string whereClause, string[] values, string groupBy, string having, string orderBy, string limit)
    {
        SqliteDataReader cursor = DatabaseHelper.GetInstance().ExecuteQuery(PersistenceHelper.GetTableName(clazz),
               PersistenceHelper.GetColumnNames(clazz), distinct, whereClause, values, groupBy, having, orderBy, limit);
        if (cursor == null)
        {
            return 0;
        }
        int count = 0;
        while (cursor.Read())
        {
            count++;
        }
        return count;
    }

    private static List<T> BuildResult<T>(Type clazz, SqliteDataReader cursor)
    {
        List<T> result = new List<T>();

        while(cursor.Read())
        {
            T obj = Activator.CreateInstance<T>();
            PersistenceHelper.Mapping(obj, cursor);
            result.Add(obj);
        }

        return result;
    }

    public static T GetObject<T>(Type clazz, long identity)
    {
        SqliteDataReader cursor = DatabaseHelper.GetInstance().ExecuteQuery(PersistenceHelper.GetTableName(clazz),
                PersistenceHelper.GetColumnNames(clazz), false, "identity=?", new string[] { identity.ToString() }, null, null, null, null);
        if (cursor == null)
        {
            return default(T);
        }

        T obj = BuildObject<T>(cursor, clazz);
        cursor.Close();
        return obj;
    }

    public static T GetObject<T>(Type clazz, string whereClause, string[] values)
    {
        SqliteDataReader cursor = DatabaseHelper.GetInstance().ExecuteQuery(PersistenceHelper.GetTableName(clazz),
                PersistenceHelper.GetColumnNames(clazz), false, whereClause, values, null, null, null, null);
        if (cursor == null)
        {
            return default(T);
        }

        if (cursor == null)
        {
            return default(T);
        }

        T obj = BuildObject<T>(cursor, clazz);
        cursor.Close();
        return obj;
    }

    private static T BuildObject<T>(SqliteDataReader cursor, Type clazz)
    {
        T obj = default(T);
        while (cursor.Read())
        {
            obj = Activator.CreateInstance<T>();
            PersistenceHelper.Mapping(obj, cursor);
        }
        return obj;
    }

    public static void Insert(AbstractEntity obj)
    {
        Dictionary<string, object> values = new Dictionary<string, object>();
        PersistenceHelper.Mapping(obj, values);
        SqliteDataReader cursor = DatabaseHelper.GetInstance().ExecuteInsert(PersistenceHelper.GetTableName(obj.GetType()), values);
    }

    public static bool InsertOrUpdate(AbstractEntity _new, AbstractEntity _old)
    {
        if (_old == null)
        {
            Insert(_new);
            return true;
        }
        else
        {
            _new.identity = _old.identity;
            Update(_new);
            return false;
        }
    }

    public static void Update(AbstractEntity obj)
    {
        if(obj == null)
        {
            return;
        }
        Dictionary<string, object> values = new Dictionary<string, object>();
        PersistenceHelper.Mapping(obj, values);
        DatabaseHelper.GetInstance().ExecuteUpdate(PersistenceHelper.GetTableName(obj.GetType()), values, "identity=?", new string[] { obj.identity.ToString() });
    }

    public static void Delete(AbstractEntity obj)
    {
        DatabaseHelper.GetInstance().ExecuteDelete(PersistenceHelper.GetTableName(obj.GetType()), "identity=?", new string[] { obj.identity.ToString() });
    }

    public static void Clear(Type clazz)
    {
        DatabaseHelper.GetInstance().ExecuteSql("DELETE FROM " + PersistenceHelper.GetTableName(clazz), null);
    }


}
