//==============================================================
//create by ZED
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;

public class QueryBuilder
{
    private bool distinct = false;
    private StringBuilder whereClauseBuilder = null;
    private StringBuilder orderByBuilder = null;
    private List<string> bindArgs = new List<string>();
    private String having = null;
    private String groupBy = null;
    private int limit = -1;

    public const string OP_EQUALS               = "=";
    public const string OP_NOT_EQUALS           = "<>";
    public const string OP_GREATER_THAN         = ">";
    public const string OP_NOT_GREATER_THAN     = "<=";
    public const string OP_LESS_THAN            = "<";
    public const string OP_NOT_LESS_THAN        = ">=";
    public const string OP_LIKE                 = " like ";
    public const string OP_IN                   = " in ";

    public QueryBuilder setDistinct(bool distinct)
    {
        this.distinct = distinct;
        return this;
    }

    /**
     * Add a where clause block "{@code column} {@code operator} {@code value}" to the query.
     * @param column the column name in database.
     * @param operator the compare operator.
     *      Must be one of {@link QueryBuilder#OP_EQUALS},
     *      {@link QueryBuilder#OP_NOT_EQUALS},
     *      {@link QueryBuilder#OP_GREATER_THAN},
     *      {@link QueryBuilder#OP_NOT_GREATER_THAN},
     *      {@link QueryBuilder#OP_LESS_THAN},
     *      {@link QueryBuilder#OP_NOT_LESS_THAN}
     * @param value the column's value.
     * @return this object.
     */
    public QueryBuilder AddWhere(string column, string oper, object value)
    {
        if (value == null)
        {
            return this;
        }
        if (whereClauseBuilder == null)
        {
            whereClauseBuilder = new StringBuilder();
        }
        else if (whereClauseBuilder.Length > 0)
        {
            whereClauseBuilder.Append(" AND ");
        }
        string v;
        if (value.GetType().Equals(typeof(bool))) {
            v = ((bool) value? 1 : 0).ToString();
        } else {
            v = value.ToString();
        }
        whereClauseBuilder.Append(column).Append(oper);
        if(oper.Equals(OP_IN)) {
            object[] vv = (object[])value;
            int size = vv.Length;
            String s = "";
            for(int i = 0; i<size; i++) {
                s = s + (i == 0 ? "" : ",") + "?";
                bindArgs.Add(vv[i].ToString());
            }
            whereClauseBuilder.Append("(" + s + ")");
        } else {
            whereClauseBuilder.Append("?");
            bindArgs.Add(v);
        }
        return this;
    }

    public QueryBuilder AddLike(string column, object value)
    {
        return AddWhere(column, OP_LIKE, "%" + value + "%");
    }

    public QueryBuilder AddLeftLike(string column, object value)
    {
        return AddWhere(column, OP_LIKE, "%" + value);
    }

    public QueryBuilder AddRightLike(string column, object value)
    {
        return AddWhere(column, OP_LIKE, value + "%");
    }

    public QueryBuilder AddEquals(string column, object value)
    {
        return AddWhere(column, OP_EQUALS, value);
    }

    public QueryBuilder AddNotEquals(string column, object value)
    {
        return AddWhere(column, OP_NOT_EQUALS, value);
    }

    public QueryBuilder AddGreaterThan(string column, object value)
    {
        return AddWhere(column, OP_GREATER_THAN, value);
    }

    public QueryBuilder AddNotGreaterThan(string column, object value)
    {
        return AddWhere(column, OP_NOT_GREATER_THAN, value);
    }

    public QueryBuilder AddLessThan(string column, object value)
    {
        return AddWhere(column, OP_LESS_THAN, value);
    }

    public QueryBuilder AddNotLessThan(string column, object value)
    {
        return AddWhere(column, OP_NOT_GREATER_THAN, value);
    }

    public QueryBuilder AddIn(string column, object[] value)
    {
        return AddWhere(column, OP_IN, value);
    }

    public QueryBuilder AddDescendOrderBy(string column)
    {
        return SetOrderBy(column, true);
    }

    public QueryBuilder AddAscendOrderBy(string column)
    {
        return SetOrderBy(column, false);
    }
    public QueryBuilder AddAscendOrderBy(string column, bool isOrder)
    {
        return SetOrderBy(column, isOrder);
    }

    private QueryBuilder SetOrderBy(string orderBy, bool descending)
    {
        if (orderByBuilder == null)
        {
            orderByBuilder = new StringBuilder();
        }
        else if (orderByBuilder.Length > 0)
        {
            orderByBuilder.Append(",");
        }
        orderByBuilder.Append(orderBy).Append(descending ? " DESC" : " ASC");
        return this;
    }

    public QueryBuilder SetHaving(string having)
    {
        this.having = having;
        return this;
    }

    public QueryBuilder SetGroupBy(string groupBy)
    {
        this.groupBy = groupBy;
        return this;
    }

    public QueryBuilder SetLimit(int limit)
    {
        this.limit = limit;
        return this;
    }

    public bool IsDistinct()
    {
        return distinct;
    }

    public string GetWhereClause()
    {
        return whereClauseBuilder == null ? null : whereClauseBuilder.ToString();
    }

    public string[] GetValues()
    {
        if (bindArgs == null || bindArgs.Count <= 0)
        {
            return new String[0];
        }
        return bindArgs.ToArray();
    }

    public string ValuesToString()
    {
        return bindArgs.ToString();
    }

    public string GetOrderBy()
    {
        return orderByBuilder == null ? null : orderByBuilder.ToString();
    }

    public string GetHaving()
    {
        return having;
    }

    public string GetGroupBy()
    {
        return groupBy;
    }

    public string GetLimit()
    {
        if (limit < 0)
        {
            return null;
        }
        return limit.ToString();
    }
}

