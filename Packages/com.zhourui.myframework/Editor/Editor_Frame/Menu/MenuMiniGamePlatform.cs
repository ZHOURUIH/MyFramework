using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FrameMacro;

public class MenuMiniGamePlatform
{
	[MenuItem("游戏平台/切换到 OPPO")]
	public static void selectOppo()
	{
		selectPlatform(OPPO_MINI_GAME);
	}
	[MenuItem("游戏平台/切换到 vivo")]
	public static void selectVivo()
	{
		selectPlatform(VIVO_MINI_GAME);
	}
	[MenuItem("游戏平台/切换到抖音")]
	public static void selectByteDance()
	{
		selectPlatform(BYTE_DANCE);
	}
	[MenuItem("游戏平台/平台配置")]
	public static void openPlatformConfig()
	{
		MenuSetting.openPlatformSetting();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void selectPlatform(string platform)
	{
		HashSet<string> defines = new(PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL).Split(';'));
		defines.Remove("");
		defines.Remove(BYTE_DANCE);
		defines.Remove("UNITY_WEIXINMINIGAME");
		defines.Remove(OPPO_MINI_GAME);
		defines.Remove(VIVO_MINI_GAME);
		defines.Add(platform);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, string.Join(";", defines));
		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
		{
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
		}
		AssetDatabase.Refresh();
		Debug.Log("已选择 " + platform + "，编译结束后使用对应平台 SDK 的导出菜单。");
	}
}
