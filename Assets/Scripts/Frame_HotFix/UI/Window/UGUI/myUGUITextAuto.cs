using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using static StringUtility;
using static FrameBaseHotFix;

// 如果节点上有Text组件,则使用Text,如果有TextMeshPro组件,就使用TextMeshPro
public class myUGUITextAuto : myUGUIObject, IUGUIText
{
	protected TextMeshProUGUI mTextPro;		// TextMeshPro的Text组件
	protected Text mText;					// UGUI的Text组件
	protected CanvasGroup mCanvasGroup;     // 用于是否显示
	public override void init()
	{
		base.init();
		mObject.TryGetComponent(out mTextPro);
		mObject.TryGetComponent(out mText);
	}
	public void setText(string text)
	{
		if (mTextPro != null)
		{
			if (mTextPro.text != text)
			{
				mTextPro.text = text;
			}
		}
		else if (mText != null)
		{
			if (mText.text != text)
			{
				mText.text = text;
			}
		}
	}
	public void setTextWithPreferredWidth(string text, float extraWidth = 0.0f)
	{
		setText(text);
		applyPreferredWidth(0.0f, extraWidth);
	}
	public void setTextWithPreferredHeight(string text, float extraHeight = 0.0f)
	{
		setText(text);
		applyPreferredHeight(0.0f, extraHeight);
	}
	public void setText(int value)
	{
		setText(IToS(value));
	}
	public void setText(long value)
	{
		setText(LToS(value));
	}
	public void applyPreferredWidth(float height = 0.0f, float extraWidth = 0.0f)
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
		float preferredWidth = 0.0f;
		if (mTextPro != null)
		{
			preferredWidth = mTextPro.preferredWidth;
		}
		else if (mText != null)
		{
			preferredWidth = mText.preferredWidth;
		}
		setWindowSize(new(preferredWidth + extraWidth, height));
	}
	public void applyPreferredHeight(float width = 0.0f, float extraHeight = 0.0f)
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
		float preferredHeight = 0.0f;
		if (mTextPro != null)
		{
			preferredHeight = mTextPro.preferredHeight;
		}
		else if (mText != null)
		{
			preferredHeight = mText.preferredHeight;
		}
		setWindowSize(new(width, preferredHeight + extraHeight));
	}
	public string getText() 
	{
		if (mTextPro != null)
		{
			return mTextPro.text;
		}
		else if (mText != null)
		{
			return mText.text;
		}
		return EMPTY;
	}
	public override float getAlpha() 
	{
		if (mTextPro != null)
		{
			return mTextPro.color.a;
		}
		else if (mText != null)
		{
			return mText.color.a;
		}
		return 1.0f; 
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		if (mTextPro != null)
		{
			Color color = mTextPro.color;
			color.a = alpha;
			mTextPro.color = color;
		}
		else if (mText != null)
		{
			Color color = mText.color;
			color.a = alpha;
			mText.color = color;
		}
	}
	public override void setColor(Color color) 
	{
		if (mTextPro != null)
		{
			mTextPro.color = color;
		}
		else if (mText != null)
		{
			mText.color = color;
		}
	}
	public override Color getColor() 
	{
		if (mTextPro != null)
		{
			return mTextPro.color;
		}
		else if (mText != null)
		{
			return mText.color;
		}
		return Color.white; 
	}
	public float getFontSize() 
	{
		if (mTextPro != null)
		{
			return mTextPro.fontSize;
		}
		else if (mText != null)
		{
			return mText.fontSize;
		}
		return 20;
	}
	public void setFontSize(float fontSize) 
	{
		if (mTextPro != null)
		{
			mTextPro.fontSize = fontSize;
		}
		else if (mText != null)
		{
			mText.fontSize = (int)fontSize;
		}
	}
	public TMP_FontAsset getTMPFont() 
	{
		if (mTextPro != null)
		{
			return mTextPro.font;
		}
		return null;
	}
	public Font getFont()
	{
		if (mText != null)
		{
			return mText.font;
		}
		return null;
	}
	public float getPreferredWidth() 
	{
		if (mTextPro != null)
		{
			return mTextPro.preferredWidth;
		}
		else if (mText != null)
		{
			return mText.preferredWidth;
		}
		return 0.0f;
	}
	public float getPreferredHeight() 
	{
		if (mTextPro != null)
		{
			return mTextPro.preferredHeight;
		}
		else if (mText != null)
		{
			return mText.preferredHeight;
		}
		return 0.0f;
	}
	public void setAlignment(TextAlignmentOptions textAnchor) 
	{
		if (mTextPro != null)
		{
			mTextPro.alignment = textAnchor;
		}
	}
	public void setAlignment(TextAnchor textAnchor)
	{
		if (mText != null)
		{
			mText.alignment = textAnchor;
		}
	}
	// 是否剔除渲染
	public void cull(bool isCull)
	{
		if (mText != null)
		{
			if (mCanvasGroup == null)
			{
				mCanvasGroup = getOrAddUnityComponent<CanvasGroup>();
			}
			mCanvasGroup.alpha = isCull ? 0.0f : 1.0f;
		}
		else
		{
			Color color = mTextPro.color;
			color.a = isCull ? 0.0f : 1.0f;
			mTextPro.color = color;
		}
	}
	public TextMeshProUGUI getTextProComponent()  { return mTextPro; }
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
