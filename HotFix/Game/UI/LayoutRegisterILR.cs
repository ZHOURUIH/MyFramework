using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LayoutRegisterILR : LayoutRegisterBaseILR
{
	public static void registeAll()
	{
		registeLayout<ScriptLogin>(LAYOUT_ILR.LOGIN, "UILogin");
		registeLayout<ScriptGaming>(LAYOUT_ILR.GAMING, "UIGaming");

		mLayoutManager.addScriptCallback(onScriptChanged);
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
}