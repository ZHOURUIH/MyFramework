using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LayoutRegisterILR : GB
{
	public static void registeAll()
	{
		registeLayout<ScriptLogin>(LAYOUT_ILR.LOGIN, "UILogin");
		registeLayout<ScriptGaming>(LAYOUT_ILR.GAMING, "UIGaming");

		GameLayout.addScriptCallback(onScriptChanged);
	}
	public static void onScriptChanged(LayoutScript script, bool created = true)
	{
		// 只有布局与脚本唯一对应的才能使用变量快速访问
		if (mLayoutManager.getScriptMappingCount(script.getType()) > 1)
		{
			return;
		}
		if (assign(ref mScriptLogin, script, created)) return;
		if (assign(ref mScriptGaming, script, created)) return;
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------------------------
	protected static void registeLayout<T>(int layout, string name) where T : LayoutScript
	{
		registeLayout<T>(layout, name, EMPTY);
	}
	protected static void registeLayout<T>(int layout, string name, string prePath) where T : LayoutScript
	{
		mLayoutManager.registeLayout(typeof(T), layout, prePath + name + "/" + name, false);
	}
	protected static bool assign<T>(ref T thisScript, LayoutScript value, bool created) where T : LayoutScript
	{
		if (typeof(T) == value.GetType())
		{
			thisScript = created ? value as T : null;
			return true;
		}
		return false;
	}
}