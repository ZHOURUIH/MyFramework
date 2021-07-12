#if USE_ILRUNTIME
using UnityEngine;
using System;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using HotFix;

// 用于绑定所有跨域继承的适配器
public static class CrossAdapterRegister
{
	public static void registeCrossAdaptor(ILRAppDomain appDomain)
	{
		appDomain.RegisterCrossBindingAdaptor(new FrameBaseAdapter());
		appDomain.RegisterCrossBindingAdaptor(new ClassObjectAdapter());
		appDomain.RegisterCrossBindingAdaptor(new GameSceneAdapter());
		appDomain.RegisterCrossBindingAdaptor(new LayoutScriptAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SceneProcedureAdapter());
		appDomain.RegisterCrossBindingAdaptor(new CharacterAdapter());
		appDomain.RegisterCrossBindingAdaptor(new CharacterDataAdapter());
		appDomain.RegisterCrossBindingAdaptor(new GameComponentAdapter());
		appDomain.RegisterCrossBindingAdaptor(new StateParamAdapter());
		appDomain.RegisterCrossBindingAdaptor(new CharacterStateAdapter());
		appDomain.RegisterCrossBindingAdaptor(new StateGroupAdapter());
		appDomain.RegisterCrossBindingAdaptor(new CommandAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SQLiteTableAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SQLiteDataAdapter());
		appDomain.RegisterCrossBindingAdaptor(new PooledWindowAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SceneInstanceAdapter());
		appDomain.RegisterCrossBindingAdaptor(new FrameSystemAdapter());
		appDomain.RegisterCrossBindingAdaptor(new TransformableAdapter());
		appDomain.RegisterCrossBindingAdaptor(new GameBaseAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SocketPacketAdapter());
		appDomain.RegisterCrossBindingAdaptor(new GameEventAdapter());
		appDomain.RegisterCrossBindingAdaptor(new WindowItemAdapter());
		appDomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
		appDomain.RegisterCrossBindingAdaptor(new SocketConnectClientAdapter());
		appDomain.RegisterCrossBindingAdaptor(new DelayCmdWatcherAdapter());
		appDomain.RegisterCrossBindingAdaptor(new GameDefineAdapter());
	}
}
#endif
