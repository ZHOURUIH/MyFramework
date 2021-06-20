using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public abstract class ConfigBase : FrameSystem
{
	protected Dictionary<string, int> mFloatNameToDefine;
	protected Dictionary<int, string> mFloatDefineToName;
	protected Dictionary<string, int> mStringNameToDefine;
	protected Dictionary<int, string> mStringDefineToName;
	protected Dictionary<int, FloatParameter> mFloatList;
	protected Dictionary<int, StringParameter> mStringList;
	public ConfigBase()
	{
		mFloatNameToDefine = new Dictionary<string, int>();
		mFloatDefineToName = new Dictionary<int, string>();
		mStringNameToDefine = new Dictionary<string, int>();
		mStringDefineToName = new Dictionary<int, string>();
		mFloatList = new Dictionary<int, FloatParameter>();
		mStringList = new Dictionary<int, StringParameter>();
	}
	public override void init()
	{
		base.init();
		addFloat();
		addString();
		readConfig();
	}
	public abstract void writeConfig();
	public float getFloat(int param)
	{
		if(mFloatList.TryGetValue(param, out FloatParameter floatParam))
		{
			return floatParam.mValue;
		}
		return 0.0f;
	}
	public string getString(int param)
	{
		if(mStringList.TryGetValue(param, out StringParameter strinParam))
		{
			return strinParam.mValue;
		}
		return EMPTY;
	}
	public void setFloat(int param, float value, string comment = null)
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
	public void setString(int param, string value, string comment = null)
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
	protected void addFloat(string paramName, int type)
	{
		mFloatNameToDefine.Add(paramName, type);
		mFloatDefineToName.Add(type, paramName);
	}
	protected void addString(string paramName, int type)
	{
		mStringNameToDefine.Add(paramName, type);
		mStringDefineToName.Add(type, paramName);
	}
	protected string floatTypeToName(int type)
	{
		mFloatDefineToName.TryGetValue(type, out string value);
		return value;
	}
	protected int floatNameToType(string name)
	{
		if (mFloatNameToDefine.TryGetValue(name, out int type))
		{
			return type;
		}
		return 0;
	}
	protected string stringTypeToName(int type)
	{
		mStringDefineToName.TryGetValue(type, out string str);
		return str;
	}
	protected int stringNameToType(string name)
	{
		if (mStringNameToDefine.TryGetValue(name, out int type))
		{
			return type;
		}
		return 0;
	}
	protected bool hasFloatParam(int param) { return mFloatList.ContainsKey(param); }
	protected bool hasStringParam(int param) { return mStringList.ContainsKey(param); }
	protected string generateFloatFile()
	{
		MyStringBuilder fileString = STRING("// 注意\r\n// 每个参数上一行必须是该参数的注释\r\n// 可以添加任意的换行和空格\r\n// 变量命名应与代码中枚举命名相同\r\n\r\n");
		string nextLineStr = "\r\n\r\n";
		foreach (var info in mFloatList)
		{
			fileString.Append("// ", info.Value.mComment, "\r\n");
			fileString.Append(info.Value.mTypeString, " = ", FToS(info.Value.mValue, 2), nextLineStr);
		}
		// 移除最后的\r\n\r\n
		return END_STRING(fileString.Remove(fileString.Length - nextLineStr.Length));
	}
	protected string generateStringFile()
	{
		MyStringBuilder fileString = STRING("// 注意\r\n// 每个参数上一行必须是该参数的注释\r\n// 可以添加任意的换行和空格\r\n// 变量命名应与代码中枚举命名相同\r\n\r\n");
		string nextLineStr = "\r\n\r\n";
		foreach (var info in mStringList)
		{
			fileString.Append("// ", info.Value.mComment, "\r\n");
			fileString.Append(info.Value.mTypeString, " = ", info.Value.mValue, nextLineStr);
		}
		// 移除最后的\r\n\r\n
		return END_STRING(fileString.Remove(fileString.Length - nextLineStr.Length));
	}
	// fileName为StreamAssets/Config目录下的相对路径
	protected void readFile(string fileName, bool floatParam)
	{
		// 资源目录为本地目录,直接读取本地文件,否则使用http同步下载文件
		string text;
		if (mResourceManager.isLocalRootPath())
		{
			text = openTxtFile(fileName, false);
		}
		else
		{
			text = bytesToString(HttpUtility.downloadFile(fileName), Encoding.UTF8);
		}
		string[] lineList = split(text, true, "\n");
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
				int def = floatNameToType(item.Key);
				if (def != 0)
				{
					setFloat(def, SToF(item.Value.mValue), item.Value.mComment);
				}
			}
			else
			{
				int def = stringNameToType(item.Key);
				if (def != 0)
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