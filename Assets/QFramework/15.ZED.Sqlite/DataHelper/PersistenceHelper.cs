//==============================================================
//create by ZED
//==============================================================

using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class PersistenceHelper
{
    private static Dictionary<Type, string> mTables = new Dictionary<Type, string>();
    private static Dictionary<Type, string[]> mColumnNames = new Dictionary<Type, string[]>();
    private static Dictionary<Type, Holder> mROORMappings = new Dictionary<Type, Holder>();

    public static string GetTableName(Type type)
    {
        string name = "";
        if(mTables.ContainsKey(type))
        {
            name = mTables[type];
        }
        else
        {
            MemberInfo info = type;
            object[] attrs = info.GetCustomAttributes(typeof(TableAttribute), false);
            if(attrs != null && attrs.Length > 0)
            {
                TableAttribute tableAttr = (TableAttribute)attrs[0];
                name = tableAttr.name;
                mTables.Add(type, name);
            }
        }
        return name;
    }

    public static string[] GetColumnNames(Type type, bool isContainsJsonField = false)
    {
        string[] names = new string[] { };
        if(mColumnNames.ContainsKey(type))
        {
            names = mColumnNames[type];
        }
        else
        {
            List<string> tempNames = new List<string>();
            FieldInfo[] fs = type.GetFields();
            foreach (FieldInfo f in fs)
            {
                object[] objs = f.GetCustomAttributes(false);

                if (objs != null)
                {
                    foreach (object obj in objs)
                    {
                        if (obj is ColumnAttribute)
                        {
                            tempNames.Add(ConvertColumnName(f.Name));
                        }
                        if(isContainsJsonField && obj is JsonColumnAttribute)
                        {
                            tempNames.Add(ConvertColumnName(f.Name));
                        }
                    }
                }
            }
            names = tempNames.ToArray();
            mColumnNames.Add(type, names);
        }
        return names;
    }

    public static string ConvertColumnName(string fieldName)
    {
        StringBuilder name = new StringBuilder();
        for(int i = 0, size = fieldName.Length; i < size; i++)
        {
            char ch = fieldName[i];
            if(Char.IsUpper(ch) && i != 0)
            {
                name.Append("_");
            }
            name.Append(Char.ToLower(ch));
        }
        return name.ToString();
    }

    public static string GetDDL(Type type)
    {
        StringBuilder buffer = new StringBuilder();
        string start = string.Format("CREATE TABLE {0}  (", GetTableName(type));
        buffer.Append(start);
        bool first = true;
        FieldInfo[] fs = type.GetFields();
        foreach (FieldInfo f in fs)
        {
            object[] objs = f.GetCustomAttributes(false);

            if (objs != null)
            {
                foreach (object obj in objs)
                {
                    if (obj is ColumnAttribute)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            buffer.Append(",");
                        }
                        if (f.Name.Equals("identity"))
                        {
                            buffer.Append("identity INTEGER PRIMARY KEY AUTOINCREMENT");
                        }
                        else
                        {
                            Type ft = f.FieldType;
                            String tableColumnName = ConvertColumnName(f.Name);
                            buffer.Append(tableColumnName);

                            if (ft.Equals(typeof(int)) || ft.Equals(typeof(long)))
                            {
                                buffer.Append(" INTEGER");
                            }
                            else if (ft.Equals(typeof(bool)))
                            {
                                buffer.Append(" INTEGER");
                            }
                            else if (ft.Equals(typeof(DateTime)))
                            {
                                buffer.Append(" TEXT");
                            }
                            else if (ft.Equals(typeof(string)))
                            {
                                buffer.Append(" TEXT");
                            }
                            else if (ft.Equals(typeof(float)))
                            {
                                buffer.Append(" REAL");
                            }
                        }
                    }
                }
            }
        }
        buffer.Append(");");
        return buffer.ToString();
    }

    private class Holder
    {
        public FieldInfo[] fields;
        public string[] colNames;
    }

    private static Holder GetHolder(Type type)
    {
        Holder holder;
        if(mROORMappings.ContainsKey(type))
        {
            holder = mROORMappings[type];
        }
        else
        {
            holder = new Holder();
            FieldInfo[] fs = type.GetFields();
            List<FieldInfo> tempL = new List<FieldInfo>();
            foreach (FieldInfo f in fs)
            {
                object[] objs = f.GetCustomAttributes(false);

                if (objs != null)
                {
                    foreach (object obj in objs)
                    {
                        if (obj is ColumnAttribute)
                        {
                            tempL.Add(f);
                        }
                    }
                }
            }
            holder.fields = tempL.ToArray();
            holder.colNames = GetColumnNames(type);
            mROORMappings.Add(type, holder);
        }
        return holder;
    }

    public static FieldInfo GetJsonObjField(Type entity)
    {
        FieldInfo[] fs = entity.GetFields();
        foreach (FieldInfo f in fs)
        {
            object[] objs = f.GetCustomAttributes(false);

            if (objs != null)
            {
                foreach (object obj in objs)
                {
                    if (obj is JsonColumnAttribute)
                    {
                        return f;
                    }
                }
            }
        }
        return null;
    }

    public static void Mapping(object entity, SqliteDataReader cursor)
    {
        if (entity == null)
            return;
        int index;
        FieldInfo field;
        Type type;
        string colName;
        Holder holder = GetHolder(entity.GetType());
        for(int i = 0, size = holder.fields.Length; i < size; i++)
        {
            colName = holder.colNames[i];
            index = cursor.GetOrdinal(colName);
            if(!string.IsNullOrEmpty(colName) && index != -1)
            {
                field = holder.fields[index];
                type = field.FieldType;
                if (type.Equals(typeof(DateTime)))
                {
                    DateTime date = DateTime.Parse(cursor.GetString(index));
                    field.SetValue(entity, date);
                }
                else if (type.Equals(typeof(string)) && field.Name.Equals("jsonInfo"))
                {
                    string jsonStr = cursor.GetString(index);
                    if (!string.IsNullOrEmpty(jsonStr))
                    {
                        FieldInfo jsonField = GetJsonObjField(entity.GetType());
                        if (jsonField != null)
                        {
                            object obj = JsonUtility.FromJson(jsonStr, jsonField.FieldType);
                            jsonField.SetValue(entity, obj);
                        }
                    }
                    field.SetValue(entity, jsonStr);
                }
                else if (type.Equals(typeof(int)))
                {
                    field.SetValue(entity, cursor.GetInt32(index));
                }
                else if(type.Equals(typeof(long)))
                {
                    field.SetValue(entity, cursor.GetInt64(index));
                }
                else if(type.Equals(typeof(bool)))
                {
                    field.SetValue(entity, cursor.GetInt16(index) == 1);
                }
                else if(type.Equals(typeof(float)))
                {
                    field.SetValue(entity, cursor.GetFloat(index));
                }
                else if(type.Equals(typeof(string)))
                {
                    field.SetValue(entity, cursor.GetString(index));
                }
            }
        }
        ((AbstractEntity)entity).UnPack();
    }

    public static void Mapping(object entity, Dictionary<string, object> values)
    {
        if (entity == null)
            return;
        FieldInfo field;
        Type type;
        string colName;
        Holder holder = GetHolder(entity.GetType());
        ((AbstractEntity)entity).Pack();
        for (int i = 0, size = holder.fields.Length; i < size; i++)
        {
            colName = holder.colNames[i];
            field = holder.fields[i];
            if (!string.IsNullOrEmpty(colName) && !colName.Equals("identity"))
            {
                type = field.FieldType;
                if (type.Equals(typeof(string)) && field.Name.Equals("jsonInfo"))
                {
                    FieldInfo jsonField = GetJsonObjField(entity.GetType());
                    if (jsonField != null)
                    {
                        object jsonObj = jsonField.GetValue(entity);
                        string jsonStr = JsonUtility.ToJson(jsonObj);
                        values.Add(colName, jsonStr == null ? "" : jsonStr);
                    }
                    else
                    {
                        values.Add(colName, "");
                    }
                }
                else if(type.Equals(typeof(string)) && field.Name.Equals("accountIdentity"))
                {
                    values.Add(colName, AbstractEntity.BuildUserIdentity());
                }
                else if (type.Equals(typeof(DateTime)))
                {
                    DateTime date = (DateTime)field.GetValue(entity);
                    if(date != null)
                    {
                        values.Add(colName, date.ToString());
                    }
                }
                else if(type.Equals(typeof(string)))
                {
                    object value = field.GetValue(entity);
                    values.Add(colName, value == null ? "" : value);
                }
                else
                {
                    values.Add(colName, field.GetValue(entity));
                }
            }
        }
    }
   
    public static void InvokeStaticMethod(Type type, string methodName, object[] args)
    {
        object obj = Activator.CreateInstance(type);
        type.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, obj, args);
    }


}

