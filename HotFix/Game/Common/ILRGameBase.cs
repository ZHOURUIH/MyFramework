using UnityEngine;
using System.Collections;
using System;

// 这个父类的添加是方便代码的书写
// 为了尽量减少代码书写量,所以写成简写,表示ILRGameBase
// 一个类如果不需要继承主工程中的类,则尽量继承此类,方便访问其他模块
public class GB : ILRFrameBase
{
	// LayoutScript
	public static ScriptLogin mScriptLogin;
	public static ScriptGaming mScriptGaming;
	public static T newILRCmd<T>(out T cmd, bool show = true, bool delay = false) where T : Command
	{
		return cmd = mCommandSystem.newCmd(typeof(T), show, delay) as T;
	}
}
