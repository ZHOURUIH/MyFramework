using System;
using UnityEngine;
using static MathUtility;

// 像素遮罩裁剪,根据遮罩图片的像素红色分量判断图像像素是否显示
public class WindowShaderPixelMaskCut : WindowShader
{
	protected Texture mMask;		// 遮罩纹理
	protected Vector2 mMaskSize;	// 遮罩大小
	protected Vector2 mMaskPos;		// 遮罩位置
	protected int mMaskTexID;		// 属性ID
	protected int mSizeXID;			// 属性ID
	protected int mSizeYID;			// 属性ID
	protected int mPosXID;			// 属性ID
	protected int mPosYID;			// 属性ID
	public WindowShaderPixelMaskCut()
	{
		mMaskTexID = Shader.PropertyToID("_MaskTex");
		mSizeXID = Shader.PropertyToID("_SizeX");
		mSizeYID = Shader.PropertyToID("_SizeY");
		mPosXID = Shader.PropertyToID("_PosX");
		mPosYID = Shader.PropertyToID("_PosY");
	}
	public void setMaskTexture(Texture mask) { mMask = mask; }
	public void setMaskSize(Vector2 size) { mMaskSize = size; }
	public void setMaskPos(Vector2 pos) { mMaskPos = pos; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetTexture(mMaskTexID, mMask);
			if (isVectorZero(mMaskSize))
			{
				mMaskSize = new Vector2(mMask.width, mMask.height);
			}
			mat.SetFloat(mSizeXID, mMaskSize.x);
			mat.SetFloat(mSizeYID, mMaskSize.y);
			mat.SetFloat(mPosXID, mMaskPos.x);
			mat.SetFloat(mPosYID, mMaskPos.y);
		}
	}
}