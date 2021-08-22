using System;
using UnityEngine;

public class WindowShaderMaskCut : WindowShader
{
	protected Texture mMask;
	protected Vector2 mMaskScale;
	protected int mMaskTexID;
	protected int mSizeXID;
	protected int mSizeYID;
	public WindowShaderMaskCut()
	{
		mMask = null;
		mMaskScale = Vector2.one;
		mMaskTexID = Shader.PropertyToID("_MaskTex");
		mSizeXID = Shader.PropertyToID("_SizeX");
		mSizeYID = Shader.PropertyToID("_SizeY");
	}
	public void setMaskTexture(Texture mask) { mMask = mask; }
	public void setMaskScale(Vector2 scale) { mMaskScale = scale; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetTexture(mMaskTexID, mMask);
			mat.SetFloat(mSizeXID, mMaskScale.x);
			mat.SetFloat(mSizeYID, mMaskScale.y);
		}
	}
}