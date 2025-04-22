using System;
using static GameBase;
using static FrameBase;

public class LayoutRegister
{
	public static void registeAllLayout()
	{
		registeLayout<UIDemo>((script) => { mUIDemo = script; });
	}
	protected static void registeLayout<T>(Action<T> callback) where T : GameLayout
	{
		mLayoutManager.registeLayout(typeof(T), typeof(T).ToString(), (script) => { callback?.Invoke(script as T); });
	}
}