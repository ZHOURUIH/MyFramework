using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;

public struct FloatParameter
{
	public float mValue;
	public string mComment;
	public GAME_FLOAT mType;
	public string mTypeString;
}

public struct StringParameter
{
	public string mValue;
	public string mComment;
	public GAME_STRING mType;
	public string mTypeString;
}

public struct ConfigInfo
{
	public string mComment;
	public string mName;
	public string mValue;
}

public abstract class ConfigBase : FrameSystem
{
	protected Dictionary<string, GAME_FLOAT> mFloatNameToDefine;
	protected Dictionary<GAME_FLOAT, string> mFloatDefineToName;
	protected Dictionary<string, GAME_STRING> mStringNameToDefine;
	protected Dictionary<GAME_STRING, string> mStringDefineToName;
	protected Dictionary<GAME_FLOAT, FloatParameter> mFloatList;
	protected Dictionary<GAME_STRING, StringParameter> mStringList;
	public ConfigBase()
	{
		mFloatNameToDefine = new Dictionary<string, GAME_FLOAT>();
		mFloatDefineToName = new Dictionary<GAME_FLOAT, string>();
		mStringNameToDefine = new Dictionary<string, GAME_STRING>();
		mStringDefineToName = new Dictionary<GAME_STRING, string>();
		mFloatList = new Dictionary<GAME_FLOAT, FloatParameter>();
		mStringList = new Dictionary<GAME_STRING, StringParameter>();
	}
	public override void init()
	{
		base.init();
		addFloat();
		addString();
		readConfig();
	}
	public abstract void writeConfig();
	public float getFloat(GAME_FLOAT param)
	{
		if(mFloatList.TryGetValue(param, out FloatParameter floatParam))
		{
			return floatParam.mValue;
		}
		return 0.0f;
	}
	public string getString(GAME_STRING param)
	{
		if(mStringList.TryGetValue(param, out StringParameter strinParam))
		{
			return strinParam.mValue;
		}
		return EMPTY;
	}
	public void setFloat(GAME_FLOAT param, float value, string comment = null)
	{
		if (!mFloatList.TryGetValue(param, out FloatParameter floatParam))
		{
			floatParam = new FloatParameter();
			floatParam.mValue = value;
			floatParam.mComment = comment;
			floatParam.mType = param;
			floatParam.mTypeString = param.ToString();
			mFloatList.Add(param, floatParam);
		}
		else
		{
			floatParam.mValue = value;
			if (!isEmpty(comment))
			{
				floatParam.mComment = comment;
			}
			mFloatList[param] = floatParam;
		}
	}
	public void setString(GAME_STRING param, string value, string comment = null)
	{
		if (!mStringList.TryGetValue(param, out StringParameter stringParam))
		{
			stringParam = new StringParameter();
			stringParam.mValue = value;
			stringParam.mComment = comment;
			stringParam.mType = param;
			stringParam.mTypeString = param.ToString();
			mStringList.Add(param, stringParam);
		}
		else
		{
			stringParam.mValue = value;
			if (!isEmpty(comment))
			{
				stringParam.mComment = comment;
			}
			mStringList[param] = stringParam;
		}
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addFloat(GAME_FLOAT type)
	{
		mFloatNameToDefine.Add(type.ToString(), type);
		mFloatDefineToName.Add(type, type.ToString());
	}
	protected void addString(GAME_STRING type)
	{
		mStringNameToDefine.Add(type.ToString(), type);
		mStringDefineToName.Add(type, type.ToString());
	}
	protected string floatTypeToName(GAME_FLOAT type)
	{
		mFloatDefineToName.TryGetValue(type, out string value);
		return value;
	}
	protected GAME_FLOAT floatNameToType(string name)
	{
		if (mFloatNameToDefine.TryGetValue(name, out GAME_FLOAT type))
		{
			return type;
		}
		return GAME_FLOAT.NONE;
	}
	protected string stringTypeToName(GAME_STRING type)
	{
		mStringDefineToName.TryGetValue(type, out string str);
		return str;
	}
	protected GAME_STRING stringNameToType(string name)
	{
		if (mStringNameToDefine.TryGetValue(name, out GAME_STRING type))
		{
			return type;
		}
		return GAME_STRING.NONE;
	}
	protected bool hasParameter(GAME_FLOAT param) { return mFloatList.ContainsKey(param); }
	protected bool hasParameter(GAME_STRING param) { return mStringList.ContainsKey(param); }
	protected string generateFloatFile()
	{
		string preString = "// 注意\r\n// 每个参数上一行必须是该参数的注释\r\n// 可以添加任意的换行和空格\r\n// 变量命名应与代码中枚举命名相同\r\n\r\n";
		string nextLineStr = "\r\n\r\n";
		string fileString = preString;
		foreach (var info in mFloatList)
		{
			fileString += "// ";
			fileString += info.Value.mComment;
			fileString += "\r\n";
			fileString += info.Value.mTypeString;
			fileString += " = ";
			fileString += floatToString(info.Value.mValue, 2);
			fileString += nextLineStr;
		}
		// 移除最后的\r\n\r\n
		fileString = fileString.Substring(0, fileString.Length - nextLineStr.Length);
		return fileString;
	}
	protected string generateStringFile()
	{
		string preString = "// 注意\r\n// 每个参数上一行必须是该参数的注释\r\n// 可以添加任意的换行和空格\r\n// 变量命名应与代码中枚举命名相同\r\n\r\n";
		string nextLineStr = "\r\n\r\n";
		string fileString = preString;
		foreach (var info in mStringList)
		{
			fileString += "// ";
			fileString += info.Value.mComment;
			fileString += "\r\n";
			fileString += info.Value.mTypeString;
			fileString += " = ";
			fileString += info.Value.mValue;
			fileString += nextLineStr;
		}
		// 移除最后的\r\n\r\n
		fileString = fileString.Substring(0, fileString.Length - nextLineStr.Length);
		return fileString;
	}
	// fileName为StreamAssets/Config目录下的相对路径
	protected void readFile(string fileName, bool floatParam)
	{
		// 资源目录为本地目录,直接读取本地文件,否则使用http同步下载文件
		string text;
		if (ResourceManager.mLocalRootPath)
		{
			text = openTxtFile(fileName, false);
		}
		else
		{
			text = bytesToString(HttpUtility.downloadFile(fileName), Encoding.UTF8);
		}
		string[] lineList = split(text, true, "\r\n");
		Dictionary<string, ConfigInfo> valueList = new Dictionary<string, ConfigInfo>();
		string comment = null;
		// 前4行需要被丢弃
		int dropLine = 4;
		for (int i = 0; i < lineList.Length; ++i)
		{
			if (i < dropLine)
			{
				continue;
			}
			string line = lineList[i];
			// 去除所有空白字符
			line = Regex.Replace(line, @"\s", EMPTY);
			// 如果该行是空的,或者是注释,则不进行处理
			if (!isEmpty(line))
			{
				if (line.Substring(0, 2) == "//")
				{
					comment = line.Substring(2, line.Length - 2);
				}
				else
				{
					string[] value = split(line, false, "=");
					if (value.Length != 2)
					{
						logError("配置文件错误 : line : " + line);
						return;
					}
					ConfigInfo info = new ConfigInfo();
					info.mComment = comment;
					info.mName = value[0];
					info.mValue = value[1];
					if (!valueList.ContainsKey(info.mName))
					{
						valueList.Add(info.mName, info);
					}
				}
			}
		}

		foreach (var item in valueList)
		{
			if (floatParam)
			{
				GAME_FLOAT def = floatNameToType(item.Key);
				if (def != GAME_FLOAT.NONE)
				{
					setFloat(def, stringToFloat(item.Value.mValue), item.Value.mComment);
				}
			}
			else
			{
				GAME_STRING def = stringNameToType(item.Key);
				if (def != GAME_STRING.NONE)
				{
					setString(def, item.Value.mValue, item.Value.mComment);
				}
			}
		}
	}
	protected abstract void addFloat();
	protected abstract void addString();
	protected abstract void readConfig();
}