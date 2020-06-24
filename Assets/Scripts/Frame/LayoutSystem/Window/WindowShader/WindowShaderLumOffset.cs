using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderLumOffset : WindowShader
{
	protected float mLumOffsetValue;
	protected string mShaderName;
	protected string mLumPropertyName;
	public WindowShaderLumOffset()
	{
		mShaderName = "LumOffset";
		mLumPropertyName = "_LumOffset";
	}
	public void setLumOffset(float lumOffset){ mLumOffsetValue = lumOffset;}
	public float getLumOffset() { return mLumOffsetValue; }
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