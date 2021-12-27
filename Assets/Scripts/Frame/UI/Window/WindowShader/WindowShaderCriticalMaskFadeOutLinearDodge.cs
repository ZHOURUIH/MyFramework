using System;
using UnityEngine;

// 带线性减淡效果的临界值遮罩淡出shader
public class WindowShaderCriticalMaskFadeOutLinearDodge : WindowShaderCriticalMask
{
	protected float mFadeOutCriticalValue;	// 临界值
	protected int mFadeOutCriticalValueID;	// 属性ID
	public WindowShaderCriticalMaskFadeOutLinearDodge()
	{
		mFadeOutCriticalValueID = Shader.PropertyToID("_FadeOutCriticalValue");
	}
	public void setFadeOutCriticalValue(float value) { mFadeOutCriticalValue = value; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetFloat(mFadeOutCriticalValueID, mFadeOutCriticalValue);
		}
	}
}