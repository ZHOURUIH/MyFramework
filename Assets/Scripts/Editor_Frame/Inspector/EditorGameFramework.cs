using UnityEditor;

[CustomEditor(typeof(GameEntryBase), true)]
public class EditorGameFramework : GameInspector
{
	protected GameEntryBase mGameEntry;
	protected override void onGUI()
	{
		mGameEntry = target as GameEntryBase;

		bool modified = false;
		modified |= displayEnum("WindowMode", "窗口类型", ref mGameEntry.mFrameworkParam.mWindowMode);
		if (mGameEntry.mFrameworkParam.mWindowMode != WINDOW_MODE.FULL_SCREEN)
		{
			modified |= displayInt("ScreenWidth", "窗口宽度,当WindowMode为FULL_SCREEN时无效", ref mGameEntry.mFrameworkParam.mScreenWidth);
			modified |= displayInt("ScreenHeight", "窗口高度,当WindowMode为FULL_SCREEN时无效", ref mGameEntry.mFrameworkParam.mScreenHeight);
		}
		modified |= displayInt("TargetFrameRate", "窗口高度,当WindowMode为FULL_SCREEN时无效", ref mGameEntry.mFrameworkParam.mDefaultFrameRate);
		modified |= toggle("PoolStackTrace", "是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,可使用F4动态开启", ref mGameEntry.mFrameworkParam.mEnablePoolStackTrace);
		modified |= toggle("ScriptDebug", "是否启用调试脚本,也就是挂接在GameObject上用于显示调试信息的脚本,可使用F3动态开启", ref mGameEntry.mFrameworkParam.mEnableScriptDebug);
		modified |= displayEnum("LoadSource", "加载源", ref mGameEntry.mFrameworkParam.mLoadSource);

		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}