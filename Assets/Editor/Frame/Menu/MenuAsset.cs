using UnityEngine;
using UnityEditor;
using static FrameDefine;
using static UnityUtility;

public class MenuAsset
{
	public const string mGameAssetsMenuName = "GameAsset/";
	[MenuItem(mGameAssetsMenuName + "将图集还原为散图")]
	public static void doMultiSpriteToSpritePNG()
	{
		var tex = Selection.activeObject as Texture2D;
		if (tex == null)
		{
			Debug.LogError("当前在Project中没有选中任何图集文件");
			return;
		}
		if (multiSpriteToSpritePNG(Selection.activeObject as Texture2D, F_ASSETS_PATH + tex.name))
		{
			Debug.Log("已输出图片到" + F_ASSETS_PATH + tex.name);
		}
		else
		{
			Debug.LogError("生成散图失败");
		}
	}
}