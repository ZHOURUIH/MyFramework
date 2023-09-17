using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using static FrameDefine;
using static StringUtility;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 用于记录Image组件上的图片所在的路径,因为在运行时是没办法获得Image上图片的路径,从而也就无法直到所在的图集
// 所以使用一个组件来在编辑模式下就记录路径
[ExecuteInEditMode]
public class ImageAtlasPath : MonoBehaviour
{
	public string mAtlasPath;		// 记录的图集路径
	public bool mRefresh = true;	// 是否刷新一次,用于在修改图片后手动刷新获取一次图片所在的路径
	private void OnValidate()
	{
		if(mRefresh)
		{
			refreshPath();
			mRefresh = false;
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshPath()
	{
#if UNITY_EDITOR
		var image = GetComponent<Image>();
		Texture texture = image?.mainTexture;
		if (texture == null)
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer != null)
			{
				texture = spriteRenderer.sprite?.texture;
			}
		}
		if (texture == null)
		{
			mAtlasPath = string.Empty;
			return;
		}
		mAtlasPath = AssetDatabase.GetAssetPath(texture);
		// 去除Assets/GameResoureces或者Assets/Resources前缀
		removeStartString(ref mAtlasPath, P_GAME_RESOURCES_PATH);
		removeStartString(ref mAtlasPath, P_RESOURCES_PATH);
#endif
	}
}