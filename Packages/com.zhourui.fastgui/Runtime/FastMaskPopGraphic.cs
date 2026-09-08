using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("")]
public class FastMaskPopGraphic : FastUIRenderElement
{
	protected FastUIRenderElement mSource;
	public void setSource(FastUIRenderElement source)
	{
		mSource = source;
		if (source != null)
		{
			mColor = source.getColor();
		}
		markVertexDirty(FastUIDirtyFlags.AllVertex);
		notifyBatchChanged();
	}
	public FastUIRenderElement getSource()
	{
		return mSource;
	}
	public override Texture getRenderTexture()
	{
		return mSource != null ? mSource.getRenderTexture() : Texture2D.whiteTexture;
	}
	public override Material getSourceRenderMaterial(Material defaultMaterial)
	{
		return mSource != null ? mSource.getSourceRenderMaterial(defaultMaterial) : base.getSourceRenderMaterial(defaultMaterial);
	}
	public override int getSOACompatibilityID()
	{
		return FastUIRenderSOACompatibility.MaskPop;
	}
	public override void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		if (mSource == null)
		{
			builder.Clear();
			return;
		}
		mSource.buildGeometry(builder, canvasWorldToLocal);
	}
	public override void refreshMaskStateFromHierarchy()
	{
	}
	public void setPopState(FastUIMaskState state)
	{
		setMaskState(state);
	}
	public void syncSourceColor()
	{
		if (mSource == null)
		{
			return;
		}
		Color color = mSource.getColor();
		if (mColor == color)
		{
			return;
		}
		mColor = color;
		markVertexDirty(FastUIDirtyFlags.Color);
	}
	public void notifySourceGeometryChanged()
	{
		markVertexDirty(FastUIDirtyFlags.Geometry | FastUIDirtyFlags.Transform);
	}
	public void notifySourceBatchChanged()
	{
		releaseMaskRuntimeMaterial();
		notifyBatchChanged();
	}
}
