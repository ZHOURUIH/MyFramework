using System;
using UnityEngine;

public class WindowShaderPixelMaskCut : WindowShader
{
	protected Texture mMask;
	protected Vector2 mMaskSize;
	protected Vector2 mMaskPos;
	protected int mMaskTexID;
	protected int mSizeXID;
	protected int mSizeYID;
	protected int mPosXID;
	protected int mPosYID;
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