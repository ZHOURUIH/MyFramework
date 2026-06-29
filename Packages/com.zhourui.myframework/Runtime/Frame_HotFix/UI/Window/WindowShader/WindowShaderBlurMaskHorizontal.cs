using UnityEngine;

// 实际上这个文件夹中的shader对象,由于shader文件早已不再,或者由于unity版本升级已经不再适用,所以这些shader对象已经处于废弃状态
// 高斯模糊中横向模糊的shader
public class WindowShaderBlurMaskHorizontal : WindowShader
{
	protected float mSampleInterval;	// 采样间隔
	protected int mSampleIntervalID;	// 属性ID
	public WindowShaderBlurMaskHorizontal()
	{
		mSampleInterval = 1.5f;
		mSampleIntervalID = Shader.PropertyToID("_SampleInterval");
	}
    public override void resetProperty()
    {
        base.resetProperty();
		mSampleInterval = 1.5f;
		//mSampleIntervalID = 0;
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