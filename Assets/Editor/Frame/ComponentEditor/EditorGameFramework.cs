using UnityEditor;

[CustomEditor(typeof(GameFramework), true)]
public class EditorGameFramework : GameEditorBase
{
	protected GameFramework mGameFramework;
	public override void OnInspectorGUI()
	{
		mGameFramework = target as GameFramework;

		bool modified = false;
		modified |= displayEnum("WindowMode", "窗口类型", ref mGameFramework.mWindowMode);
		if (mGameFramework.mWindowMode != WINDOW_MODE.FULL_SCREEN)
		{
			modified |= displayInt("ScreenWidth", "窗口宽度,当WindowMode为FULL_SCREEN时无效", ref mGameFramework.mScreenWidth);
			modified |= displayInt("ScreenHeight", "窗口高度,当WindowMode为FULL_SCREEN时无效", ref mGameFramework.mScreenHeight);
		}
		modified |= toggle("PoolStackTrace", "是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,可使用F4动态开启", ref mGameFramework.mEnablePoolStackTrace);
		modified |= toggle("ScriptDebug", "是否启用调试脚本,也就是挂接在GameObject上用于显示调试信息的脚本,可使用F3动态开启", ref mGameFramework.mEnableScriptDebug);
		modified |= toggle("UseFixedTime", "是否将每帧的时间固定下来", ref mGameFramework.mUseFixedTime);
		modified |= toggle("ForceTop", "窗口是否始终显示在顶层", ref mGameFramework.mForceTop);
		modified |= displayEnum("LoadSource", "加载源", ref mGameFramework.mLoadSource);
		modified |= displayEnum("LogLevel", "日志等级", ref mGameFramework.mLogLevel);

		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}