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
	public GAME_DEFINE_FLOAT mType;
	public string mTypeString;
}

public struct StringParameter
{
	public string mValue;
	public string mComment;
	public GAME_DEFINE_STRING mType;
	public string mTypeString;
}

public struct ConfigInfo
{
	public string mComment;
	public string mName;
	public string mValue;
}

public abstract class ConfigBase : FrameComponent
{
	protected Dictionary<string, GAME_DEFINE_FLOAT> mFloatNameToDefine;
	protected Dictionary<GAME_DEFINE_FLOAT, string> mFloatDefineToName;
	protected Dictionary<string, GAME_DEFINE_STRING> mStringNameToDefine;
	protected Dictionary<GAME_DEFINE_STRING, string> mStringDefineToName;
	protected Dictionary<GAME_DEFINE_FLOAT, FloatParameter> mFloatList;
	protected Dictionary<GAME_DEFINE_STRING, StringParameter> mStringList;
	public ConfigBase(string name)
		:base(name) 
	{
		mFloatNameToDefine = new Dictionary<string, GAME_DEFINE_FLOAT>();
		mFloatDefineToName = new Dictionary<GAME_DEFINE_FLOAT, string>();
		mStringNameToDefine = new Dictionary<string, GAME_DEFINE_STRING>();
		mStringDefineToName = new Dictionary<GAME_DEFINE_STRING, string>();
		mFloatList = new Dictionary<GAME_DEFINE_FLOAT, FloatParameter>();
		mStringList = new Dictionary<GAME_DEFINE_STRING, StringParameter>();
	}
	public override void init()
	{
		base.init();
		addFloat();
		addString();
		readConfig();
	}
	public abstract void writeConfig();
	public override void destroy()
	{
		base.destroy();
	}
	public float getFloatParam(GAME_DEFINE_FLOAT param)
	{
		return mFloatList.ContainsKey(param) ? mFloatList[param].mValue : 0.0f;
	}
	public string getStringParam(GAME_DEFINE_STRING param)
	{
		return mStringList.ContainsKey(param) ? mStringList[param].mValue : EMPTY_STRING;
	}
	public void setFloatParam(GAME_DEFINE_FLOAT param, float value, string comment = EMPTY_STRING)
	{
		if (!mFloatList.ContainsKey(param))
		{
			FloatParameter floatParam = new FloatParameter();
			floatParam.mValue = value;
			floatParam.mComment = comment;
			floatParam.mType = param;
			floatParam.mTypeString = param.ToString();
			mFloatList.Add(param, floatParam);
		}
		else
		{
			FloatParameter temp = mFloatList[param];
			temp.mValue = value;
			if(comment.Length != 0)
			{
				temp.mComment = comment;
			}
			mFloatList[param] = temp;
		}
	}
	public void setStringParam(GAME_DEFINE_STRING param, string value, string comment = EMPTY_STRING)
	{
		if (!mStringList.ContainsKey(param))
		{
			StringParameter strParam = new StringParameter();
			strParam.mValue = value;
			strParam.mComment = comment;
			strParam.mType = param;
			strParam.mTypeString = param.ToString();
			mStringList.Add(param, strParam);
		}
		else
		{
			StringParameter temp = mStringList[param];
			temp.mValue = value;
			if (comment.Length != 0)
			{
				temp.mComment = comment;
			}
			mStringList[param] = temp;
		}
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addFloatParam(GAME_DEFINE_FLOAT type)
	{
		mFloatNameToDefine.Add(type.ToString(), type);
		mFloatDefineToName.Add(type, type.ToString());
	}
	protected void addStringParam(GAME_DEFINE_STRING type)
	{
		mStringNameToDefine.Add(type.ToString(), type);
		mStringDefineToName.Add(type, type.ToString());
	}
	protected string floatTypeToName(GAME_DEFINE_FLOAT type)
	{
		if(mFloatDefineToName.ContainsKey(type))
		{
			return mFloatDefineToName[type];
		}
		return EMPTY_STRING;
	}
	protected GAME_DEFINE_FLOAT floatNameToType(string name)
	{
		return mFloatNameToDefine.ContainsKey(name) ? mFloatNameToDefine[name] : GAME_DEFINE_FLOAT.GDF_NONE;
	}
	protected string stringTypeToName(GAME_DEFINE_STRING type)
	{
		return mStringDefineToName.ContainsKey(type) ? mStringDefineToName[type] : EMPTY_STRING;
	}
	protected GAME_DEFINE_STRING stringNameToType(string name)
	{
		return mStringNameToDefine.ContainsKey(name) ? mStringNameToDefine[name] : GAME_DEFINE_STRING.GDS_NONE;
	}
	protected bool hasParameter(GAME_DEFINE_FLOAT param)
	{
		return mFloatList.ContainsKey(param);
	}
	protected bool hasParameter(GAME_DEFINE_STRING param)
	{
		return mStringList.ContainsKey(param);
	}
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
		string text = EMPTY_STRING;
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
		string comment = EMPTY_STRING;
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
			line = Regex.Replace(line, @"\s", EMPTY_STRING);
			// 如果该行是空的,或者是注释,则不进行处理
			if (line.Length > 0)
			{
				if (line.Substring(0, 2) == "//")
				{
					comment = line.Substring(2, line.Length - 2);
				}
				else
				{
					string[] value = split(line, false, "=");
					if(value.Length != 2)
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
		
		foreach(var item in valueList)
		{
			if (floatParam)
			{
				GAME_DEFINE_FLOAT def = floatNameToType(item.Key);
				if (def != GAME_DEFINE_FLOAT.GDF_NONE)
				{
					setFloatParam(def, stringToFloat(item.Value.mValue), item.Value.mComment);
				}
			}
			else
			{
				GAME_DEFINE_STRING def = stringNameToType(item.Key);
				if (def != GAME_DEFINE_STRING.GDS_NONE)
				{
					setStringParam(def, item.Value.mValue, item.Value.mComment);
				}
			}
		}
	}
	protected abstract void addFloat();
	protected abstract void addString();
	protected abstract void readConfig();
}