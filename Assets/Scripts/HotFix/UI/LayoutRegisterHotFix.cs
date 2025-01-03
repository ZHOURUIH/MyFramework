using static GBH;
using static LayoutManager;

public class LayoutRegisterHotFix
{
	public static void registeAll()
	{
		registeLayout<UILogin>((script) => { mUILogin = script; });
		registeLayout<UIGaming>((script) => { mUIGaming = script; });
	}
}