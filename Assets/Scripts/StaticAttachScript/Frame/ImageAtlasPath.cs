using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImageAtlasPath : MonoBehaviour
{
	public string mAtlasPath;
	public bool mRefresh = true;
	private void OnValidate()
	{
		if(mRefresh)
		{
			refreshPath();
			mRefresh = false;
		}
	}
	//-------------------------------------------------------------------------------------------------------------
	protected void refreshPath()
	{
#if UNITY_EDITOR
		Image image = GetComponent<Image>();
		if (image == null)
		{
			Debug.LogError("can not find Image component");
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