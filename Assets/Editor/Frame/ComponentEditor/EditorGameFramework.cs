using UnityEditor;

[CustomEditor(typeof(GameEntry), true)]
public class EditorGameFramework : GameEditorBase
{
	protected GameEntry mGameEntry;
	public override void OnInspectorGUI()
	{
		mGameEntry = target as GameEntry;

		bool modified = false;
		modified |= displayEnum("WindowMode", "��������", ref mGameEntry.mFramworkParam.mWindowMode);
		if (mGameEntry.mFramworkParam.mWindowMode != WINDOW_MODE.FULL_SCREEN)
		{
			modified |= displayInt("ScreenWidth", "���ڿ��,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameEntry.mFramworkParam.mScreenWidth);
			modified |= displayInt("ScreenHeight", "���ڸ߶�,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameEntry.mFramworkParam.mScreenHeight);
		}
		modified |= displayInt("TargetFrameRate", "���ڸ߶�,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameEntry.mFramworkParam.mDefaultFrameRate);
		modified |= toggle("PoolStackTrace", "�Ƿ����ö�����еĶ�ջ׷��,���ڶ�ջ׷�ٷǳ���ʱ,����Ĭ�Ϲر�,��ʹ��F4��̬����", ref mGameEntry.mFramworkParam.mEnablePoolStackTrace);
		modified |= toggle("ScriptDebug", "�Ƿ����õ��Խű�,Ҳ���ǹҽ���GameObject��������ʾ������Ϣ�Ľű�,��ʹ��F3��̬����", ref mGameEntry.mFramworkParam.mEnableScriptDebug);
		modified |= displayEnum("LoadSource", "����Դ", ref mGameEntry.mFramworkParam.mLoadSource);

		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}