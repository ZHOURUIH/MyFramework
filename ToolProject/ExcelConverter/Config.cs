using System;
using static FileUtility;
using static StringUtility;

public class Config
{
	public static string mClientProjectPath;
	public static string mExcelPath;
	public static string mExcelDataHotFixPath;
	public static string mExcelTableHotFixPath;
	public static string mExcelBytesPath;
	public static string mHotFixCommonPath;
	public static string mFileSuffix;
	public static bool parse(string configPath)
	{
		openTxtFileLines(configPath, out string[] lines);
		if(lines == null)
		{
			Console.WriteLine("找不到配置文件ExcelConverterConfig.txt,或者配置文件内容为空");
			return false;
		}
		for(int i = 0; i < lines.Length; ++i)
		{
			if (lines[i].StartsWith("//"))
			{
				continue;
			}
			string[] param = split(lines[i], true, "=");
			if (param.Length != 2)
			{
				Console.WriteLine("配置文件读取错误:" + lines[i]);
				return false;
			}
			string paramName = param[0];
			string paramValue = param[1];
			if (paramName == "ProjectPath")
			{
				mClientProjectPath = paramValue;
				validPath(ref mClientProjectPath);
				rightToLeft(ref mClientProjectPath);
			}
			else if (paramName == "ExcelPath")
			{
				mExcelPath = paramValue;
				validPath(ref mExcelPath);
				rightToLeft(ref mExcelPath);
			}
			else if (paramName == "FileSuffix")
			{
				mFileSuffix = paramValue;
			}
		}
		if (isEmpty(mClientProjectPath))
		{
			Console.WriteLine("路径解析错误,找不到ProjectPath的配置");
			return false;
		}
		if (isEmpty(mExcelPath))
		{
			Console.WriteLine("路径解析错误,找不到ExcelPath的配置");
			return false;
		}
		if (isEmpty(mFileSuffix))
		{
			Console.WriteLine("需要指定文件后缀");
			return false;
		}
		string hotfixGamePath = mClientProjectPath + "Assets/Scripts/HotFix/";
		mExcelDataHotFixPath = hotfixGamePath + "DataBase/Excel/Data/";
		mExcelTableHotFixPath = hotfixGamePath + "DataBase/Excel/Table/";
		mExcelBytesPath = mClientProjectPath + "Assets/GameResources/Excel/";
		mHotFixCommonPath = hotfixGamePath + "Common/";
		return true;
	}
}