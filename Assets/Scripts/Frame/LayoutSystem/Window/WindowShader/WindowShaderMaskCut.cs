using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderMaskCut : WindowShader
{
	protected Texture mMask;
	protected Vector2 mMaskScale = Vector2.one;
	protected string mMaskCut = "MaskCut";
	public void setMaskTexture(Texture mask) { mMask = mask; }
	public void setMaskScale(Vector2 scale) { mMaskScale = scale; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			if (getFileName(mat.shader.name) == mMaskCut)
			{
				mat.SetTexture("_MaskTex", mMask);
				mat.SetFloat("_SizeX", mMaskScale.x);
				mat.SetFloat("_SizeY", mMaskScale.y);
			}
		}
	}
}