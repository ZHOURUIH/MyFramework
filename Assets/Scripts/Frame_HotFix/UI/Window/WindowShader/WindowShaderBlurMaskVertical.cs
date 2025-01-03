using UnityEngine;

// 高斯模糊中纵向模糊的shader
public class WindowShaderBlurMaskVertical : WindowShader
{
	protected float mSampleInterval;	// 采样间隔
	protected int mSampleIntervalID;	// 属性ID
	public WindowShaderBlurMaskVertical()
	{
		mSampleInterval = 1.5f;
		mSampleIntervalID = Shader.PropertyToID("_SampleInterval");
	}
	public void setSampleInterval(float sampleInterval) { mSampleInterval = sampleInterval; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetFloat(mSampleIntervalID, mSampleInterval);
		}
	}
}