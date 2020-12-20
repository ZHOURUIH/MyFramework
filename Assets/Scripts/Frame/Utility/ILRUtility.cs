#if USE_ILRUNTIME
using ILRuntime.CLR.TypeSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

// 用于访问ILR工程
public class ILRUtility : FrameBase
{
	// 调用静态函数,带返回值的
	public static T callStatic<T>(string type, string method, params object[] p)
	{
		if (mILRSystem.getAppDomain() == null)
		{
			return default;
		}
		return (T)mILRSystem.getAppDomain().Invoke(type, method, null, p);
	}
	// 调用静态函数,不带返回值
	public static void callStatic(string type, string method, params object[] p)
	{
		if (mILRSystem.getAppDomain() == null)
		{
			return;
		}
		mILRSystem.getAppDomain().Invoke(type, method, null, p);
	}
	// 调用成员函数,带返回值
	public static T callMethod<T>(string type, string method, object instance, params object[] p)
	{
		if (mILRSystem.getAppDomain() == null)
		{
			return default;
		}
		return (T)mILRSystem.getAppDomain().Invoke(type, method, instance, p);
	}
	// 调用成员函数,不带返回值
	public static void callMethod(string type, string method, object instance, params object[] p)
	{
		if (mILRSystem.getAppDomain() == null)
		{
			return;
		}
		mILRSystem.getAppDomain().Invoke(type, method, instance, p);
	}
	// 根据名字获取热更工程中的类型
	public static Type getILRType(string name)
	{
		ILRAppDomain appDomain = mILRSystem.getAppDomain();
		if (appDomain == null)
		{
			return null;
		}
		if (!appDomain.LoadedTypes.TryGetValue(name, out IType type))
		{
			return null;
		}
		return type.ReflectionType;
	}
}
#endif