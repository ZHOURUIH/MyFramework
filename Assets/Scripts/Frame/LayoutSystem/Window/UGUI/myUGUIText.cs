﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class myUGUIText : myUGUIObject
{
	protected Text mText;
	public override void init()
	{
		base.init();
		mText = mObject.GetComponent<Text>();
		if (mText == null)
		{
			mText = mObject.AddComponent<Text>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mText == null)
		{
			logError(Typeof(this) + " can not find " + Typeof<Text>() + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public void setText(string text, bool preferredHeight = false)
	{
		if (mText.text != text)
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
		setText(intToString(value));
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
			setWindowSize(new Vector2(width, getWindowSize().y));
		}
		setWindowSize(new Vector2(width, mText.preferredHeight));
	}
	public float getPrefferredWidth() { return mText.preferredWidth; }
	public float getPrefferredHeight() { return mText.preferredHeight; }
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
}