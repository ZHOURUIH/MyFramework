using UnityEngine;
using UnityEngine.UI;
using static FrameEditorUtility;

// 用于记录Image组件上的材质所在的路径
// 所以使用一个组件来在编辑模式下就记录路径
[ExecuteInEditMode]
public class MaterialPath : MonoBehaviour
{
	public string mMaterialPath;       // 记录的材质路径
	public void OnValidate()
	{
		if (isEditor() && !Application.isPlaying)
		{
			refreshPath();
		}
	}
	public void Update()
	{
		if (isEditor() && !Application.isPlaying)
		{
			refreshPath();
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshPath()
	{
		Material material = getMaterial();
		if (material == null)
		{
			setMaterialPathInternal(string.Empty);
			return;
		}
		setMaterialPathInternal(getAssetPath(material));
	}
	protected void setMaterialPathInternal(string path)
	{
		if (mMaterialPath != path)
		{
			mMaterialPath = path;
			setDirty(gameObject);
		}
	}
	protected Material getMaterial()
	{
		if (TryGetComponent<Image>(out var image))
		{
			return image.material;
		}
		if (TryGetComponent<RawImage>(out var rawImage))
		{
			return rawImage.material;
		}
		if (TryGetComponent<SpriteRenderer>(out var com))
		{
			return com.sharedMaterial;
		}
		return null;
	}
}