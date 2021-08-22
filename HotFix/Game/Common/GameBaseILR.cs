using UnityEngine;
using System;
using System.Collections.Generic;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 因为使用很频繁所以简写为GB,全称为GameBaseILR
public partial class GB : FrameUtilityILR
{
	// FrameSystem
	public static DemoSystem mDemoSystem;
	// LayoutScript
	public static ScriptLogin mScriptLogin;
	public static ScriptGaming mScriptGaming;
	public static void constructILRDone()
	{
		getILRSystem(out mDemoSystem);
	}
	public static T PACKET_ILR<T>(out T packet) where T : NetPacketTCPFrame
	{
		return packet = mSocketFactory.createSocketPacket(typeof(T)) as T;
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected static void getILRSystem<T>(out T system) where T : FrameSystem
	{
		system = mGameFramework.getSystem(typeof(T)) as T;
	}
}