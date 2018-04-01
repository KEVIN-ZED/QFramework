//==============================================================
//create by ZED
//==============================================================
using System;

[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute : Attribute
{
    public string name { get; set; }
}