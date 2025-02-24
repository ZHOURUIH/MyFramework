using UnityEditor;

[CustomEditor(typeof(GameFrameworkHotFix), true)]
public class EditorGameFramework : GameEditorBase
{
	protected GameFrameworkHotFix mGameFramework;
	public override void OnInspectorGUI()
	{
		mGameFramework = target as GameFrameworkHotFix;

		bool modified = false;
		modified |= displayEnum("WindowMode", "��������", ref mGameFramework.mParam.mWindowMode);
		if (mGameFramework.mParam.mWindowMode != WINDOW_MODE.FULL_SCREEN)
		{
			modified |= displayInt("ScreenWidth", "���ڿ��,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameFramework.mParam.mScreenWidth);
			modified |= displayInt("ScreenHeight", "���ڸ߶�,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameFramework.mParam.mScreenHeight);
		}
		modified |= displayInt("TargetFrameRate", "���ڸ߶�,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameFramework.mParam.mDefaultFrameRate);
		modified |= toggle("PoolStackTrace", "�Ƿ����ö�����еĶ�ջ׷��,���ڶ�ջ׷�ٷǳ���ʱ,����Ĭ�Ϲر�,��ʹ��F4��̬����", ref mGameFramework.mParam.mEnablePoolStackTrace);
		modified |= toggle("ScriptDebug", "�Ƿ����õ��Խű�,Ҳ���ǹҽ���GameObject��������ʾ������Ϣ�Ľű�,��ʹ��F3��̬����", ref mGameFramework.mParam.mEnableScriptDebug);
		modified |= toggle("UseFixedTime", "�Ƿ�ÿ֡��ʱ��̶�����", ref mGameFramework.mParam.mUseFixedTime);
		modified |= toggle("ForceTop", "�����Ƿ�ʼ����ʾ�ڶ���", ref mGameFramework.mParam.mForceTop);
		modified |= displayEnum("LoadSource", "����Դ", ref mGameFramework.mParam.mLoadSource);
		modified |= displayEnum("LogLevel", "��־�ȼ�", ref mGameFramework.mParam.mLogLevel);

		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}