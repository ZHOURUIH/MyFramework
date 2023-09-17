#if USE_ILRUNTIME
using System;
using System.Collections.Generic;
using ILRuntime.Runtime.Generated;
using UnityEngine;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

// 此类只是给ILRLaunchFrame调用的,其他地方不需要调用
public class ILRLaunchExtension : ILRLaunchFrame
{
	public override void valueTypeBind(ILRAppDomain appDomain)
	{
		// 值类型绑定
		appDomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
		appDomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
		appDomain.RegisterValueTypeBinder(typeof(Vector2Int), new Vector2IntBinder());
		appDomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
		appDomain.RegisterValueTypeBinder(typeof(Vector3Int), new Vector3IntBinder());
	}
	public override void collectCrossInheritClass(HashSet<Type> classList)
	{
		base.collectCrossInheritClass(classList);
		// 收集所有需要生成适配器的类
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void crossAdapter(ILRAppDomain appDomain)
	{
		// 跨域继承适配器
		CrossAdapterRegister.registeCrossAdaptor(appDomain);
	}
	protected override void clrBind(ILRAppDomain appDomain)
	{
		CLRBindings.Initialize(appDomain);
	}
	protected override void registeAllDelegate(ILRAppDomain appDomain)
	{
		base.registeAllDelegate(appDomain);
	}
	}
#endif