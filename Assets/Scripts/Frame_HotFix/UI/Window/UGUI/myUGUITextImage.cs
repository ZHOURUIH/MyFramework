﻿using static UnityUtility;

// 可实现图文混排的文本
// <quad width=xxx sprite=xxx/>
public class myUGUITextImage : myUGUIObject
{
	protected WindowPool<myUGUIImage> mImagePool;	// 图片节点对象池
    protected TextImage mTextImage;					// 处理图文混排的组件,继承自Text
	protected myUGUIImage mImage;					// 当前文本节点下的Image节点,用于获得图集信息,以及克隆图片
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mTextImage))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个TextImage组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mTextImage = mObject.AddComponent<TextImage>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}

		// 自动获取该节点下的名为Image的子节点
		mLayout.getScript().newObject(out mImage, this, "Image", false);
		if (mImage == null)
		{
			logError("可图文混排的文本下必须有一个名为Image的子节点");
		}

		// 初始化图片模板信息相关
		mImagePool = new(mLayout.getScript());
		mImagePool.assignTemplate(mImage);
		mImagePool.init(false);
		mTextImage.setCreateImage(()=> { return mImagePool.newWindow(); });
		mTextImage.setDestroyImage((myUGUIImage image)=> { mImagePool.unuseWindow(image); });
	}
	public TextImage getTextImage() { return mTextImage; }
}