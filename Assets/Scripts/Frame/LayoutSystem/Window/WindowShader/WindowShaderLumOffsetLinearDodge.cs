using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderLumOffsetLinearDodge : WindowShaderLumOffset
{
	public WindowShaderLumOffsetLinearDodge()
	{
		mShaderName = "LumOffsetLinearDodge";
	}
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			if (mat.shader.name == mShaderName && !isFloatEqual(mat.GetFloat(mLumPropertyName), mLumOffsetValue))
			{
				mat.SetFloat(mLumPropertyName, mLumOffsetValue);
			}
		}
	}
}