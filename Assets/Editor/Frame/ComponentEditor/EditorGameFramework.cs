using UnityEditor;

[CustomEditor(typeof(GameFrameworkHotFix), true)]
public class EditorGameFramework : GameEditorBase
{
	protected GameFrameworkHotFix mGameFramework;
	public override void OnInspectorGUI()
	{
		mGameFramework = target as GameFrameworkHotFix;

		bool modified = false;
		modified |= displayEnum("WindowMode", "窗口类型", ref mGameFramework.mParam.mWindowMode);
		if (mGameFramework.mParam.mWindowMode != WINDOW_MODE.FULL_SCREEN)
		{
			modified |= displayInt("ScreenWidth", "窗口宽度,当WindowMode为FULL_SCREEN时无效", ref mGameFramework.mParam.mScreenWidth);
			modified |= displayInt("ScreenHeight", "窗口高度,当WindowMode为FULL_SCREEN时无效", ref mGameFramework.mParam.mScreenHeight);
		}
		modified |= displayInt("TargetFrameRate", "窗口高度,当WindowMode为FULL_SCREEN时无效", ref mGameFramework.mParam.mDefaultFrameRate);
		modified |= toggle("PoolStackTrace", "是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,可使用F4动态开启", ref mGameFramework.mParam.mEnablePoolStackTrace);
		modified |= toggle("ScriptDebug", "是否启用调试脚本,也就是挂接在GameObject上用于显示调试信息的脚本,可使用F3动态开启", ref mGameFramework.mParam.mEnableScriptDebug);
		modified |= toggle("UseFixedTime", "是否将每帧的时间固定下来", ref mGameFramework.mParam.mUseFixedTime);
		modified |= toggle("ForceTop", "窗口是否始终显示在顶层", ref mGameFramework.mParam.mForceTop);
		modified |= displayEnum("LoadSource", "加载源", ref mGameFramework.mParam.mLoadSource);
		modified |= displayEnum("LogLevel", "日志等级", ref mGameFramework.mParam.mLogLevel);

		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}