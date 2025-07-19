using static GBH;
using static LayoutManager;

public class LayoutRegisterHotFix
{
	public static void registeAll()
	{
		// 需要添加auto generate start和auto generate end才会自动生成代码
		// auto generate start
		registeLayout<UIGaming>((script) =>							{ mUIGaming = script; });
		registeLayout<UILogin>((script) =>							{ mUILogin = script; });
		// auto generate end
	}
}