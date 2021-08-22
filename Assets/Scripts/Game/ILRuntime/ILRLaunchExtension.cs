#if USE_ILRUNTIME
using System;
using System.Collections.Generic;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

// 此类只是给ILRLaunchFrame调用的,其他地方不需要调用
public class ILRLaunchExtension
{
	public static void registeValueTypeBinder(ILRAppDomain appDomain){}
	public static void collectCrossInheritClass(HashSet<Type> classList)
	{
		// 收集所有需要生成适配器的类
		classList.Add(typeof(GameDefine));
	}
	public static void registeAllDelegate(ILRAppDomain appDomain)
	{
		;
	}
}
#endif