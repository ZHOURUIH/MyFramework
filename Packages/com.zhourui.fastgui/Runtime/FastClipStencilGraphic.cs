using UnityEngine;

// FastRectMask2D内部Stencil Writer/Pop渲染节点，用户不应手工添加。
// Geometry只描述Clip自身形状，不参与后代元素Geometry；Writer写Stencil，Pop在子树末尾清除当前层Stencil bit。
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("")]
public class FastClipStencilGraphic : FastUIRenderElement
{
	protected FastRectMask2D mSource;
	protected bool mWriter;
	protected override void OnDisable()
	{
		base.OnDisable();
		if (mCanvas != null && mCanvas.gameObject.activeInHierarchy)
		{
			unregisterCanvas();
		}
	}
	public FastRectMask2D getSource()
	{
		return mSource;
	}
	public bool isWriter()
	{
		return mWriter;
	}
	public override int getSOACompatibilityID()
	{
		return FastUIRenderSOACompatibility.ClipStencil;
	}
	public void setSource(FastRectMask2D source, bool writer)
	{
		mSource = source;
		mWriter = writer;
		mColor = Color.white;
		markVertexDirty(FastUIDirtyFlags.AllVertex);
		if (mCanvas != null)
		{
			mCanvas.notifyElementBatchChanged(this);
		}
	}
	public void setStencilState(FastUIMaskState state)
	{
		setMaskState(state);
	}
	public void notifySourceGeometryChanged()
	{
		markVertexDirty(FastUIDirtyFlags.Geometry | FastUIDirtyFlags.Transform);
	}
	public override Texture getRenderTexture()
	{
		return Texture2D.whiteTexture;
	}
	public override Material getSourceRenderMaterial(Material defaultMaterial)
	{
		return defaultMaterial;
	}
	public override void refreshMaskStateFromHierarchy()
	{
	}
	public override void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		if (mSource == null)
		{
			builder.Clear();
			return;
		}
		Rect clipRect = mSource.getClipRect();
		if (clipRect.width <= 0.0f || clipRect.height <= 0.0f)
		{
			builder.Clear();
			return;
		}
		Matrix4x4 matrix = canvasWorldToLocal * mSource.getRectTransform().localToWorldMatrix;
		Vector3 bottomLeft = matrix.MultiplyPoint3x4(new Vector3(clipRect.xMin, clipRect.yMin, 0.0f));
		Vector3 topLeft = matrix.MultiplyPoint3x4(new Vector3(clipRect.xMin, clipRect.yMax, 0.0f));
		Vector3 topRight = matrix.MultiplyPoint3x4(new Vector3(clipRect.xMax, clipRect.yMax, 0.0f));
		Vector3 bottomRight = matrix.MultiplyPoint3x4(new Vector3(clipRect.xMax, clipRect.yMin, 0.0f));
		builder.Clear();
		builder.AddQuad(bottomLeft, topLeft, topRight, bottomRight, Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
	}
}
