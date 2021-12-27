using System;
using UnityEngine;

// HSL偏移计算shader,可变化图像的色相,色度,饱和度
public class WindowShaderHSLOffset : WindowShader
{
	protected Vector3 mHSLOffsetValue;	// 当前HSL偏移,只有当shader为HSLOffet或者HSLOffsetLinearDodge时才有效
	protected Texture mHSLTexture;		// 纹理
	protected int mHSLOffsetID;			// 属性ID
	protected int mHSLTexID;			// 属性ID
	protected int mHasHSLTexID;			// 是否有mHSLTexture
	public WindowShaderHSLOffset()
	{
		mHSLOffsetID = Shader.PropertyToID("_HSLOffset");
		mHSLTexID = Shader.PropertyToID("_HSLTex");
		mHasHSLTexID = Shader.PropertyToID("_HasHSLTex");
	}
	public void setHSLOffset(Vector3 offset) { mHSLOffsetValue = offset; }
	public Vector3 getHSLOffset() { return mHSLOffsetValue; }
	public void setHSLTexture(Texture hslTexture) { mHSLTexture = hslTexture; }
	public Texture getHSLTexture() { return mHSLTexture; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetColor(mHSLOffsetID, new Color(mHSLOffsetValue.x, mHSLOffsetValue.y, mHSLOffsetValue.z));
			mat.SetTexture(mHSLTexID, mHSLTexture);
			mat.SetInt(mHasHSLTexID, mHSLTexture == null ? 0 : 1);
		}
	}
}