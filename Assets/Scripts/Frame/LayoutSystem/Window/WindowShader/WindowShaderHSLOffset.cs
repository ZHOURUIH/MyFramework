using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderHSLOffset : WindowShader
{
	protected Vector3 mHSLOffsetValue;   // 当前HSL偏移,只有当shader为HSLOffet或者HSLOffsetLinearDodge时才有效
	protected Texture mHSLTexture;
	protected string mHSLOffset = "HSLOffset";
	public void setHSLOffset(Vector3 offset) { mHSLOffsetValue = offset; }
	public Vector3 getHSLOffset() { return mHSLOffsetValue; }
	public void setHSLTexture(Texture hslTexture) { mHSLTexture = hslTexture; }
	public Texture getHSLTexture() { return mHSLTexture; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			if (getFileName(mat.shader.name) == mHSLOffset)
			{
				mat.SetColor("_HSLOffset", new Color(mHSLOffsetValue.x, mHSLOffsetValue.y, mHSLOffsetValue.z));
				mat.SetTexture("_HSLTex", mHSLTexture);
				mat.SetInt("_HasHSLTex", mHSLTexture == null ? 0 : 1);
			}
		}
	}
}