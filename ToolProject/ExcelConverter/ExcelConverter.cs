using System;
using System.Collections.Generic;
using System.Text;
using static MathUtility;

// 字段或者表格是客户端用还是服务器用
public enum OWNER : byte
{
	NONE,		// 客户端和服务器都不使用,一般是辅助填表的,方便阅读的
	CLIENT,		// 仅客户端用
	SERVER,		// 仅服务器用
	BOTH,		// 客户端和服务器都要用
}

// 表格信息
public class ExcelInfo
{
	public string mName;		// 表格名
	public string mComment;		// 表格注释
	public ExcelTable mTable;	// 表格对象
	public List<MemberInfo> mMemberList = new();	// 表格字段列表
	public OWNER mOwner;		// 表格是客户端用还是服务器用
}

// 字段信息
public class MemberInfo
{
	public string mMemberName;				// 字段名
	public string mMemberType;				// 字段类型,可以是枚举
	public string mMemberListElementType;	// 如果字段是列表类型的,则表示列表的元素类型,可以是枚举
	public string mMemberListWithEnumRealElementType;	// 如果字段是枚举的列表类型的,则表示转换以后的实际整数类型的列表类型比如List<byte>
	public string mEnumRealType;            // 如果mMemberType或者mMemberListElementType是枚举类型,则表示此枚举的实际整数类型
	public string mComment;					// 字段注释
	public string mSeperate;				// 如果是字符串的列表类型,则表示列表类型的分隔符,其他数据类型的列表不需要此分隔符,都是以逗号分隔
	public OWNER mOwner;					// 字段是客户端用还是服务器用
}

