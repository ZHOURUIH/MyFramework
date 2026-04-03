#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using static FrameDefine;
using static StringUtility;
using static MathUtility;
using static FileUtility;

[ExecuteAlways]
public class SequenceRawImagePreview : MonoBehaviour
{
	[Range(0, 1)]
	public float mSlider;
	public RawImage mRawImage;
	protected RawImageAnimPath mAnimPath;
	public List<Texture> mTextureList = new();
	public void Awake()
	{
		enabled = !Application.isPlaying;
		mRawImage = GetComponentInChildren<RawImage>();
		mAnimPath = mRawImage.GetComponent<RawImageAnimPath>();
	}
	public void OnEnable()
	{
		mTextureList.Clear();
		if (mAnimPath == null)
		{
			Debug.LogError("需要添加RawImageAnimPath组件以后才能预览");
			return;
		}
		// 刷新图片列表
		if (mRawImage != null && mRawImage.texture != null)
		{
			for (int i = 0; i < 1000; ++i)
			{
				string fileName = F_GAME_RESOURCES_PATH + mAnimPath.mTexturePath + "/" + mAnimPath.mTextureName + "_" + IToS(i) + ".png";
				if (!isFileExist(fileName))
				{
					break;
				}
				if (!mTextureList.addNotNull(AssetDatabase.LoadAssetAtPath<Texture>(fullPathToProjectPath(fileName))))
				{
					break;
				}
			}
		}
	}
	public void Update()
	{
		// 刷新当前图片
		int frameCount = mTextureList.Count;
		if (frameCount > 0)
		{
			mRawImage.texture = mTextureList[clamp(ceil(mSlider * frameCount) - 1, 0, frameCount - 1)];
		}
	}
}

#endif