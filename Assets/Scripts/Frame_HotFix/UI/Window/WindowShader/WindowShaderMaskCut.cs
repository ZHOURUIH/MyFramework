using UnityEngine;

// 根据遮罩图片判断图像像素是否显示的shader
public class WindowShaderMaskCut : WindowShader
{
	protected Texture mMask;		// 遮罩纹理
	protected Vector2 mMaskScale;	// 遮罩缩放
	protected int mMaskTexID;		// 属性ID
	protected int mSizeXID;			// 属性ID
	protected int mSizeYID;			// 属性ID
	public WindowShaderMaskCut()
	{
		mMask = null;
		mMaskScale = Vector2.one;
		mMaskTexID = Shader.PropertyToID("_MaskTex");
		mSizeXID = Shader.PropertyToID("_SizeX");
		mSizeYID = Shader.PropertyToID("_SizeY");
	}
	public void setMaskTexture(Texture mask) { mMask = mask; }
	public void setMaskScale(Vector2 scale) { mMaskScale = scale; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetTexture(mMaskTexID, mMask);
			mat.SetFloat(mSizeXID, mMaskScale.x);
			mat.SetFloat(mSizeYID, mMaskScale.y);
		}
	}
}