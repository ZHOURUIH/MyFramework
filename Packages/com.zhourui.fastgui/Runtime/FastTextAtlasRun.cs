using UnityEngine;

// FastText只有在同一段文字真实跨Font/Atlas时才创建该节点。
// 它是Text渲染分段，不是RawImage；只复用Graphic公共生命周期/Mask/Slot能力。
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FastTextAtlasRun : FastUIRenderElement
{
	protected FastText mOwner;
	protected int mRunIndex = -1;
	protected Material mBaseMaterial;
	protected Texture mAtlasTexture;
	public FastText getOwner()
	{
		return mOwner;
	}
	public int getRunIndex()
	{
		return mRunIndex;
	}
	public override Texture getRenderTexture()
	{
		return mAtlasTexture != null ? mAtlasTexture : Texture2D.whiteTexture;
	}
	public override Material getSourceRenderMaterial(Material defaultMaterial)
	{
		Material source = mMaterial != null ? mMaterial : defaultMaterial;
		return FastUITextRenderMaterialCache.get(source, getRenderTexture());
	}
	public override bool requiresTMPVertexLayout()
	{
		return true;
	}
	public override int getSOACompatibilityID()
	{
		return FastUIRenderSOACompatibility.TextAtlasRun;
	}
	public override bool requiresGeometryRebuildForColor()
	{
		return mOwner != null && mOwner.hasRichColor();
	}
	public override bool geometryControlsVertexColor()
	{
		return true;
	}
	public override bool requiresGeometryRebuildForMatrixTransform()
	{
		return true;
	}
	public void setOwnerAndRun(FastText owner, int runIndex)
	{
		bool changed = mOwner != owner || mRunIndex != runIndex;
		mOwner = owner;
		mRunIndex = runIndex;
		if (changed)
		{
			markVertexDirty(FastUIDirtyFlags.Geometry);
		}
	}
	public void setRenderSource(Material baseMaterial, Texture atlasTexture)
	{
		if (mBaseMaterial == baseMaterial && mAtlasTexture == atlasTexture)
		{
			return;
		}
		releaseMaterialVariant();
		mBaseMaterial = baseMaterial;
		mAtlasTexture = atlasTexture;
		Material material = baseMaterial;
		base.setMaterial(material);
		notifyBatchChanged();
		markVertexDirty(FastUIDirtyFlags.Geometry);
	}
	public void releaseRenderSource()
	{
		releaseMaterialVariant();
		base.setMaterial(null);
		notifyBatchChanged();
	}
	public void syncOwnerColor(Color color)
	{
		base.setColor(color);
	}
	public void markRunGeometryDirty()
	{
		markVertexDirty(FastUIDirtyFlags.Geometry);
	}
	public override void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		builder.Clear();
		if (mOwner != null)
		{
			mOwner.buildRenderRunGeometry(mRunIndex, builder, canvasWorldToLocal);
		}
	}
	protected override void OnDestroy()
	{
		releaseMaterialVariant();
		mOwner = null;
		base.OnDestroy();
	}
	private void releaseMaterialVariant()
	{
		mBaseMaterial = null;
		mAtlasTexture = null;
	}
}
