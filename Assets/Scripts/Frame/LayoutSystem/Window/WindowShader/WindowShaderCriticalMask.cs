using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderCriticalMask : WindowShader
{
	protected float mCriticalValue = 1.0f;
	protected bool mInverseVertical = false;
	protected string mCriticalMask = "CriticalMask";
	public void setCriticalValue(float critical) { mCriticalValue = critical; }
	public void setInverseVertical(bool inverse) { mInverseVertical = inverse; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			if (getFileName(mat.shader.name) == mCriticalMask)
			{
				mat.SetFloat("_CriticalValue", mCriticalValue);
				mat.SetInt("_InverseVertical", mInverseVertical ? 1 : 0);
			}
		}
	}
}