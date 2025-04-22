#if USE_TMP
using TMPro;
#endif
using UnityEngine;
using UnityEditor;
using System.IO;
using static FrameBaseDefine;
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
#if USE_TMP
	[MenuItem(mGameAssetsMenuName + "精简TMP字体大小,但是精简完以后无法再替换材质")]
	public static void extractTexture()
	{
		string fontPath = AssetDatabase.GetAssetPath(Selection.activeObject).rightToLeft();
		string texturePath = fontPath.Replace(".asset", ".png");
		var targeFontAsset = Selection.activeObject as TMP_FontAsset;
		Texture2D texture2D = new(targeFontAsset.atlasTexture.width, targeFontAsset.atlasTexture.height, TextureFormat.ASTC_6x6, false);
		Graphics.CopyTexture(targeFontAsset.atlasTexture, texture2D);
		byte[] dataBytes = texture2D.EncodeToPNG();
		FileStream fs = File.Open(texturePath, FileMode.OpenOrCreate);
		fs.Write(dataBytes, 0, dataBytes.Length);
		fs.Flush();
		fs.Close();
		AssetDatabase.Refresh();
		Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath.Replace(Application.dataPath, "Assets"));
		AssetDatabase.RemoveObjectFromAsset(targeFontAsset.atlasTexture);
		targeFontAsset.atlasTextures[0] = atlas;
		targeFontAsset.material.mainTexture = atlas;
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
#endif
}