using System;
using System.Collections.Generic;
using System.Reflection;

// 根据一个类,生成重写了这个类的所有虚函数的新的类
public class ClassTypeGenerator
{
	protected List<string> mNameSpaceList;
	public ClassTypeGenerator()
	{
		mNameSpaceList = new List<string>();
	}
	public void generateILRClass(Type type, string path)
	{
		if (!type.IsClass)
		{
			return;
		}
		bool isAbstract = false;
		string allMethodString = "";
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (var item in methods)
		{
			if (item.IsAbstract)
			{
				isAbstract = true;
			}
			if (!item.IsVirtual || item.IsAbstract || item.Name == "Finalize")
			{
				continue;
			}
			allMethodString += "\t" + generateMethodString(item) + "\r\n";
		}

		string classBody = "";
		classBody += "using System;\r\n";
		classBody += "using System.Collections;\r\n";
		classBody += "using System.Collections.Generic;\r\n";
		foreach(var item in mNameSpaceList)
		{
			classBody += "using " + item + ";\r\n";
		}
		classBody += "\t\n";
		classBody += "public ";
		if(isAbstract)
		{
			classBody += "abstract ";
		}
		classBody += "class ";
		classBody += "ILR" + type.Name + " : " + type.Name + "\r\n";
		classBody += "{\t\n";
		classBody += allMethodString;
		classBody += "}\t\n";
		FileUtility.writeTxtFile(path + "ILR" + type.Name + ".cs", classBody);
	}
	//--------------------------------------------------------------------------------------------
	protected void collectNamespace(Type type)
	{
		// 这三个是默认包含的
		if (StringUtility.isEmpty(type.Namespace) || 
			type.Namespace == "System" ||
			type.Namespace == "System..Collections" ||
			type.Namespace == "System..Collections.Generic")
		{
			return;
		}
		if (mNameSpaceList.Contains(type.Namespace))
		{
			return;
		}
		mNameSpaceList.Add(type.Namespace);
	}
	protected string getPublixPrivatePrefix(MethodInfo info)
	{
		if (info.IsPublic)
		{
			return "public ";
		}
		else if (info.IsPrivate)
		{
			return "private ";
		}
		// 不是公有也不是私有,则是保护权限
		return "protected ";
	}
	protected string getInOutPrefix(ParameterInfo param)
	{
		if (param.IsOut)
		{
			return "out ";
		}
		if (param.IsIn)
		{
			return "in ";
		}
		if (param.ParameterType.ToString().Contains("&"))
		{
			return "ref ";
		}
		return "";
	}
	protected string getDefaultValueString(Type type, object defaultValue)
	{
		string defaultString = "";
		if (type == typeof(bool))
		{
			defaultString += (bool)defaultValue ? "true" : "false";
		}
		else if (type == typeof(sbyte))
		{
			defaultString += (sbyte)defaultValue;
		}
		else if (type == typeof(byte))
		{
			defaultString += (byte)defaultValue;
		}
		else if (type == typeof(ushort))
		{
			defaultString += (ushort)defaultValue;
		}
		else if (type == typeof(short))
		{
			defaultString += (short)defaultValue;
		}
		else if (type == typeof(uint))
		{
			defaultString += (uint)defaultValue;
		}
		else if (type == typeof(int))
		{
			defaultString += (int)defaultValue;
		}
		else if (type == typeof(float))
		{
			defaultString += (float)defaultValue + "f";
		}
		else if (type == typeof(double))
		{
			defaultString += (double)defaultValue;
		}
		else if (type == typeof(long))
		{
			defaultString += (long)defaultValue;
		}
		else if (type == typeof(ulong))
		{
			defaultString += (ulong)defaultValue;
		}
		else if (type == typeof(string))
		{
			defaultString += (string)defaultValue;
		}
		else if (type.IsEnum)
		{
			defaultString += getTypeString(type) + "." + type.GetEnumName(defaultValue);
		}
		else if (type.IsClass)
		{
			if (defaultValue != null)
			{
				UnityUtility.logError("参数类型为class,但是默认值不为空");
			}
			defaultString += " = null";
		}
		else
		{
			UnityUtility.logError("无法识别默认参数类型:" + type);
		}
		return defaultString;
	}
	protected string getTypeString(Type type)
	{
		if (TypeUtility.getTypeName(type) != null)
		{
			return TypeUtility.getTypeName(type);
		}
		string typeString = type.ToString();
		// 去除引用符号
		typeString = typeString.Replace("&", "");
		// 因为bool和ref bool在typeof后的值不一致,所以所有的基础类型都要判断
		if (TypeUtility.getTypeName(typeString) != null)
		{
			return TypeUtility.getTypeName(typeString);
		}
		// 有命名空间的去掉命名空间
		if (typeString.Contains("."))
		{
			return typeString.Substring(typeString.LastIndexOf('.') + 1);
		}
		// 类,基础数据类型以外的值类型,接口,则直接返回类型字符串
		if (type.IsClass || type.IsValueType || type.IsInterface)
		{
			return typeString;
		}
		UnityUtility.logError("未知的类型,无法获取类型字符串:" + type);
		return "";
	}
	protected string generateMethodString(MethodInfo method)
	{
		string methodString = "";
		collectNamespace(method.ReturnType);
		methodString += getPublixPrivatePrefix(method) + "override " + getTypeString(method.ReturnType) + " ";
		// 函数形式参数列表的字符串
		string paramString = "";
		// 函数体内调用基类函数时的实际参数列表的字符串
		string callParamString = "";
		ParameterInfo[] paramInfos = method.GetParameters();
		foreach (var param in paramInfos)
		{
			paramString += getInOutPrefix(param);
			callParamString += getInOutPrefix(param);
			callParamString += param.Name;
			collectNamespace(param.ParameterType);
			paramString += getTypeString(param.ParameterType) + " ";
			paramString += param.Name;
			if (param.HasDefaultValue)
			{
				paramString += " = " + getDefaultValueString(param.ParameterType, param.DefaultValue);
			}
			if(param != paramInfos[paramInfos.Length - 1])
			{
				paramString += ", ";
				callParamString += ", ";
			}
		}
		methodString += method.Name + "(" + paramString + ")";
		string methodBody = "";
		if (method.ReturnType != typeof(void))
		{
			methodBody += "return ";
		}
		methodBody += "base." + method.Name + "(" + callParamString + ");";
		methodString += " { " + methodBody + " }";
		return methodString;
	}
}