public class ExcelConverter : FileUtility
{
	public const int ROW_HEADER = 7;        // 前7行不是数据区域
	public static void generate(List<ExcelReader> readerList)
	{
		// 解析
		List<ExcelInfo> allExcelInfo = new();
		foreach (ExcelReader reader in readerList)
		{
			ExcelInfo info = parseTableMemberInfo(reader.getFilePath(), reader.getTable());
			if (info == null)
			{
				Console.ReadKey();
			}
			allExcelInfo.Add(info);
		}

		foreach (ExcelInfo item in allExcelInfo)
		{
			if (item.mOwner == OWNER.SERVER || item.mOwner == OWNER.NONE)
			{
				continue;
			}
			// 转换表格文件
			if (!convertTable(Config.mExcelBytesPath, item))
			{
				Console.ReadKey();
			}
			// 生成ExcelData.cs
			generateExcelDataFile(item, Config.mExcelDataHotFixPath);
			// 生成ExcelTable.cs
			generateExcelTableFile(item.mName, Config.mExcelTableHotFixPath);
		}

		// 在上一层目录生成ExcelRegister.cs
		generateCSharpExcelRegisteFileFile(allExcelInfo, getFilePath(Config.mExcelDataHotFixPath) + "/");
		generateCSharpExcelDeclare(allExcelInfo, Config.mHotFixCommonPath);
	}
	// 仅生成 bytes
	public static void generateBytesOnly(ExcelReader reader)
	{
		ExcelInfo item = parseTableMemberInfo(reader.getFilePath(), reader.getTable());
		if (item == null)
		{
			Console.ReadKey();
			return;
		}
		if (item.mOwner == OWNER.SERVER || item.mOwner == OWNER.NONE)
		{
			return;
		}
		if (!convertTable(Config.mExcelBytesPath, item))
		{
			Console.WriteLine("Convert error!");
			Console.ReadKey();
		}
	}
	// 仅生成调试用的txt数据文件
	public static void generateDataTxt(List<ExcelReader> readerList)
	{
		// 解析
		List<ExcelInfo> allExcelInfo = new();
		for (int i = 0; i < readerList.Count; ++i)
		{
			ExcelInfo info = parseTableMemberInfo(readerList[i].getFilePath(), readerList[i].getTable());
			if (info == null)
			{
				Console.ReadKey();
				return;
			}
			allExcelInfo.Add(info);
		}

		foreach (var item in allExcelInfo)
		{
			if (item.mOwner == OWNER.SERVER || item.mOwner == OWNER.NONE)
			{
				continue;
			}
			// 转换表格文件
			if (!convertTableToTxt(Config.mExcelBytesPath, item))
			{
				Console.ReadKey();
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ExcelTable.cs
	protected static void generateExcelTableFile(string tableName, string path)
	{
		string dataClassName = "ED" + tableName;
		string tableClassName = "Excel" + tableName;
		string fileContent = "";
		line(ref fileContent, "");
		line(ref fileContent, "public partial class " + tableClassName + " : ExcelTableT<" + dataClassName + ">");
		line(ref fileContent, "{}", false);
		writeTxtFileBOM(path + tableClassName + ".cs", GB2312ToUTF8(fileContent));
	}
	// ExcelData.cs
	protected static void generateExcelDataFile(ExcelInfo info, string path)
	{
		string file = "";
		string dataClassName = "ED" + info.mName;
		line(ref file, "using System;");
		line(ref file, "using System.Collections.Generic;");
		line(ref file, "using UnityEngine;");
		line(ref file, "");
		line(ref file, "// " + info.mComment);
		line(ref file, "public class " + dataClassName + " : ExcelData");
		line(ref file, "{");
		int memberCount = info.mMemberList.Count;
		for (int i = 0; i < memberCount; ++i)
		{
			MemberInfo member = info.mMemberList[i];
			if (member.mMemberName == "ID")
			{
				continue;
			}
			if (member.mOwner == OWNER.SERVER || member.mOwner == OWNER.NONE)
			{
				continue;
			}
			string typeName = member.mMemberType;
			string memberLine;
			if (typeName.StartsWith("List<"))
			{
				memberLine = "\tpublic " + typeName + " m" + member.mMemberName + " = new();";
			}
			else
			{
				memberLine = "\tpublic " + typeName + " m" + member.mMemberName + ";";
			}
			memberLine += EMPTY.PadRight(generateAlignTableCount(memberLine, 52), '\t');
			memberLine += "// " + info.mMemberList[i].mComment;
			line(ref file, memberLine);
		}
		line(ref file, "\tpublic override void read(SerializerRead reader)");
		line(ref file, "\t{");
		line(ref file, "\t\tbase.read(reader);");
		for (int i = 0; i < memberCount; ++i)
		{
			MemberInfo memberInfo = info.mMemberList[i];
			if (memberInfo.mMemberName == "ID")
			{
				continue;
			}
			if (memberInfo.mOwner == OWNER.SERVER || memberInfo.mOwner == OWNER.NONE)
			{
				continue;
			}
			string typeName = memberInfo.mMemberType;
			// 列表类型
			if (typeName.StartsWith("List<"))
			{
				// 列表中的枚举类型
				if (!isEmpty(memberInfo.mEnumRealType))
				{
					if (memberInfo.mEnumRealType == "sbyte")
					{
						line(ref file, "\t\treader.readEnumSByteList(m" + memberInfo.mMemberName + ");");
					}
					else if (memberInfo.mEnumRealType == "byte")
					{
						line(ref file, "\t\treader.readEnumByteList(m" + memberInfo.mMemberName + ");");
					}
					else if (memberInfo.mEnumRealType == "short")
					{
						line(ref file, "\t\treader.readEnumShortList(m" + memberInfo.mMemberName + ");");
					}
					else if (memberInfo.mEnumRealType == "ushort")
					{
						line(ref file, "\t\treader.readEnumUShortList(m" + memberInfo.mMemberName + ");");
					}
					else if (memberInfo.mEnumRealType == "int")
					{
						line(ref file, "\t\treader.readEnumIntList(m" + memberInfo.mMemberName + ");");
					}
					else if (memberInfo.mEnumRealType == "uint")
					{
						line(ref file, "\t\treader.readEnumUIntList(m" + memberInfo.mMemberName + ");");
					}
					else if (memberInfo.mEnumRealType == "long")
					{
						line(ref file, "\t\treader.readEnumLongList(m" + memberInfo.mMemberName + ");");
					}
					else if (memberInfo.mEnumRealType == "ulong")
					{
						line(ref file, "\t\treader.readEnumULongList(m" + memberInfo.mMemberName + ");");
					}
				}
				else
				{
					line(ref file, "\t\treader.readList(m" + memberInfo.mMemberName + ");");
				}
			}
			// 枚举类型
			else if (!isEmpty(memberInfo.mEnumRealType))
			{
				if (memberInfo.mEnumRealType == "sbyte")
				{
					line(ref file, "\t\treader.readEnumSByte(out m" + memberInfo.mMemberName + ");");
				}
				else if (memberInfo.mEnumRealType == "byte")
				{
					line(ref file, "\t\treader.readEnumByte(out m" + memberInfo.mMemberName + ");");
				}
				else if (memberInfo.mEnumRealType == "short")
				{
					line(ref file, "\t\treader.readEnumShort(out m" + memberInfo.mMemberName + ");");
				}
				else if (memberInfo.mEnumRealType == "ushort")
				{
					line(ref file, "\t\treader.readEnumUShort(out m" + memberInfo.mMemberName + ");");
				}
				else if (memberInfo.mEnumRealType == "int")
				{
					line(ref file, "\t\treader.readEnumInt(out m" + memberInfo.mMemberName + ");");
				}
				else if (memberInfo.mEnumRealType == "uint")
				{
					line(ref file, "\t\treader.readEnumUInt(out m" + memberInfo.mMemberName + ");");
				}
				else if (memberInfo.mEnumRealType == "long")
				{
					line(ref file, "\t\treader.readEnumLong(out m" + memberInfo.mMemberName + ");");
				}
				else if (memberInfo.mEnumRealType == "ulong")
				{
					line(ref file, "\t\treader.readEnumULong(out m" + memberInfo.mMemberName + ");");
				}
			}
			// string
			else if (typeName == "string")
			{
				line(ref file, "\t\treader.readString(out m" + memberInfo.mMemberName + ");");
			}
			// 其他类型
			else
			{
				line(ref file, "\t\treader.read(out m" + memberInfo.mMemberName + ");");
			}
		}
		line(ref file, "\t}");
		line(ref file, "}", false);
		writeTxtFileBOM(path + dataClassName + ".cs", GB2312ToUTF8(file));
	}
	// ExcelRegister.cs文件
	protected static void generateCSharpExcelRegisteFileFile(List<ExcelInfo> excelInfoList, string path)
	{
		string hotFixfile = "";
		line(ref hotFixfile, "using System;");
		line(ref hotFixfile, "using static GBR;");
		line(ref hotFixfile, "using static FrameBase;");
		line(ref hotFixfile, "");
		line(ref hotFixfile, "public class ExcelRegisterILR");
		line(ref hotFixfile, "{");
		line(ref hotFixfile, "\tpublic static void registeAll()");
		line(ref hotFixfile, "\t{");
		foreach (var item in excelInfoList)
		{
			if (item.mOwner == OWNER.SERVER || item.mOwner == OWNER.NONE)
			{
				continue;
			}
			string lineStr = "\t\tregisteTable(out mExcel%s, typeof(ED%s), \"%s\");";
			lineStr = replaceAll(lineStr, "%s", item.mName);
			line(ref hotFixfile, lineStr);
		}
		line(ref hotFixfile, "");
		line(ref hotFixfile, "\t\t// 进入热更以后,所有资源都处于可用状态");
		line(ref hotFixfile, "\t\tmExcelManager.resourceAvailable();");
		line(ref hotFixfile, "\t}");
		line(ref hotFixfile, "\t//------------------------------------------------------------------------------------------------------------------------------");
		line(ref hotFixfile, "\tprotected static void registeTable<T>(out T table, Type dataType, string tableName) where T : ExcelTable");
		line(ref hotFixfile, "\t{");
		line(ref hotFixfile, "\t\ttable = mExcelManager.registe(tableName, typeof(T), dataType) as T;");
		line(ref hotFixfile, "\t}");
		line(ref hotFixfile, "}", false);

		writeTxtFileBOM(path + "ExcelRegisterILR.cs", GB2312ToUTF8(hotFixfile));
	}
	// GameBaseExcel.cs文件
	protected static void generateCSharpExcelDeclare(List<ExcelInfo> infoList, string path)
	{
		// 热更工程中的表格注册
		string hotFixfile = "";
		line(ref hotFixfile, "using System;");
		line(ref hotFixfile, "");
		line(ref hotFixfile, "// FrameBase的部分类,用于定义Excel表格的对象");
		line(ref hotFixfile, "public partial class GBR");
		line(ref hotFixfile, "{");
		foreach (ExcelInfo info in infoList)
		{
			if (info.mOwner == OWNER.SERVER || info.mOwner == OWNER.NONE)
			{
				continue;
			}
			line(ref hotFixfile, "\tpublic static Excel" + info.mName + " mExcel" + info.mName + ";");
		}
		line(ref hotFixfile, "}", false);

		writeTxtFileBOM(path + "GameBaseExcelILR.cs", GB2312ToUTF8(hotFixfile));
	}
	protected static OWNER stringToOwner(string str)
	{
		if (str != null)
		{
			if (str == "Client")
			{
				return OWNER.CLIENT;
			}
			if (str == "Server")
			{
				return OWNER.SERVER;
			}
			if (str == "Both")
			{
				return OWNER.BOTH;
			}
			if (str == "None")
			{
				return OWNER.NONE;
			}
		}
		Console.WriteLine("归属转换错误:" + str);
		Console.ReadKey();
		return OWNER.NONE;
	}
	protected static ExcelInfo parseTableMemberInfo(string path, ExcelTable table)
	{
		ExcelInfo excelInfo = new();
		excelInfo.mTable = table;
		excelInfo.mName = getFileNameNoSuffix(path, true);
		int rowCount = table.getRowCount();
		int colCount = table.getColumnCount();
		if (rowCount <= ROW_HEADER || colCount <= 1)
		{
			Console.WriteLine("表格错误:行数:" + rowCount + ", 列数:" + colCount + ", 表名:" + excelInfo.mName);
			return null;
		}

		try
		{
			excelInfo.mComment = table.getCellString(0, 0, true);
			excelInfo.mOwner = stringToOwner(table.getCellString(0, 1, true));
			for (int i = 0; i < colCount; ++i)
			{
				// 第0行是每一列数据的类型,第1行是字段类型
				MemberInfo info = new();
				info.mOwner = stringToOwner(table.getCellString(1, i, true));
				if (info.mOwner == OWNER.SERVER || info.mOwner == OWNER.NONE)
				{
					info.mComment = table.getCellString(6, i, false);
					excelInfo.mMemberList.Add(info);
					continue;
				}
				info.mMemberName = table.getCellString(2, i, true);
				string memberType = table.getCellString(3, i, true);
				info.mComment = table.getCellString(6, i, true);
				if (memberType.StartsWith("List<"))
				{
					// 解析列表类型以及列表的元素类型
					string mainPart = memberType.Substring(0, memberType.IndexOf('>') + 1);
					string listElementType = mainPart.Substring(mainPart.IndexOf('<') + 1, mainPart.IndexOf('>') - mainPart.IndexOf('<') - 1);
					// 列表元素是带括号的枚举类型
					if (listElementType.Contains('('))
					{
						int preIndex = listElementType.IndexOf('(');
						int endIndex = listElementType.IndexOf(')');
						info.mMemberListElementType = listElementType.Substring(0, preIndex);
						info.mEnumRealType = listElementType.Substring(preIndex + 1, endIndex - preIndex - 1);
						info.mMemberType = "List<" + info.mMemberListElementType + ">";
						info.mMemberListWithEnumRealElementType = "List<" + info.mEnumRealType + ">";
					}
					// 列表元素是其他类型
					else
					{
						info.mMemberType = mainPart;
						info.mMemberListElementType = listElementType;
					}

					// 解析分隔符
					string endPart = memberType.Substring(memberType.IndexOf('>') + 1);
					// 没有填则是以换行符为分隔
					if (isEmpty(endPart))
					{
						info.mSeperate = "\n";
					}
					else
					{
						int preIndex = endPart.IndexOf('(');
						int endIndex = endPart.IndexOf(')');
						if (preIndex < 0 || endIndex < 0)
						{
							Console.WriteLine("列表分隔符错误: column:" + i + ", table:" + excelInfo.mName);
							return null;
						}
						info.mSeperate = endPart.Substring(preIndex + 1, endIndex - preIndex - 1);
					}
				}
				else
				{
					// 带括号的是枚举类型
					if (memberType.Contains('('))
					{
						int preIndex = memberType.IndexOf('(');
						int endIndex = memberType.IndexOf(')');
						info.mMemberType = memberType.Substring(0, preIndex);
						info.mEnumRealType = memberType.Substring(preIndex + 1, endIndex - preIndex - 1);
					}
					// 不带括号的是其他类型
					else
					{
						info.mMemberType = memberType;
					}
					info.mSeperate = null;
				}
				excelInfo.mMemberList.Add(info);
				checkType(info);
			}
		}
		catch (Exception e)
		{
			Console.WriteLine("解析表头时错误,表格:" + excelInfo.mName + ", info:" + e.Message);
			return null;
		}
		return excelInfo;
	}
	protected static void checkType(MemberInfo info)
	{
		bool isValid;
		if (info.mMemberType.StartsWith("List<"))
		{
			if (isEmpty(info.mEnumRealType))
			{
				isValid = info.mMemberListElementType == "sbyte" ||
						  info.mMemberListElementType == "byte" ||
						  info.mMemberListElementType == "short" ||
						  info.mMemberListElementType == "ushort" ||
						  info.mMemberListElementType == "int" ||
						  info.mMemberListElementType == "uint" ||
						  info.mMemberListElementType == "long" ||
						  info.mMemberListElementType == "ulong" ||
						  info.mMemberListElementType == "bool" ||
						  info.mMemberListElementType == "string" ||
						  info.mMemberListElementType == "Vector2" ||
						  info.mMemberListElementType == "Vector3" ||
						  info.mMemberListElementType == "Vector2Int" ||
						  info.mMemberListElementType == "Vector3Int" ||
						  info.mMemberListElementType == "float";
			}
			else
			{
				isValid = info.mEnumRealType == "sbyte" ||
						  info.mEnumRealType == "byte" ||
						  info.mEnumRealType == "short" ||
						  info.mEnumRealType == "ushort" ||
						  info.mEnumRealType == "int" ||
						  info.mEnumRealType == "uint" ||
						  info.mEnumRealType == "long" ||
						  info.mEnumRealType == "ulong";
			}
		}
		else
		{
			if (isEmpty(info.mEnumRealType))
			{
				isValid = info.mMemberType == "sbyte" ||
						  info.mMemberType == "byte" ||
						  info.mMemberType == "short" ||
						  info.mMemberType == "ushort" ||
						  info.mMemberType == "int" ||
						  info.mMemberType == "uint" ||
						  info.mMemberType == "long" ||
						  info.mMemberType == "ulong" ||
						  info.mMemberType == "bool" ||
						  info.mMemberType == "string" ||
						  info.mMemberType == "Vector2" ||
						  info.mMemberType == "Vector3" ||
						  info.mMemberType == "Vector2Int" ||
						  info.mMemberType == "Vector3Int" ||
						  info.mMemberType == "float";
			}
			else
			{
				isValid = info.mEnumRealType == "sbyte" ||
						  info.mEnumRealType == "byte" ||
						  info.mEnumRealType == "short" ||
						  info.mEnumRealType == "ushort" ||
						  info.mEnumRealType == "int" ||
						  info.mEnumRealType == "uint" ||
						  info.mEnumRealType == "long" ||
						  info.mEnumRealType == "ulong";
			}
		}
		if (!isValid)
		{
			Console.WriteLine("字段类型错误:" + info.mMemberType);
			Console.ReadKey();
		}
	}
	protected static bool convertTable(string path, ExcelInfo item)
	{
		string tableName = item.mName;
		ExcelTable table = item.mTable;
		List<MemberInfo> memberInfoList = item.mMemberList;
		try
		{
			int rowCount = table.getRowCount();
			int colCount = table.getColumnCount();
			// 生成二进制的data文件
			SerializerWrite fileWriter = new();
			for (int i = ROW_HEADER; i < rowCount; ++i)
			{
				for (int j = 0; j < colCount; ++j)
				{
					MemberInfo member = memberInfoList[j];
					if (member.mOwner == OWNER.SERVER || member.mOwner == OWNER.NONE)
					{
						continue;
					}
					string colType;
					// 枚举的列表类型
					if (!isEmpty(member.mMemberListWithEnumRealElementType))
					{
						colType = member.mMemberListWithEnumRealElementType;
					}
					// 枚举类型
					else if (!isEmpty(member.mEnumRealType))
					{
						colType = member.mEnumRealType;
					}
					// 其他类型
					else
					{
						colType = member.mMemberType;
					}
					if (colType == "int")
					{
						fileWriter.write(table.getCellInt(i, j));
					}
					else if (colType == "float")
					{
						fileWriter.write(table.getCellFloat(i, j));
					}
					else if (colType == "bool")
					{
						fileWriter.write(table.getCellInt(i, j) > 0);
					}
					else if (colType == "byte")
					{
						fileWriter.write((byte)table.getCellInt(i, j));
					}
					else if (colType == "List<byte>")
					{
						string str = table.getCellString(i, j);
						if (str != null && str[0] == '[' && str[str.Length - 1] == ']')
						{
							str = str.Remove(0, 1);
							str = str.Remove(str.Length - 1, 1);
						}
						List<byte> bytes = new();
						stringToBytes(str, bytes);
						fileWriter.writeList(bytes);
					}
					else if (colType == "List<int>")
					{
						string str = table.getCellString(i, j);
						if (str != null && str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
						{
							str = str.Remove(0, 1);
							str = str.Remove(str.Length - 1, 1);
						}
						fileWriter.writeList(stringToInts(str));
					}
					else if (colType == "List<Vector2>")
					{
						string str = table.getCellString(i, j);
						if (str != null && str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
						{
							str = str.Remove(0, 1);
							str = str.Remove(str.Length - 1, 1);
						}
						fileWriter.writeList(stringToVector2s(str, member.mSeperate));
					}
					else if (colType == "List<Vector2Int>")
					{
						string str = table.getCellString(i, j);
						if (str != null && str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
						{
							str = str.Remove(0, 1);
							str = str.Remove(str.Length - 1, 1);
						}
						fileWriter.writeList(stringToVector2Ints(str, member.mSeperate));
					}
					else if (colType == "List<Vector3>")
					{
						string str = table.getCellString(i, j);
						if (str != null && str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
						{
							str = str.Remove(0, 1);
							str = str.Remove(str.Length - 1, 1);
						}
						fileWriter.writeList(stringToVector3s(str, member.mSeperate));
					}
					else if (colType == "List<Vector3Int>")
					{
						string str = table.getCellString(i, j);
						if (str != null && str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
						{
							str = str.Remove(0, 1);
							str = str.Remove(str.Length - 1, 1);
						}
						fileWriter.writeList(stringToVector3Ints(str, member.mSeperate));
					}
					else if (colType == "List<float>")
					{
						string str = table.getCellString(i, j);
						if (str != null && str[0] == '[' && str[str.Length - 1] == ']')
						{
							str = str.Remove(0, 1);
							str = str.Remove(str.Length - 1, 1);
						}
						fileWriter.writeList(stringToFloats(str));
					}
					else if (colType == "List<string>")
					{
						string str = (table.getCellString(i, j));
						if (member.mSeperate == "\n")
						{
							splitLine(str, out string[] lines);
							if (lines != null)
							{
								fileWriter.writeList(new List<string>(lines));
							}
							else
							{
								fileWriter.writeList(new List<string>());
							}
						}
						else
						{
							fileWriter.writeList(stringToStrings(str, member.mSeperate));
						}
					}
					else if (colType == "Vector2")
					{
						string str = table.getCellString(i, j);
						string[] splitList = split(str, true, ",");
						if (splitList == null || splitList.Length != 2)
						{
							Console.WriteLine("Vector2单元格的内容错误,字段名:" + member.mMemberName + ", 表格:" + tableName + ", ID:" + table.getCellInt(i, 0));
							return false;
						}
						fileWriter.write(SToVector2(str));
					}
					else if (colType == "Vector2Int")
					{
						string str = table.getCellString(i, j);
						string[] splitList = split(str, true, ",");
						if (splitList == null || splitList.Length != 2)
						{
							Console.WriteLine("Vector2Int单元格的内容错误,字段名:" + member.mMemberName + ", 表格:" + tableName + ", ID:" + table.getCellInt(i, 0));
							return false;
						}
						fileWriter.write(SToVector2Int(str));
					}
					else if (colType == "Vector3")
					{
						string str = table.getCellString(i, j);
						string[] splitList = split(str, true, ",");
						if (splitList == null || splitList.Length != 3)
						{
							Console.WriteLine("Vector3单元格的内容错误,字段名:" + member.mMemberName + ", 表格:" + tableName + ", ID:" + table.getCellInt(i, 0));
							return false;
						}
						fileWriter.write(SToVector3(str));
					}
					else if (colType == "Vector3Int")
					{
						string str = table.getCellString(i, j);
						string[] splitList = split(str, true, ",");
						if (splitList == null || splitList.Length != 3)
						{
							Console.WriteLine("Vector3Int单元格的内容错误,字段名:" + member.mMemberName + ", 表格:" + tableName + ", ID:" + table.getCellInt(i, 0));
							return false;
						}
						fileWriter.write(SToVector3Int(str));
					}
					else if (colType == "string")
					{
						fileWriter.writeString((table.getCellString(i, j)));
					}
					else
					{
						Console.WriteLine("类型错误:" + colType);
						return false;
					}
				}
			}
			if (fileWriter.getBuffer() == null)
			{
				Console.WriteLine("表格数据为空:" + tableName);
				return false;
			}
			// 重新计算密钥
			string key = generateMD5("ASLD" + tableName) + "23y35y983";
			byte[] buffer = fileWriter.getBuffer();
			int dataSize = fileWriter.getDataSize();
			for (int i = 0; i < dataSize; ++i)
			{
				buffer[i] = (byte)((buffer[i] - ((i << 1) & 0xFF)) ^ key[i % key.Length]);
			}

			writeFile(path + tableName + ".bytes", buffer, dataSize);
			Console.WriteLine("已转换:" + tableName);
		}
		catch (Exception e)
		{
			Console.WriteLine("转换表格内容时报错,表格:" + tableName + ", info:" + e.Message);
			return false;
		}
		return true;
	}
	protected static bool convertTableToTxt(string path, ExcelInfo info)
	{
		ExcelTable table = info.mTable;
		List<MemberInfo> memberInfoList = info.mMemberList;
		string tableName = info.mName;
		try
		{
			int rowCount = table.getRowCount();
			int colCount = table.getColumnCount();
			// 生成二进制的data文件
			string fileContent = EMPTY;
			for (int i = ROW_HEADER; i < rowCount; ++i)
			{
				for (int j = 0; j < colCount; ++j)
				{
					MemberInfo member = memberInfoList[j];
					if (member.mOwner == OWNER.SERVER || member.mOwner == OWNER.NONE)
					{
						continue;
					}
					string colType = member.mMemberType;
					if (colType == "int")
					{
						fileContent += IToS(table.getCellInt(i, j));
					}
					else if (colType == "float")
					{
						fileContent += FToS(table.getCellFloat(i, j));
					}
					else if (colType == "bool")
					{
						fileContent += (table.getCellInt(i, j) > 0 ? "true" : "false");
					}
					else if (colType == "List<int>" ||
							 colType == "List<Vector2>" ||
							 colType == "List<Vector2Int>" ||
							 colType == "List<Vector3>" ||
							 colType == "List<Vector3Int>" ||
							 colType == "List<float>")
					{
						string str = table.getCellString(i, j);
						if (str != null && str[0] == '[' && str[str.Length - 1] == ']')
						{
							str = str.Remove(0, 1);
							str = str.Remove(str.Length - 1, 1);
						}
						fileContent += str;
					}
					else if (colType == "List<string>")
					{
						string str = table.getCellString(i, j);
						if (member.mSeperate == "\n")
						{
							splitLine(str, out string[] lines);
							if (lines != null)
							{
								fileContent += str;
							}
						}
						else
						{
							fileContent += str;
						}
					}
					else if (colType == "Vector2" || colType == "Vector2Int")
					{
						string str = table.getCellString(i, j);
						string[] splitList = split(str, true, ",");
						if (splitList == null || splitList.Length != 2)
						{
							Console.WriteLine(colType + "单元格的内容错误,字段名:" + member.mMemberName + ", 表格:" + tableName + ", ID:" + table.getCellInt(i, 0));
							return false;
						}
						fileContent += str;
					}
					else if (colType == "Vector3" || colType == "Vector3Int")
					{
						string str = table.getCellString(i, j);
						string[] splitList = split(str, true, ",");
						if (splitList == null || splitList.Length != 3)
						{
							Console.WriteLine(colType + "单元格的内容错误,字段名:" + member.mMemberName + ", 表格:" + tableName + ", ID:" + table.getCellInt(i, 0));
							return false;
						}
						fileContent += str;
					}
					else if (colType == "string")
					{
						fileContent += table.getCellString(i, j);
					}
					fileContent += "\t";
				}
				fileContent += "\n";
			}
			writeTxtFile(path + tableName + ".txt", fileContent);
			Console.WriteLine("已转换:" + tableName);
		}
		catch (Exception e)
		{
			Console.WriteLine("转换表格内容时报错,表格:" + tableName + ", info:" + e.Message);
			return false;
		}
		return true;
	}
	protected static void line(ref string line, string text, bool returnLine = true)
	{
		if (returnLine)
		{
			line += text + "\r\n";
		}
		else
		{
			line += text;
		}
	}
}