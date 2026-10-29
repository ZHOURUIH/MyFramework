using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static FrameUtility;

// 图文混排：<quad width=1 sprite=名称/>，动态图片可附加 fps=帧率；<link=标识>文字</link>。
public class myUGUITextImage : myUGUIObject
{
	protected WindowPool<myUGUIImage> mImagePool;
	protected WindowPool<myUGUIImageAnim> mImageAnimPool;
	protected TextImage mTextImage;
	protected TextImageTMP mTextImageTMP;
	protected TextMeshProUGUI mTMPText;
	protected myUGUIImage mImage;
	protected myUGUIImageAnim mImageAnim;
	protected AtlasRef mImageAtlas;					// 静态图片持有的图集引用
	protected AtlasRef mImageAnimAtlas;				// 动态图片持有的图集引用
	protected List<TextImageLinkArea> mLinkAreas = new();
	protected List<myUGUITextImageLink> mLinkObjects = new();
	protected StringCallback mLinkCallback;
	protected bool mLinkAreasDirty;
	public override void init()
	{
		base.init();
		if (mObject.TryGetComponent(out mTextImageTMP))
		{
			mObject.TryGetComponent(out mTMPText);
		}
		else if (!mObject.TryGetComponent(out mTextImage))
		{
			if (!mIsNewObject)
			{
				logError("需要添加TextImage或TextImageTMP组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mTextImage = mObject.AddComponent<TextImage>();
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
		mLayout.getScript().newObject(out mImage, this, "Image", false);
		if (mImage == null)
		{
			logError("可图文混排的文本下必须有一个名为Image的子节点");
			return;
		}
		mImagePool = new(mLayout.getScript());
		mImagePool.assignTemplate(mImage);
		mImagePool.init();
		GameObject imageAnimObject = cloneObject(mImage.getGameObject(), "ImageAnim");
		mImageAnim = LayoutScript.newUIObject<myUGUIImageAnim>(this, mLayout, imageAnimObject, true);
		mImageAnimPool = new(mLayout.getScript());
		mImageAnimPool.assignTemplate(mImageAnim);
		mImageAnimPool.init();
		if (mTextImageTMP != null)
		{
			mTextImageTMP.setCreateImageAnim(createImageAnim);
			mTextImageTMP.setDestroyImageAnim(destroyImageAnim);
			mTextImageTMP.setCreateImage(createImage);
			mTextImageTMP.setDestroyImage(destroyImage);
			mTextImageTMP.setLinkLayoutChanged(onLinkLayoutChanged);
		}
		else
		{
			mTextImage.setCreateImageAnim(createImageAnim);
			mTextImage.setDestroyImageAnim(destroyImageAnim);
			mTextImage.setCreateImage(createImage);
			mTextImage.setDestroyImage(destroyImage);
			mTextImage.setLinkLayoutChanged(onLinkLayoutChanged);
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mLinkAreasDirty && mLinkCallback != null)
		{
			refreshLinkAreas();
		}
	}
	public override void destroy()
	{
		mTextImageTMP?.setLinkLayoutChanged(null);
		mTextImage?.setLinkLayoutChanged(null);
		mAtlasManager.unloadAtlas(ref mImageAtlas);
		mAtlasManager.unloadAtlas(ref mImageAnimAtlas);
		base.destroy();
	}
	// 设置所有静态图片的图集,组件持有独立引用,传入null时恢复初始图集。
	public void setImageAtlas(AtlasRef atlas)
	{
		if (atlas != null && !atlas.isValid())
		{
			logError("静态图片图集无效");
			return;
		}
		AtlasBase target = atlas?.getAtlas();
		if (mImageAtlas?.getAtlas() == target)
		{
			return;
		}
		AtlasRef next = null;
		if (target != null)
		{
			CLASS(out next);
			next.setAtlas(target);
		}
		AtlasRef old = mImageAtlas;
		mImageAtlas = next;
		AtlasRef current = mImageAtlas ?? mImage.getAtlas();
		foreach (myUGUIImage image in mImagePool.getWindowList())
		{
			image.setAtlas(current);
		}
		foreach (myUGUIImage image in mImagePool.getUnusedList())
		{
			image.setAtlas(current);
		}
		mTextImage?.refreshImageAtlas();
		mTextImageTMP?.refreshImageAtlas();
		mAtlasManager.unloadAtlas(ref old);
	}
	// 设置所有动态图片的图集,组件持有独立引用,传入null时恢复初始图集。
	public void setImageAnimAtlas(AtlasRef atlas)
	{
		if (atlas != null && !atlas.isValid())
		{
			logError("动态图片图集无效");
			return;
		}
		AtlasBase target = atlas?.getAtlas();
		if (mImageAnimAtlas?.getAtlas() == target)
		{
			return;
		}
		AtlasRef next = null;
		if (target != null)
		{
			CLASS(out next);
			next.setAtlas(target);
		}
		AtlasRef old = mImageAnimAtlas;
		mImageAnimAtlas = next;
		AtlasRef current = mImageAnimAtlas ?? mImageAnim.getAtlas();
		foreach (myUGUIImageAnim image in mImageAnimPool.getWindowList())
		{
			updateImageAnimAtlas(image, current);
		}
		foreach (myUGUIImageAnim image in mImageAnimPool.getUnusedList())
		{
			updateImageAnimAtlas(image, current);
		}
		mTextImage?.refreshImageAnimAtlas();
		mTextImageTMP?.refreshImages();
		mAtlasManager.unloadAtlas(ref old);
	}
	// 在指定位置显示图片,用于输入框等由外部计算图片位置的场景。
	public void setImage(int index, string sprite, Vector2 position, float size)
	{
		List<myUGUIImage> images = mImagePool.getWindowList();
		myUGUIImage image = index < images.Count ? images[index] : createImage();
		if (image.getSpriteName() != sprite)
		{
			image.setSpriteName(sprite);
		}
		image.setPosition(position);
		image.setSize(new(size, size));
	}
	public void setImageAnim(int index, string sprite, float fps, Vector2 position, float size)
	{
		List<myUGUIImageAnim> images = mImageAnimPool.getWindowList();
		myUGUIImageAnim image = index < images.Count ? images[index] : createImageAnim();
		if (image.getTextureSet() != sprite)
		{
			image.setTextureSet(sprite);
			image.setLoop(LOOP_MODE.LOOP);
			image.setInterval(1.0f / fps);
		}
		if (image.getPlayState() != PLAY_STATE.PLAY)
		{
			image.play();
		}
		image.setPosition(position);
		image.setSize(new(size, size));
	}
	public void setImageCount(int count)
	{
		List<myUGUIImage> images = mImagePool.getWindowList();
		while (images.Count > count)
		{
			destroyImage(images[images.Count - 1]);
		}
	}
	public void setImageAnimCount(int count)
	{
		List<myUGUIImageAnim> images = mImageAnimPool.getWindowList();
		while (images.Count > count)
		{
			destroyImageAnim(images[images.Count - 1]);
		}
	}
	public void setText(string text)
	{
		if (getText() != text)
		{
			clearLinkAreas();
		}
		if (mTextImageTMP != null)
		{
			mTextImageTMP.setText(text);
		}
		else
		{
			mTextImage.text = text;
		}
	}
	public string getText() { return mTextImageTMP != null ? mTextImageTMP.getText() : mTextImage.getSourceText(); }
	public float getPreferredWidth() { return mTMPText != null ? mTMPText.preferredWidth : mTextImage.preferredWidth; }
	public float getPreferredHeight() { return mTMPText != null ? mTMPText.preferredHeight : mTextImage.preferredHeight; }
	public void applyPreferredHeight(float width)
	{
		if (width <= 0.0f)
		{
			width = getSize().x;
		}
		else
		{
			setSize(new(width, getSize().y));
		}
		setSize(new(width, getPreferredHeight()));
		mTextImageTMP?.refreshImages();
	}
	public void clearImages()
	{
		if (mTextImageTMP != null)
		{
			mTextImageTMP.clearImages();
		}
		else
		{
			mTextImage.text = string.Empty;
		}
	}
	public override float getAlpha() { return getColor().a; }
	public override void setAlpha(float alpha)
	{
		Color color = getColor();
		color.a = alpha;
		setColor(color);
	}
	public override Color getColor() { return mTMPText != null ? mTMPText.color : mTextImage.color; }
	public override void setColor(Color color)
	{
		if (mTMPText != null)
		{
			mTMPText.color = color;
		}
		else
		{
			mTextImage.color = color;
		}
	}
	public void setLinkCallback(StringCallback callback)
	{
		clearLinkAreas();
		mLinkCallback = callback;
		if (callback == null)
		{
			return;
		}
		setNeedUpdate(true);
		mLinkAreasDirty = true;
	}
	public TextImage getTextImage() { return mTextImage; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLinkLayoutChanged()
	{
		if (mLinkCallback != null)
		{
			mLinkAreasDirty = true;
		}
	}
	protected void clearLinkAreas()
	{
		mLinkAreasDirty = false;
		foreach (myUGUITextImageLink link in mLinkObjects)
		{
			link.setLink(null, null);
			link.setActive(false);
		}
	}
	protected void refreshLinkAreas()
	{
		mLinkAreasDirty = false;
		mLinkAreas.Clear();
		if (mTextImageTMP != null)
		{
			mTextImageTMP.getLinkAreas(mLinkAreas);
		}
		else
		{
			mTextImage.getLinkAreas(mLinkAreas);
		}
		for (int i = 0; i < mLinkAreas.Count; ++i)
		{
			if (i >= mLinkObjects.Count)
			{
				mLinkObjects.Add(mLayout.getScript().createUGUIObject<myUGUITextImageLink>(this, "Link", true));
			}
			TextImageLinkArea area = mLinkAreas[i];
			myUGUITextImageLink link = mLinkObjects[i];
			link.setLink(area.mID, mLinkCallback);
			link.setPosition(area.mRect.center);
			link.setSize(area.mRect.size);
			link.setActive(true);
		}
		for (int i = mLinkAreas.Count; i < mLinkObjects.Count; ++i)
		{
			mLinkObjects[i].setLink(null, null);
			mLinkObjects[i].setActive(false);
		}
	}
	protected myUGUIImage createImage()
	{
		myUGUIImage image = mImagePool.newItem();
		if (mImageAtlas != null)
		{
			image.setAtlas(mImageAtlas);
		}
		return image;
	}
	protected void destroyImage(myUGUIImage image)
	{
		mImagePool.unuseItem(image);
	}
	protected myUGUIImageAnim createImageAnim()
	{
		myUGUIImageAnim image = mImageAnimPool.newItem();
		if (mImageAnimAtlas != null)
		{
			updateImageAnimAtlas(image, mImageAnimAtlas);
		}
		return image;
	}
	protected void updateImageAnimAtlas(myUGUIImageAnim image, AtlasRef atlas)
	{
		if (image.getAtlas() == atlas)
		{
			return;
		}
		string textureSet = image.getTextureSet();
		PLAY_STATE state = image.getPlayState();
		int frame = image.getCurFrameIndex();
		image.setAtlas(atlas, false, true);
		if (!string.IsNullOrEmpty(textureSet))
		{
			image.setTextureSet(textureSet);
			image.setCurFrameIndex(frame);
			if (state == PLAY_STATE.PLAY)
			{
				image.play();
			}
			else if (state == PLAY_STATE.PAUSE)
			{
				image.pause();
			}
		}
	}
	protected void destroyImageAnim(myUGUIImageAnim image)
	{
		image.stop();
		mImageAnimPool.unuseItem(image);
	}
}