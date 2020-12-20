using System;
using System.Collections.Generic;
using System.Reflection;

// 根据一个类,生成重写了这个类的所有虚函数的新的类
public class TypeUtility
{
	protected static Dictionary<Type, string> mBasicTypeToString;
	protected static Dictionary<string, Type> mBasicFullStringToType;
	public static Type getType(string typeName)
	{
		initTypes();
		if (mBasicFullStringToType.ContainsKey(typeName))
		{
			return mBasicFullStringToType[typeName];
		}
		return null;
	}
	public static string getTypeName(string fullTypeName)
	{
		initTypes();
		if (mBasicFullStringToType.TryGetValue(fullTypeName, out Type type))
		{
			return mBasicTypeToString[type];
		}
		return null;
	}
	public static string getTypeName(Type type)
	{
		initTypes();
		mBasicTypeToString.TryGetValue(type, out string name);
		return name;
	}
	//---------------------------------------------------------------------------------------------------
	protected static void initTypes()
	{
		if (mBasicTypeToString != null)
		{
			return;
		}
		mBasicTypeToString = new Dictionary<Type, string>();
		mBasicFullStringToType = new Dictionary<string, Type>();
		mBasicTypeToString.Add(typeof(void), "void");
		mBasicTypeToString.Add(typeof(bool), "bool");
		mBasicTypeToString.Add(typeof(sbyte), "sbyte");
		mBasicTypeToString.Add(typeof(byte), "byte");
		mBasicTypeToString.Add(typeof(ushort), "ushort");
		mBasicTypeToString.Add(typeof(short), "short");
		mBasicTypeToString.Add(typeof(uint), "uint");
		mBasicTypeToString.Add(typeof(int), "int");
		mBasicTypeToString.Add(typeof(float), "float");
		mBasicTypeToString.Add(typeof(double), "double");
		mBasicTypeToString.Add(typeof(string), "string");
		mBasicTypeToString.Add(typeof(object), "object");
		foreach(var item in mBasicTypeToString)
		{
			mBasicFullStringToType.Add(item.Key.ToString(), item.Key);
		}
	}
}
