
// 用于以安全的方式去调用一个布局的函数,在布局未加载或者没有显示时不会执行布局函数
public static class LayoutScriptExtension
{
	public static T safe<T>(this T script) where T : LayoutScript
	{
		if (script == null || !script.isVisible())
		{
			return null;
		}
		return script;
	}
}