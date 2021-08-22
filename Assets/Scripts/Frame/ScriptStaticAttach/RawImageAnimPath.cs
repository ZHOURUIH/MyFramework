using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RawImageAnimPath : MonoBehaviour
{
	public string mTexturePath;		// 序列帧所在的目录,相对于GameResources,不带文件名,以/结尾
	public string mTextureName;		// 序列帧图片名前缀,不带_
	public int mImageCount;			// 图片序列帧的图片数量,图片的名字中的下标应该从0开始
	public bool mRefresh = true;
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
		var rawImage = GetComponent<RawImage>();
		if (rawImage == null)
		{
			Debug.LogError("can not find RawImage Component");
			return;
		}
		string imagePathName = AssetDatabase.GetAssetPath(rawImage.mainTexture);
		mTextureName = StringUtility.getFileNameNoSuffix(imagePathName, true);
		mTextureName = mTextureName.Substring(0, mTextureName.LastIndexOf('_'));
		mTexturePath = StringUtility.getFilePath(imagePathName, true);
		// 去除Assets/GameResoureces前缀
		StringUtility.removeStartString(ref mTexturePath, FrameDefine.P_GAME_RESOURCES_PATH);
		// 获取图片数量
		string suffix = StringUtility.getFileSuffix(imagePathName);
		string preString = FrameDefine.F_GAME_RESOURCES_PATH + mTexturePath + mTextureName + "_";
		int index = 0;
		while(true)
		{
			if (!FileUtility.isFileExist(preString + StringUtility.IToS(index) + suffix))
			{
				break;
			}
			++index;
		}
		mImageCount = index;
#endif
	}
}