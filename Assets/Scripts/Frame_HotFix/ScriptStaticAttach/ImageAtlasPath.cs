using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;
using static FrameEditorUtility;

// 用于记录Image组件上的图片所在的路径,因为在运行时是没办法获得Image上图片的路径,从而也就无法直到所在的图集
// 所以使用一个组件来在编辑模式下就记录路径
[ExecuteInEditMode]
public class ImageAtlasPath : MonoBehaviour
{
	public string mAtlasPath;       // 记录的图集路径
	public string mSpriteName;      // 当前使用的图片名字,图片无效时仍然记录上一次的名字
	public void OnValidate()
	{
		if (isEditor() && !Application.isPlaying)
		{
			refreshPath();
			refreshSpriteName();
		}
	}
	public void Update()
	{
		if (isEditor() && !Application.isPlaying)
		{
			refreshPath();
			refreshSpriteName();
		}
	}
	public void recover()
	{
		if (string.IsNullOrEmpty(mSpriteName) || string.IsNullOrEmpty(mAtlasPath))
		{
			return;
		}
		if (getSprite() != null)
		{
			return;
		}
		UObject[] sprites = loadAllAssetsAtPath(mAtlasPath);
		UObject atlasObject = loadMainAssetAtPath(mAtlasPath);
		UGUIAtlas atlas = AtlasLoaderBase.atlasLoaded(sprites, atlasObject, mAtlasPath);
		if (atlas != null && getSprite() != atlas.getSprite(mSpriteName))
		{
			setSprite(atlas.getSprite(mSpriteName));
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshSpriteName()
	{
		Sprite sprite = getSprite();
		if (sprite != null)
		{
			if ((mSpriteName.isEmpty() || mSpriteName != sprite.name) && !mAtlasPath.endWith("/unity_builtin_extra"))
			{
				setSpriteNameInternal(sprite.name);
			}
		}
		else
		{
			setSpriteNameInternal(string.Empty);
		}
	}
	protected void refreshPath()
	{
		Sprite sprite = getSprite();
		if (sprite == null)
		{
			setAtlasPathInternal(string.Empty);
			return;
		}
		Texture texture = sprite.texture;
		if (texture == null)
		{
			setAtlasPathInternal(string.Empty);
			return;
		}
		setAtlasPathInternal(getAssetPath(texture));
	}
	protected void setSpriteNameInternal(string spriteName)
	{
		if (mSpriteName != spriteName)
		{
			mSpriteName = spriteName;
			setDirty(gameObject);
		}
	}
	protected void setAtlasPathInternal(string path)
	{
		if (mAtlasPath != path)
		{
			mAtlasPath = path;
			setDirty(gameObject);
		}
	}
	protected Sprite getSprite()
	{
		if (TryGetComponent<Image>(out var image))
		{
			return image.sprite;
		}
		if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
		{
			return spriteRenderer.sprite;
		}
		return null;
	}
	protected void setSprite(Sprite sprite)
	{
		if (TryGetComponent<Image>(out var image))
		{
			image.sprite = sprite;
			return;
		}
		if (TryGetComponent<SpriteRenderer>(out var renderer))
		{
			renderer.sprite = sprite;
		}
	}
}