using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LayoutRegister : LayoutRegisterBase
{
	public static void registeAllLayout()
	{
		registeLayout<ScriptDemoStart>(LAYOUT.DEMO_START, "UIDemoStart");
		registeLayout<ScriptDemo>(LAYOUT.DEMO, "UIDemo");
		mLayoutManager.addScriptCallback(onScriptChanged);
	}
	public static void onScriptChanged(LayoutScript script, bool created = true)
	{
		// 只有布局与脚本唯一对应的才能使用变量快速访问
		if (mLayoutManager.getScriptMappingCount(script.getType()) > 1)
		{
			return;
		}
		if (assign(ref mScriptDemo, script, created)) return;
		if (assign(ref mScriptDemoStart, script, created)) return;
	}
}