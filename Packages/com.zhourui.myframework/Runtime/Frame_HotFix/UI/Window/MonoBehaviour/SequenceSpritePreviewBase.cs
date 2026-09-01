using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using static MathUtility;
using static FrameBaseUtility;
using static FrameDefine;

// 预览一个Image或者SpriteRenderer的序列帧,不带位置偏移,不要直接将这个组件添加到GameObject上,应该需要添加派生出的子类
public abstract class SequenceSpritePreviewBase : MonoBehaviour
{
#if UNITY_EDITOR
	[Range(0, 1)]
	public float mSlider;
	[SerializeField] private Sprite[] mFrames;
	private int mCurFrame;
	private Sprite mCurSprite;
	public virtual void Awake()
	{
		enabled = !Application.isPlaying;
	}
	public void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}
		if (mCurSprite != getSprite())
		{
			mCurSprite = getSprite();
			reloadFrames();
			mCurFrame = mFrames.find(mCurSprite);
			syncSliderByCurFrame();
			refreshImage();
		}
	}
	public void OnValidate()
	{
		if (Application.isPlaying)
		{
			return;
		}
		reloadFrames();
		if (!mFrames.isEmpty())
		{
			refreshImage();
		}
	}
	public void EditorReloadFrames()
	{
		reloadFrames();
		refreshImage();
	}
	public void EditorPreviousFrame()
	{
		if (mFrames.isEmpty())
		{
			reloadFrames();
		}
		if (mFrames.isEmpty())
		{
			return;
		}
		mCurFrame = Mathf.Max(0, mCurFrame - 1);
		syncSliderByCurFrame();
		applyFrame(mCurFrame);
	}
	public void EditorNextFrame()
	{
		if (mFrames.isEmpty())
		{
			reloadFrames();
		}
		if (mFrames.isEmpty())
		{
			return;
		}
		mCurFrame = Mathf.Min(mFrames.Length - 1, mCurFrame + 1);
		syncSliderByCurFrame();
		applyFrame(mCurFrame);
	}
	public void EditorRefreshBySlider() { refreshImage(); }
	public int EditorGetFrameCount() { return mFrames.count(); }
	public int EditorGetCurFrame() { return mCurFrame; }
	public static void setImage(Component component, Sprite sprite)
	{
		if (component == null)
		{
			return;
		}

		if (component is Image image)
		{
			image.sprite = sprite;
			EditorUtility.SetDirty(image);
		}
		else if (component is SpriteRenderer renderer)
		{
			renderer.sprite = sprite;
			EditorUtility.SetDirty(renderer);
		}
	}
	public Sprite getSprite()
	{
		return getImage(getSpriteComponent());
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
		if (mFrames.isEmpty())
		{
			reloadFrames();
		}

		if (mFrames.isEmpty())
		{
			return;
		}

		int frameCount = mFrames.Length;
		int index = (mSlider * frameCount).ceil() - 1;
		mCurFrame = index.clamp(0, frameCount - 1);
		applyFrame(mCurFrame);
	}
	private void reloadFrames()
	{
		Sprite curSprite = getSprite();
		if (curSprite == null)
		{
			return;
		}

		string path = getAssetPath(curSprite.texture);
		if (path.isEmpty())
		{
			return;
		}

		string spriteSetName = getSpriteSetName(curSprite.name);
		if (spriteSetName.isEmpty())
		{
			return;
		}

		IEnumerable<Sprite> sprites = null;
		if (path.endWith(SPRITE_ATLAS_SUFFIX))
		{
			SpriteAtlas atlas = loadAssetAtPath<SpriteAtlas>(path);
			if (atlas == null)
			{
				return;
			}

			using var a = new ListScope<Sprite>(out var spriteList);
			collectAtlasSprites(atlas, spriteList);
			sprites = spriteList.ToArray();
		}
		else if (path.endWith(".png"))
		{
			sprites = loadAllAssetsAtPath(path).OfType<Sprite>();
		}

		if (sprites == null)
		{
			mFrames = null;
			return;
		}

		mFrames = sprites
			.Where(s => s != null && s.name.StartsWith(spriteSetName + "_"))
			.OrderBy(getSpriteFrameIndex)
			.ThenBy(s => s.name)
			.ToArray();

		mCurFrame = mCurFrame.clamp(0, getMax(0, mFrames.Length - 1));
	}
	private static void collectAtlasSprites(SpriteAtlas atlas, List<Sprite> spriteList)
	{
		Object[] packables = UnityEditor.U2D.SpriteAtlasExtensions.GetPackables(atlas);
		if (packables == null)
		{
			return;
		}

		foreach (Object packable in packables)
		{
			if (packable == null)
			{
				continue;
			}

			if (packable is Sprite sprite)
			{
				spriteList.addIf(sprite, !spriteList.Contains(sprite));
				continue;
			}
			string path = AssetDatabase.GetAssetPath(packable);
			if (path.isEmpty())
			{
				continue;
			}
			if (AssetDatabase.IsValidFolder(path))
			{
				foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { path }))
				{
					string texturePath = AssetDatabase.GUIDToAssetPath(guid);
					foreach (Sprite item in AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>())
					{
						spriteList.addIf(item, !spriteList.Contains(item));
					}
				}
			}
			else
			{
				foreach (Sprite item in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
				{
					spriteList.addIf(item, !spriteList.Contains(item));
				}
			}
		}
	}
	private void applyFrame(int index)
	{
		if (!TryGetComponent<ImageAtlasPath>(out _))
		{
			Debug.LogError("需要添加ImageAtlasPath组件, gameObject:" + name, this);
		}

		Component image = getSpriteComponent();
		if (image == null || mFrames.isEmpty())
		{
			return;
		}
		setImage(image, mFrames[index.clamp(0, mFrames.Length - 1)]);
		EditorUtility.SetDirty(this);
		SceneView.RepaintAll();
	}
	private void syncSliderByCurFrame()
	{
		if (mFrames.isEmpty() || mFrames.Length <= 1)
		{
			mSlider = 0.0f;
			return;
		}
		mSlider = mCurFrame / (float)(mFrames.Length - 1);
	}
	private static int getSpriteFrameIndex(Sprite sprite)
	{
		if (sprite == null || sprite.name.isEmpty())
		{
			return int.MaxValue;
		}

		string spriteName = sprite.name;
		int index = spriteName.LastIndexOf('_');
		if (index < 0 || index >= spriteName.Length - 1)
		{
			return int.MaxValue;
		}

		string indexString = spriteName[(index + 1)..];
		if (int.TryParse(indexString, out int frameIndex))
		{
			return frameIndex;
		}
		return int.MaxValue;
	}
#endif
}