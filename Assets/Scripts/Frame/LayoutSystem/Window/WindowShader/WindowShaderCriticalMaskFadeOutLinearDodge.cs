using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderCriticalMaskFadeOutLinearDodge : WindowShaderCriticalMask
{
	protected float mFadeOutCriticalValue;
	protected string mCriticalMaskFadeOutLinearDodge = "CriticalMaskFadeOutLinearDodge";
	public void setFadeOutCriticalValue(float value) { mFadeOutCriticalValue = value; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			if (getFileName(mat.shader.name) == mCriticalMaskFadeOutLinearDodge)
			{
				mat.SetFloat("_FadeOutCriticalValue", mFadeOutCriticalValue);
				mat.SetFloat("_CriticalValue", mCriticalValue);
				mat.SetInt("_InverseVertical", mInverseVertical ? 1 : 0);
			}
		}
	}
}