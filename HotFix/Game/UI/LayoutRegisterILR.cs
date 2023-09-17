using System;
using static GBR;
using static FrameBase;

public class LayoutRegisterILR : LayoutRegisterBaseILR
{
	public static void registeAll()
	{
		registeLayout<UILogin>(LAYOUT_ILR.LOGIN, "UILogin", "UILogin");
		registeLayout<UIGaming>(LAYOUT_ILR.GAMING, "UIGaming", "UIGaming");

		mLayoutManager.addScriptCallback(onScriptChanged);
	}
	public static void onScriptChanged(LayoutScript script, bool created = true)
	{
		// 只有布局与脚本唯一对应的才能使用变量快速访问
		if (mLayoutManager.getScriptMappingCount(script.getType()) > 1)
		{
			return;
		}
		if (assign(ref mUILogin, script, created)) return;
		if (assign(ref mUIGaming, script, created)) return;
	}
}