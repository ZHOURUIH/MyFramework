using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderHSLOffsetLinearDodge : WindowShaderHSLOffset
{
	protected string mHSLOffsetLinearDodge = "HSLOffsetLinearDodge";
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			if (mat.shader.name == mHSLOffsetLinearDodge)
			{
				mat.SetColor("_HSLOffset", new Color(mHSLOffsetValue.x, mHSLOffsetValue.y, mHSLOffsetValue.z));
				mat.SetTexture("_HSLTex", mHSLTexture);
				mat.SetInt("_HasHSLTex", mHSLTexture == null ? 0 : 1);
			}
		}
	}
}