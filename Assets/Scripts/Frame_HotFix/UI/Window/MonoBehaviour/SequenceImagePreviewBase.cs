#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static FrameDefine;
using static MathUtility;

// 预览一个Image或者SpriteRenderer的序列帧,不带位置偏移,不要直接将这个组件添加到GameObject上,应该需要添加派生出的子类
public class SequenceImagePreviewBase : MonoBehaviour
{
	[Range(0, 1)]
	public float mSlider;
	public virtual void Awake()
	{
		enabled = !Application.isPlaying;
	}
	public virtual void Update()
	{
		refreshImage();
	}
	public static void setImage(Component component, Sprite sprite)
	{
		if (component is Image image)
		{
			image.sprite = sprite;
		}
		else if (component is SpriteRenderer renderer)
		{
			renderer.sprite = sprite;
		}
	}
	public static Sprite getImage(Component component)
	{
		if (component is Image image)
		{
			return image.sprite;
		}
		else if (component is SpriteRenderer renderer)
		{
			return renderer.sprite;
		}
		return null;
	}
	public static string getSpriteSetName(string spriteName)
	{
		if (spriteName.isEmpty())
		{
			return "";
		}
		int index = spriteName.LastIndexOf('_');
		if (index < 0)
		{
			return "";
		}
		return spriteName.startString(index);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual Component getSpriteComponent() { return null; }
	protected void refreshImage()
	{
		if (!TryGetComponent<ImageAtlasPath>(out _))
		{
			Debug.LogError("需要添加ImageAtlasPath组件");
		}
		Component image = getSpriteComponent();
		if (image == null)
		{
			return;
		}
		Sprite curSprite = getImage(image);
		if (curSprite == null)
		{
			return;
		}
		string spriteSetName = getSpriteSetName(curSprite.name);
		string pathName = AssetDatabase.GetAssetPath(curSprite.texture).removeStartString(P_GAME_RESOURCES_PATH);
		Dictionary<string, Sprite> spriteList = new();
		foreach (Object item in AssetDatabase.LoadAllAssetsAtPath(P_GAME_RESOURCES_PATH + pathName))
		{
			if (item is Sprite sprite && sprite.name.StartsWith(spriteSetName))
			{
				spriteList.Add(sprite.name, sprite);
			}
		}
		// 刷新当前图片
		int frameCount = spriteList.Count;
		if (!spriteSetName.isEmpty() && frameCount > 0)
		{
			int index = ceil(mSlider * frameCount) - 1;
			clamp(ref index, 0, frameCount - 1);
			string spriteName = spriteSetName + "_" + index;
			if (spriteList.TryGetValue(spriteName, out Sprite sprite))
			{
				setImage(image, sprite);
			}
			else
			{
				Debug.LogError("列表中找不到图片:" + spriteName + ", gameObject:" + image.name);
			}
		}
	}
}

#endif