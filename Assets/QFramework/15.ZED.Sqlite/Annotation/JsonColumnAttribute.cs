//==============================================================
//create by ZED
//==============================================================

using System;

[AttributeUsage(AttributeTargets.Field)]
public class JsonColumnAttribute : Attribute
{
    public string name { get; set; }
}