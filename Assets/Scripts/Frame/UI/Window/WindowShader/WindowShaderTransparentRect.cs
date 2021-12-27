using System;
using UnityEngine;

// 可以使纹理进行内部矩形区域透明显示,相当于在纹理上挖一个透明的矩形区域
public class WindowShaderTransparentRect : WindowShader
{
	protected int mMaxSizeXID;		// 属性ID
	protected int mMaxSizeYID;		// 属性ID
	protected int mSizeXID;			// 属性ID
	protected int mSizeYID;			// 属性ID
	protected int mCenterXID;		// 属性ID
	protected int mCenterYID;		// 属性ID
	protected Vector2 mMaxSize;		// 纹理最大大小
	protected Vector2 mSize;		// 透明区域大小
	protected Vector2 mCenter;		// 透明区域位置,以屏幕中心为原点
	public WindowShaderTransparentRect()
	{
		mMaxSizeXID = Shader.PropertyToID("_MaxSizeX");
		mMaxSizeYID = Shader.PropertyToID("_MaxSizeY");
		mSizeXID = Shader.PropertyToID("_SizeX");
		mSizeYID = Shader.PropertyToID("_SizeY");
		mCenterXID = Shader.PropertyToID("_CenterX");
		mCenterYID = Shader.PropertyToID("_CenterY");
	}
	public void setMaxSize(Vector2 size) { mMaxSize = size; }
	public void setRectSize(Vector2 size) { mSize = size; }
	public void setRectCenter(Vector2 pos) { mCenter = pos; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetInt(mMaxSizeXID, (int)mMaxSize.x);
			mat.SetInt(mMaxSizeYID, (int)mMaxSize.y);
			mat.SetInt(mSizeXID, (int)mSize.x);
			mat.SetInt(mSizeYID, (int)mSize.y);
			mat.SetInt(mCenterXID, (int)mCenter.x);
			mat.SetInt(mCenterYID, (int)mCenter.y);
		}
	}
}