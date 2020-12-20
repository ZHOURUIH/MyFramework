using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderCriticalMask : WindowShader
{
	protected float mCriticalValue = 1.0f;
	protected bool mInverseVertical = false;
	protected int mCriticalValueID;
	protected int mInverseVerticalID;
	public WindowShaderCriticalMask()
	{
		mCriticalValueID = Shader.PropertyToID("_CriticalValue");
		mInverseVerticalID = Shader.PropertyToID("_InverseVertical");
	}
	public void setCriticalValue(float critical) { mCriticalValue = critical; }
	public void setInverseVertical(bool inverse) { mInverseVertical = inverse; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetFloat(mCriticalValueID, mCriticalValue);
			mat.SetInt(mInverseVerticalID, mInverseVertical ? 1 : 0);
		}
	}
}