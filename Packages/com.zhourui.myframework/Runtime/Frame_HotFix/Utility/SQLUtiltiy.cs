using System.Collections.Generic;
using UnityEngine;
using static StringUtility;

// SQL语句构造相关工具函数类
public class SQLUtility
{
    public static void appendValueString(ref string queryStr, string str)
    {
        queryStr += "\"" + str + "\",";
    }
    public static void appendValueVector2(ref string queryStr, Vector2 value)
    {
        queryStr += value.V2ToS() + ",";
    }
    public static void appendValueVector2Int(ref string queryStr, Vector2Int value)
    {
        queryStr += value.V2IToS() + ",";
    }
    public static void appendValueVector3(ref string queryStr, Vector3 value)
    {
        queryStr += value.V3ToS() + ",";
    }
    public static void appendValueInt(ref string queryStr, int value)
    {
        queryStr += value.IToS() + ",";
    }
    public static void appendValueUInt(ref string queryStr, uint value)
    {
        queryStr += value.IToS() + ",";
    }
    public static void appendValueFloat(ref string queryStr, float value)
    {
        queryStr += value.FToS() + ",";
    }
    public static void appendValueFloats(ref string queryStr, List<float> floatArray)
    {
        appendValueString(ref queryStr, floatArray.FsToS());
    }
    public static void appendValueInts(ref string queryStr, List<int> intArray)
    {
        appendValueString(ref queryStr, intArray.IsToS());
    }
    public static void appendConditionString(ref string condition, string col, string str, string operate)
    {
        condition += col + " = " + "\"" + str + "\"" + operate;
    }
    public static void appendConditionInt(ref string condition, string col, int value, string operate)
    {
        condition += col + " = " + value.IToS() + operate;
    }
    public static void appendUpdateString(ref string updateStr, string col, string str)
    {
        updateStr += col + " = " + "\"" + str + "\",";
    }
    public static void appendUpdateInt(ref string updateStr, string col, int value)
    {
        updateStr += col + " = " + value.IToS() + ",";
    }
    public static void appendUpdateInts(ref string updateStr, string col, List<int> intArray)
    {
        appendUpdateString(ref updateStr, col, intArray.IsToS());
    }
    public static void appendUpdateFloats(ref string updateStr, string col, List<float> floatArray)
    {
        appendUpdateString(ref updateStr, col, floatArray.FsToS());
    }
}