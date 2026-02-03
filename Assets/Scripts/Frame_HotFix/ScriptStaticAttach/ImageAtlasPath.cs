using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using System.Collections.Generic;
using static FrameBaseUtility;
using static FrameBaseDefine;
using static StringUtility;
using static FileUtility;
using static UnityUtility;
using static FrameDefine;

// 用于记录Image组件上的图片所在的路径,因为在运行时是没办法获得Image上图片的路径,从而也就无法直到所在的图集
// 所以使用一个组件来在编辑模式下就记录路径
// 兼容TPAtlas和UGUIAtlas
[ExecuteInEditMode]
public class ImageAtlasPath : MonoBehaviour
{
	[HideInInspector]
	public Sprite mSprite;				// 记录设置的Sprite,用于对比当前GameObject上的Sprite有没有被修改
	public string mAtlasPath;			// 记录的图集路径
	public bool Refresh;				// 刷新标记,用于在面板上手动刷新,因为如果使用SpriteAtlas时自动刷新太耗时,使编辑器变得异常卡顿,所以需要手动刷新
	public void Awake()
	{
		enabled = !Application.isPlaying;
		refresh(true);
	}
	public void OnEnable()
	{
		refresh(true);
	}
	public void Update()
	{
		if (isEditor() && !Application.isPlaying)
		{
			Sprite newSprite = getSprite();
			bool force = newSprite != mSprite;
			mSprite = newSprite;
			refresh(force);
		}
	}
	public bool refresh(bool force = false, Dictionary<string, SpriteAtlas> atlasListCache = null)
	{
		if (Application.isPlaying)
		{
			return false;
		}
		if (!Refresh && !force)
		{
			return false;
		}
		Refresh = false;
		return setAtlasPathInternal(getAtlasPath(getSprite(), atlasListCache));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// atlasListCache是提前加载好的SpriteAtlas的列表,用于加速,key是SpriteAtlas的以Assets开头的路径,带后缀
	protected string getAtlasPath(Sprite sprite, Dictionary<string, SpriteAtlas> atlasListCache = null)
	{
		if (sprite == null)
		{
			return "";
		}
		// 如果图集名字和图片名字一样,则说明是Single的Sprite
		if (sprite.texture.name != sprite.name)
		{
			return getAssetPath(sprite.texture);
		}
		// 先检查当前的图集是否正确
		if (!mAtlasPath.isEmpty())
		{
			SpriteAtlas curAtlas = atlasListCache?.get(mAtlasPath);
			if (curAtlas == null)
			{
				curAtlas = loadAssetAtPath<SpriteAtlas>(mAtlasPath);
			}
			if (curAtlas.isSpriteInAtlas(sprite))
			{
				return mAtlasPath;
			}
		}

		if (atlasListCache.count() > 0)
		{
			foreach (var item in atlasListCache)
			{
				if (item.Value.isSpriteInAtlas(sprite))
				{
					return item.Key;
				}
			}
		}
		else
		{
			List<string> files = new();
			findFiles(F_ASSETS_PATH, files, SPRITE_ATLAS_SUFFIX);
			foreach (string file in files)
			{
				if (loadAssetAtPath<SpriteAtlas>(fullPathToProjectPath(file)).isSpriteInAtlas(sprite))
				{
					return fullPathToProjectPath(file);
				}
			}
		}
		return "";
	}
	protected bool setAtlasPathInternal(string path)
	{
		Sprite sprite = getSprite();
		if (sprite != null && path.isEmpty())
		{
			Debug.LogError("找不到图片所属的图集:" + sprite.name + ", GameObject:" + getGameObjectPath(gameObject));
		}
		if (mAtlasPath != path && !path.isEmpty())
		{
			mAtlasPath = path;
			setDirty(gameObject);
			return true;
		}
		return false;
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
}