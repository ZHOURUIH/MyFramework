using UnityEngine;

// 亮度变化的shader
public class WindowShaderLumOffset : WindowShader
{
	protected float mLumOffsetValue;	// 亮度偏移
	protected int mLumOffsetID;			// 属性ID
	public WindowShaderLumOffset()
	{
		mLumOffsetID = Shader.PropertyToID("_LumOffset");
	}
    public override void resetProperty()
    {
        base.resetProperty();
		mLumOffsetValue = 0.0f;
		//mLumOffsetID = 0;
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