using System;
using UnityEditor;
using UnityEngine;
using static UnityUtility;
using static FrameDefine;

public class MenuGameObject
{
	[MenuItem("GameObject/新建布局", false, -100)]
	public static void createNewLayout()
	{
		Vector2 gameViewSize = getGameViewSize();
		if ((int)gameViewSize.x != STANDARD_WIDTH || (int)gameViewSize.y != STANDARD_HEIGHT)
		{
			Debug.LogError("当前分辨率不是标准分辨率：" + STANDARD_WIDTH + "*" + STANDARD_HEIGHT + ",请重新设置GameView的分辨率");
			return;
		}
		EditorWindow.GetWindow<NewLayoutWindow>(true, "新建布局", true).Show();
	}
}