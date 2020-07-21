using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUIShape : txNGUITexture
{
	protected INGUIShape mShape;
	protected bool mDirty;
	public txNGUIShape() 
	{
		mDirty = true;
	}
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		mShape = createShape();
		mTexture.mCustomVertices = true;
		mTexture.mOnFillVertices = OnFillVertices;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mShape.getDirty())
		{
			mShape.setDirty(false);
			mShape.onPointsChanged();
		}
		if (mDirty)
		{
			mDirty = false;
			mTexture.MarkAsChanged();
		}
	}
	public override Color getColor() { return mShape.getColor(); }
	public override void setColor(Color color)
	{
		mShape.setColor(color);
		markChanged();
	}
	public override void cloneFrom(txUIObject obj)
	{
		base.cloneFrom(obj);
		txNGUIShape ori = obj as txNGUIShape;
		if (ori != null)
		{
			setColor(ori.getColor());
		}
	}
	public void markChanged(bool forceUpdate = false)
	{
		if(forceUpdate)
		{
			mShape.setDirty(false);
			mShape.onPointsChanged();
			mDirty = false;
			mTexture.MarkAsChanged();
		}
		else
		{
			mDirty = true;
		}
	}
	// 获得在父节点坐标系中的图形中心,默认是窗口坐标作为图形中心
	public virtual Vector3 getShapeCenter() { return getPosition(); }
	public List<Vector2> getPolygonPoints() { return mShape.getPolygonPoints(); }
	//--------------------------------------------------------------------------------------------------------------------
#if UNITY_5
	protected void OnFillVertices(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		foreach(var item in mNGUILine.mVertices)
		{
			verts.Add(item);
		}
		foreach (var item in mNGUILine.mUVs)
		{
			uvs.Add(item);
		}
		foreach (var item in mNGUILine.mColors)
		{
			cols.Add(item);
		}
	}
#else
	protected void OnFillVertices(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		verts.AddRange(mShape.getVertices());
		uvs.AddRange(mShape.getUVs());
		cols.AddRange(mShape.getColors());
	}
#endif
	protected virtual INGUIShape createShape() { return null; }
}

#endif