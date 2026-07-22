using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static EditorDefine;
using static EditorCommonUtility;

// 二进制表格还原为CSV工具。
public static class MenuExcelBytesRestore
{
    private const string LIST_SEPARATOR = ",";                      // List字段在单元格中的分隔符。
    private const string EXPORT_BOTH = "Both";                      // 导出到客户端和服务器。
    private const string EXPORT_NONE = "None";                      // 不导出字段。
    private const string ID = "ID";                                 // 固定列：唯一ID。
    private const string VARIABLE_NAME = "VariableName";            // 固定占位列：生成的变量名。
    private const string VARIABLE_COMMENT = "VariableComment";      // 固定占位列：变量注释。
    // CSV列信息。
    private class CSVColumn
    {
        public FieldInfo mField;                                    // 对应的数据字段,为空表示这是补出来的占位列。
        public bool mUseDataID;                                     // 是否使用ExcelData.mID作为数据。
        public string mName;                                        // CSV中的变量名。
        public string mType;                                        // CSV中的变量类型。
        public string mExport;                                      // CSV中的导出端。
        public string mComment;                                     // CSV中的中文注释。
        public string mReferenceTable;                              // CSV中的引用表信息。
        public string mListReference;                               // CSV中的列表配对或列表检查信息。
        public string mCheckRule;                                   // CSV中的额外检查规则。
    }
    [MenuItem(MENU_ROOT_NAME + "工具/还原二进制表格为CSV")]
    private static void restoreExcelBytesToCSV()
    {
        List<string> bytesPathList = getSelectedBytesPathList();
        try
        {
            for (int i = 0; i < bytesPathList.Count; ++i)
            {
                string bytesPath = bytesPathList[i];
                displayProgressBar("还原二进制表格为CSV", Path.GetFileName(bytesPath), i, bytesPathList.Count);

                string tableName = Path.GetFileNameWithoutExtension(bytesPath);
                string typeName = "ED" + tableName;
                string outputPath = Path.Combine(Path.GetDirectoryName(bytesPath) ?? Application.dataPath, tableName + ".csv");
                restore(bytesPath, outputPath, tableName, typeName);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    private static List<string> getSelectedBytesPathList()
    {
        List<string> bytesPathList = new();
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (assetPath.isEmpty() || !assetPath.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            bytesPathList.addUnique(assetPathToFullPath(assetPath));
        }
        return bytesPathList;
    }
    private static string assetPathToFullPath(string assetPath)
    {
        string projectPath = Directory.GetParent(Application.dataPath)?.FullName;
        if (projectPath.isEmpty())
        {
            return assetPath;
        }
        return Path.GetFullPath(Path.Combine(projectPath, assetPath));
    }
    private static void restore(string bytesPath, string outputPath, string tableName, string typeName)
    {
        try
        {
            if (!File.Exists(bytesPath))
            {
                Debug.LogError("二进制表格文件不存在:" + bytesPath);
                return;
            }
            Type dataType = findType(typeName);
            if (dataType == null)
            {
                Debug.LogWarning("找不到表格数据类型:" + typeName + "，已跳过");
                return;
            }
            if (!typeof(ExcelData).IsAssignableFrom(dataType))
            {
                Debug.LogError("类型不是ExcelData子类:" + dataType.FullName);
                return;
            }

            byte[] fileBuffer = File.ReadAllBytes(bytesPath);
            ExcelTable.decodeFile(fileBuffer, tableName);
            List<ExcelData> dataList = parseDataList(fileBuffer, dataType, tableName);
            Dictionary<string, string> fieldCommentMap = collectFieldCommentMap(dataType);
            writeCSV(outputPath, tableName, dataList, createColumnList(dataType, fieldCommentMap));
            AssetDatabase.Refresh();

            Debug.Log("表格还原完成, 表格:" + tableName + ", 类型:" + dataType.FullName + ", 数据数量:" + dataList.Count + ", 输出路径:" + outputPath);
            EditorUtility.RevealInFinder(outputPath);
        }
        catch (Exception e)
        {
            Debug.LogError("还原表格失败:" + e);
        }
    }
    private static List<ExcelData> parseDataList(byte[] fileBuffer, Type dataType, string tableName)
    {
        List<ExcelData> dataList = new();
        SerializerRead reader = new();
        reader.init(fileBuffer);
        while (reader.getIndex() < reader.getDataSize())
        {
            int oldIndex = reader.getIndex();
            if (Activator.CreateInstance(dataType) is not ExcelData data)
            {
                Debug.LogError("创建表格数据失败:" + dataType.FullName);
                break;
            }
            if (!data.read(reader))
            {
                Debug.LogError("表格解析失败, 表格:" + tableName + ", 类型:" + dataType.FullName + ", 当前索引:" + oldIndex + ", ID:" + data.mID);
                break;
            }
            if (reader.getIndex() <= oldIndex)
            {
                Debug.LogError("表格解析索引没有推进, 表格:" + tableName + ", 类型:" + dataType.FullName + ", 当前索引:" + oldIndex);
                break;
            }
            dataList.Add(data);
        }
        return dataList;
    }
    private static List<FieldInfo> collectFields(Type dataType)
    {
        List<Type> typeList = new();
        Type curType = dataType;
        while (curType != null && typeof(ExcelData).IsAssignableFrom(curType))
        {
            if (!typeList.addIf(curType, curType != typeof(ExcelData)))
            {
                break;
            }
            curType = curType.BaseType;
        }

        typeList.Reverse();

        List<FieldInfo> fieldList = new();
        foreach (Type type in typeList)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (FieldInfo field in fields.OrderBy(field => field.MetadataToken))
            {
                fieldList.addIf(field, !field.IsStatic && !field.IsNotSerialized);
            }
        }
        return fieldList;
    }
    private static List<CSVColumn> createColumnList(Type dataType, Dictionary<string, string> fieldCommentMap)
    {
        List<FieldInfo> fieldList = collectFields(dataType);
        List<CSVColumn> columnList = new();
        columnList.Add(createIDColumn());
        columnList.Add(createPlaceholderColumn(VARIABLE_NAME, "string", EXPORT_NONE, "生成的变量名"));
        columnList.Add(createPlaceholderColumn(VARIABLE_COMMENT, "string", EXPORT_NONE, "注释"));

        foreach (FieldInfo field in fieldList)
        {
            string csvName = fieldNameToCSVName(field.Name);
            if (csvName == ID || csvName == VARIABLE_NAME || csvName == VARIABLE_COMMENT)
            {
                continue;
            }
            columnList.Add(createFieldColumn(field, fieldCommentMap));
        }
        return columnList;
    }
    private static CSVColumn createIDColumn()
    {
        return new CSVColumn()
        {
            mField = null,
            mUseDataID = true,
            mName = ID,
            mType = "int",
            mExport = EXPORT_BOTH,
            mComment = "唯一ID",
            mReferenceTable = "",
            mListReference = "",
            mCheckRule = "",
        };
    }
    private static CSVColumn createFieldColumn(FieldInfo field, Dictionary<string, string> fieldCommentMap)
    {
        string csvName = fieldNameToCSVName(field.Name);
        return new CSVColumn()
        {
            mField = field,
            mUseDataID = false,
            mName = csvName,
            mType = typeToExcelType(field.FieldType),
            mExport = EXPORT_BOTH,
            mComment = getFieldComment(field, csvName, fieldCommentMap),
            mReferenceTable = "",
            mListReference = "",
            mCheckRule = "",
        };
    }
    private static CSVColumn createPlaceholderColumn(string name, string type, string export, string comment)
    {
        return new CSVColumn()
        {
            mField = null,
            mUseDataID = false,
            mName = name,
            mType = type,
            mExport = export,
            mComment = comment,
            mReferenceTable = "",
            mListReference = "",
            mCheckRule = "",
        };
    }
    private static void writeCSV(string outputPath, string tableName, List<ExcelData> dataList, List<CSVColumn> columnList)
    {
        string directory = Path.GetDirectoryName(outputPath);
        if (!directory.isEmpty() && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using StreamWriter writer = new(outputPath, false, new UTF8Encoding(true));

        writeRow(writer, columnList.Count, 0, tableName + "表格");
        writeRow(writer, columnList.Count, 0, EXPORT_BOTH);
        writeRow(writer, columnList.Select(column => column.mName));
        writeRow(writer, columnList.Select(column => column.mType));
        writeRow(writer, columnList.Select(column => column.mExport));
        writeRow(writer, columnList.Select(column => column.mComment));
        writeRow(writer, columnList.Select(column => column.mReferenceTable));
        writeRow(writer, columnList.Select(column => column.mListReference));
        writeRow(writer, columnList.Select(column => column.mCheckRule));

        foreach (ExcelData data in dataList)
        {
            List<string> valueList = new();
            foreach (CSVColumn column in columnList)
            {
                if (column.mUseDataID)
                {
                    valueList.Add(data.mID.ToString(CultureInfo.InvariantCulture));
                }
                else if (column.mField == null)
                {
                    valueList.Add("");
                }
                else
                {
                    valueList.Add(valueToString(column.mField.GetValue(data)));
                }
            }
            writeRow(writer, valueList);
        }
    }
    private static void writeRow(StreamWriter writer, int columnCount, int valueIndex, string value)
    {
        for (int i = 0; i < columnCount; ++i)
        {
            if (i > 0)
            {
                writer.Write(",");
            }
            writer.Write(i == valueIndex ? escapeCSV(value) : "");
        }
        writer.WriteLine();
    }
    private static void writeRow(StreamWriter writer, IEnumerable<string> valueList)
    {
        bool first = true;
        foreach (string value in valueList)
        {
            if (!first)
            {
                writer.Write(",");
            }
            first = false;
            writer.Write(escapeCSV(value));
        }
        writer.WriteLine();
    }
    private static string fieldNameToCSVName(string fieldName)
    {
        if (fieldName == "mID")
        {
            return ID;
        }
        if (fieldName.Length > 1 && fieldName[0] == 'm' && char.IsUpper(fieldName[1]))
        {
            return fieldName.Substring(1);
        }
        return fieldName;
    }
    private static string typeToExcelType(Type type)
    {
        if (type == typeof(string))
        {
            return "string";
        }
        if (type == typeof(bool))
        {
            return "bool";
        }
        if (type == typeof(byte))
        {
            return "byte";
        }
        if (type == typeof(sbyte))
        {
            return "sbyte";
        }
        if (type == typeof(short))
        {
            return "short";
        }
        if (type == typeof(ushort))
        {
            return "ushort";
        }
        if (type == typeof(int))
        {
            return "int";
        }
        if (type == typeof(uint))
        {
            return "uint";
        }
        if (type == typeof(long))
        {
            return "llong";
        }
        if (type == typeof(ulong))
        {
            return "ullong";
        }
        if (type == typeof(float))
        {
            return "float";
        }
        if (type == typeof(double))
        {
            return "double";
        }
        if (type == typeof(Vector2Int))
        {
            return "Vector2Int";
        }
        if (type == typeof(Vector3Int))
        {
            return "Vector3Int";
        }
        if (type == typeof(Vector2))
        {
            return "Vector2";
        }
        if (type == typeof(Vector3))
        {
            return "Vector3";
        }
        if (type == typeof(Vector4))
        {
            return "Vector4";
        }
        if (type == typeof(Color))
        {
            return "Color";
        }
        if (type.IsEnum)
        {
            return type.Name + "(" + typeToExcelType(Enum.GetUnderlyingType(type)) + ")";
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return "Vector<" + typeToExcelType(type.GetGenericArguments()[0]) + ">";
        }
        if (type.IsArray)
        {
            return "Vector<" + typeToExcelType(type.GetElementType()) + ">";
        }
        return type.Name;
    }
    private static Dictionary<string, string> collectFieldCommentMap(Type dataType)
    {
        Dictionary<string, string> fieldCommentMap = new();
        foreach (string assetPath in collectScriptPathList(dataType))
        {
            string fullPath = assetPathToFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                continue;
            }
            string content = File.ReadAllText(fullPath, Encoding.UTF8);
            if (!content.Contains("class " + dataType.Name))
            {
                continue;
            }

            parseFieldComment(content, fieldCommentMap);
            if (fieldCommentMap.Count > 0)
            {
                break;
            }
        }
        return fieldCommentMap;
    }
    private static List<string> collectScriptPathList(Type dataType)
    {
        List<string> scriptPathList = new();
        foreach (string guid in AssetDatabase.FindAssets(dataType.Name + " t:MonoScript"))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.isEmpty())
            {
                continue;
            }

            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            Type scriptType = monoScript != null ? monoScript.GetClass() : null;
            if (scriptType == dataType || scriptType?.Name == dataType.Name || Path.GetFileNameWithoutExtension(assetPath) == dataType.Name)
            {
                scriptPathList.addUnique(assetPath);
            }
        }

        if (scriptPathList.Count > 0)
        {
            return scriptPathList;
        }

        foreach (string guid in AssetDatabase.FindAssets("t:MonoScript"))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.isEmpty())
            {
                continue;
            }
            string fullPath = assetPathToFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                continue;
            }
            string content = File.ReadAllText(fullPath, Encoding.UTF8);
            scriptPathList.addUniqueIf(assetPath, content.Contains("class " + dataType.Name));
        }
        return scriptPathList;
    }
    private static void parseFieldComment(string content, Dictionary<string, string> fieldCommentMap)
    {
        Regex fieldRegex = new(@"^\s*public\s+.+?\s+(m[A-Za-z0-9_]*)\s*(?:=\s*[^;]*)?;\s*//\s*(.*)$");
        foreach (string line in content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            Match match = fieldRegex.Match(line);
            if (!match.Success)
            {
                continue;
            }
            string fieldName = match.Groups[1].Value.Trim();
            string comment = match.Groups[2].Value.Trim();
            if (fieldName.isEmpty() || comment.isEmpty())
            {
                continue;
            }
            fieldCommentMap[fieldName] = comment;
        }
    }
    private static string getFieldComment(FieldInfo field, string csvName, Dictionary<string, string> fieldCommentMap)
    {
        if (fieldCommentMap.TryGetValue(field.Name, out string comment) && !comment.isEmpty())
        {
            return comment;
        }
        return guessComment(csvName, field.FieldType);
    }
    private static string guessComment(string csvName, Type fieldType)
    {
        if (csvName == ID)
        {
            return "唯一ID";
        }
        if (csvName == VARIABLE_NAME)
        {
            return "生成的变量名";
        }
        if (csvName == VARIABLE_COMMENT)
        {
            return "注释";
        }
        if (csvName.Contains("Description") || csvName.Contains("Comment"))
        {
            return "描述";
        }
        if (csvName.Contains("Path"))
        {
            return "文件路径,GameResources下的相对路径";
        }
        return "";
    }
    private static string valueToString(object value)
    {
        if (value == null)
        {
            return "";
        }
        Type type = value.GetType();
        if (type.IsEnum)
        {
            return Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
        }
        if (value is string str)
        {
            return str;
        }

        if (value is IList list)
        {
            List<string> valueList = new();
            foreach (object item in list)
            {
                valueList.Add(valueToString(item));
            }
            return valueList.stringsToString(LIST_SEPARATOR);
        }
        if (value is Vector2Int vector2Int)
        {
            return vector2Int.V2IToS();
        }
        if (value is Vector3Int vector3Int)
        {
            return vector3Int.V3IToS();
        }
        if (value is Vector2 vector2)
        {
            return vector2.V2ToS();
        }
        if (value is Vector3 vector3)
        {
            return vector3.V3ToS();
        }
        if (value is Vector4 vector4)
        {
            return vector4.V4ToS();
        }
        if (value is Color color)
        {
            return color.r.FToS() + "," + color.g.FToS() + "," + color.b.FToS() + "," + color.a.FToS();
        }
        if (value is bool boolValue)
        {
            return boolValue ? "1" : "0";
        }
        if (value is IFormattable formattable)
        {
            return formattable.ToString(null, CultureInfo.InvariantCulture);
        }
        return value.ToString();
    }
    private static string escapeCSV(string value)
    {
        if (value.isEmpty())
        {
            return "";
        }
        if (!(value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n')))
        {
            return value;
        }
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
    private static Type findType(string typeName)
    {
        Type type = Type.GetType(typeName);
        if (type != null)
        {
            return type;
        }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        foreach (Assembly assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }
            if (types.find(curType => curType.Name == typeName, out Type curType))
            {
                return curType;
            }
        }
        return null;
    }
}