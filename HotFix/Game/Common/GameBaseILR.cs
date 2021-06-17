using UnityEngine;
using System;
using System.Collections.Generic;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 因为使用很频繁所以简写为GB,全称为GameBaseILR
public partial class GB : FrameUtilityILR
{
	// FrameSystem
	public static GameConfig mGameConfig;
	public static DemoSystem mDemoSystem;
	// LayoutScript
	public static ScriptLogin mScriptLogin;
	public static ScriptGaming mScriptGaming;
	public override void notifyConstructDone()
	{
		base.notifyConstructDone();
		getILRSystem(out mGameConfig);
		getILRSystem(out mDemoSystem);
	}
	public static T PACKET_ILR<T>(out T packet) where T : SocketPacket
	{
		return packet = mSocketFactory.createSocketPacket(typeof(T)) as T;
	}
	//------------------------------------------------------------------------------------------------------------------------
	public void getILRSystem<T>(out T system) where T : FrameSystem
	{
		system = mGameFramework.getSystem(typeof(T)) as T;
	}
}