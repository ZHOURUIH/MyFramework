#if USE_ILRUNTIME
using ILRuntime.CLR.TypeSystem;
using System;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using static FrameBase;
using static FrameDefine;

// 用于访问ILR工程
public class ILRFrameUtility
{
	public static void socketState(NET_STATE state)
	{
		callStatic("socketState", state);
	}
	public static void start()
	{
		callStatic("start");
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 调用热更工程中ILRExport类的静态函数,带返回值的
	protected static T callStatic<T>(string method, params object[] p)
	{
		if (mILRSystem.getAppDomain() == null)
		{
			return default;
		}
		return (T)mILRSystem.getAppDomain().Invoke(ILR_EXPORT, method, null, p);
	}
	// 调用热更工程中ILRExport类的静态函数,不带返回值
	protected static void callStatic(string method, params object[] p)
	{
		if (mILRSystem.getAppDomain() == null)
		{
			return;
		}
		mILRSystem.getAppDomain().Invoke(ILR_EXPORT, method, null, p);
	}
	// 调用成员函数,带返回值
	protected static T callMethod<T>(string type, string method, object instance, params object[] p)
	{
		if (mILRSystem.getAppDomain() == null)
		{
			return default;
		}
		return (T)mILRSystem.getAppDomain().Invoke(type, method, instance, p);
	}
	// 调用成员函数,不带返回值
	protected static void callMethod(string type, string method, object instance, params object[] p)
	{
		if (mILRSystem.getAppDomain() == null)
		{
			return;
		}
		mILRSystem.getAppDomain().Invoke(type, method, instance, p);
	}
	// 根据名字获取热更工程中的类型
	protected static Type getILRType(string name)
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