using System.Collections.Generic;
using UnityEditor;

public class GameMenu
{
	public const string mMenuName = "游戏菜单/";
#if UNITY_ANDROID
	[MenuItem(mMenuName + "发布Android版本", false, 1000)]
#elif UNITY_STANDALONE_WIN
	[MenuItem(mMenuName + "发布Windows版本", false, 1000)]
#elif UNITY_IOS
	[MenuItem(mMenuName + "发布iOS版本", false, 1000)]
#elif UNITY_STANDALONE_OSX
	[MenuItem(mMenuName + "发布MacOS版本", false, 1000)]
#elif UNITY_WEBGL
	[MenuItem(mMenuName + "发布WebGL版本", false, 1000)]
#endif
	public static void releaseVersion()
	{
		EditorWindow.GetWindow<GameReleaseWindow>(true, "发布版本", true).start();
	}
	//---------------------------------------------------------------------------------------------------------------
	public static HashSet<string> collectUsedFile()
	{
		return new();
	}
}