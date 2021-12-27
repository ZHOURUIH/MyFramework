using System;
using UnityEngine;

// 临界值遮罩shader,根据图片中的值与临界值的比较判断该像素是否显示
public class WindowShaderCriticalMask : WindowShader
{
	protected float mCriticalValue;		// 临界值
	protected bool mInverseVertical;	// 是否上下翻转
	protected int mCriticalValueID;		// 属性ID
	protected int mInverseVerticalID;	// 属性ID
	public WindowShaderCriticalMask()
	{
		mCriticalValue = 1.0f;
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