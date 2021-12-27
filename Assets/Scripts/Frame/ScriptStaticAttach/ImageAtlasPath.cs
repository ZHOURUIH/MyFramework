using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 用于记录Image组件上的图片所在的路径,因为在运行时是没办法获得Image上图片的路径,从而也就无法直到所在的图集
// 所以使用一个组件来在编辑模式下就记录路径
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
		if (image == null)
		{
			Debug.LogError("can not find Image Component");
			return;
		}
		mAtlasPath = AssetDatabase.GetAssetPath(image.mainTexture);
		// 需要去除后缀名
		mAtlasPath = StringUtility.getFileNameNoSuffix(mAtlasPath);
		// 去除Assets/GameResoureces前缀
		StringUtility.removeStartString(ref mAtlasPath, FrameDefine.P_GAME_RESOURCES_PATH);
#endif
	}
}