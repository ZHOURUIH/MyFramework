using System;
using UnityEngine;

public class WindowShaderLumOffset : WindowShader
{
	protected float mLumOffsetValue;
	protected int mLumOffsetID;
	public WindowShaderLumOffset()
	{
		mLumOffsetID = Shader.PropertyToID("_LumOffset");
	}
	public void setLumOffset(float lumOffset){ mLumOffsetValue = lumOffset;}
	public float getLumOffset() { return mLumOffsetValue; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetFloat(mLumOffsetID, mLumOffsetValue);
		}
	}
}