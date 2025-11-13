using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MathUtility;
using static StringUtility;
using static FrameBaseHotFix;
using static UnityUtility;

// 对UGUI的Text的封装
public class myUGUIText : myUGUIObject, IUGUIText
{
	protected Text mText;				// UGUI的Text组件
	protected CanvasGroup mCanvasGroup; // 用于是否显示
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mText))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个Text组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mText = mObject.AddComponent<Text>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public override void destroy()
	{
		if (mCanvasGroup != null)
		{
			mCanvasGroup.alpha = 1.0f;
			mCanvasGroup = null;
		}
		base.destroy();
	}
	// 是否剔除渲染
	public void cull(bool isCull)
	{
		if (mCanvasGroup == null)
		{
			mCanvasGroup = getOrAddUnityComponent<CanvasGroup>();
		}
		mCanvasGroup.alpha = isCull ? 0.0f : 1.0f;
	}
	public override bool isCulled() { return mCanvasGroup != null && isFloatZero(mCanvasGroup.alpha); }
	public override bool canUpdate() { return base.canUpdate() && !isCulled(); }
	public override bool canGenerateDepth() { return !isCulled(); }
	public void setText(string text)
	{
		setText(text, false);
	}
	public void setText(string text, bool preferredHeight)
	{
		if (mText == null)
		{
			return;
		}
		if(text == null)
		{
			mText.text = EMPTY;
		}
		else if (mText.text != text)
		{
			mText.text = text;
		}
		if (preferredHeight)
		{
			applyPreferredHeight();
		}
	}
	public void setText(int value)
	{
		setText(IToS(value));
	}
	public void applyPreferredWidth(float height = 0.0f)
	{
		if (height <= 0.0f)
		{
			height = getWindowSize().y;
		}
		else
		{
			// 如果要改变文本区域的宽度,则需要先修改一次窗口大小,使之根据指定的宽度重新计算preferredHeight
			setWindowSize(new(getWindowSize().x, height));
		}
		setWindowSize(new(mText.preferredWidth, height));
	}
	public void applyPreferredHeight(float width = 0.0f)
	{
		if (width <= 0.0f)
		{
			width = getWindowSize().x;
		}
		else
		{
			// 如果要改变文本区域的宽度,则需要先修改一次窗口大小,使之根据指定的宽度重新计算preferredHeight
			setWindowSize(new(width, getWindowSize().y));
		}
		setWindowSize(new(width, mText.preferredHeight));
	}
	public void applyPreferredHeightKeepTop()
	{
		float oldHeight = getWindowSize().y;
		setWindowSize(new(getWindowSize().x, mText.preferredHeight));
		setPositionY(getPosition().y + (oldHeight - getWindowSize().y) * 0.5f);
	}
	public float getPreferredWidth() { return mText.preferredWidth; }
	public float getPreferredHeight() { return mText.preferredHeight; }
	public string getText() { return mText.text; }
	public override float getAlpha() { return mText.color.a; }
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		Color color = mText.color;
		color.a = alpha;
		mText.color = color;
	}
	public override void setColor(Color color) { mText.color = color; }
	public override Color getColor() { return mText.color; }
	public int getFontSize() { return mText.fontSize; }
	public void setFontSize(int fontSize) { mText.fontSize = fontSize; }
	public Font getFont() { return mText.font; }
	public void setAlignment(TextAnchor textAnchor) { mText.alignment = textAnchor; }
	public Text getTextComponent() { return mText; }
	// 设置可自动本地化的文本内容,collection是myUGUIText对象所属的布局对象或者布局结构体对象,如LayoutScript或WindowObjectUGUI
	public void setText(string mainText, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, string param, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, param);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, string param0, string param1, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, param0, param1);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, string param0, string param1, string param2, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, param0, param1, param2);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, string param0, string param1, string param2, string param3, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, param0, param1, param2, param3);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, Span<string> param, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, param);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, IList<string> paramList, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, paramList);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, LocalizationCallback callback, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, callback);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, string param, LocalizationCallback callback, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, param, callback);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, string param0, string param1, LocalizationCallback callback, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, param0, param1, callback);
		collection.addLocalizationObject(this);
	}
	public void setText(string mainText, IList<string> paramList, LocalizationCallback callback, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, mainText, paramList, callback);
		collection.addLocalizationObject(this);
	}
	public void setText(int textID, ILocalizationCollection collection)
	{
		mLocalizationManager.registeLocalization(this, textID);
		collection.addLocalizationObject(this);
	}
}