using UnityEngine.U2D;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.U2D;
#endif
using UObject = UnityEngine.Object;
using static FrameBaseUtility;

public static class SpriteAtlasExtension
{
	public static bool isSpriteInAtlas(this SpriteAtlas atlas, Sprite targetSprite)
	{
#if UNITY_EDITOR
		if (atlas == null || targetSprite == null)
		{
			return false;
		}
		foreach (UObject packable in atlas.GetPackables())
		{
			// 直接比较图片对象
			if (packable is Sprite sprite && sprite == targetSprite)
			{
				return true;
			}
			// 通过路径拼接出完整的文件路径,加载后判断是否为指定的图片对象
			else if (packable is DefaultAsset folder && loadAssetAtPath<Sprite>(getAssetPath(folder) + "/" + targetSprite.name + ".png") == targetSprite)
			{
				return true;
			}
		}
#endif
		return false;
	}
}