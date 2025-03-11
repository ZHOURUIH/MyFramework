﻿#if USE_TMP
using UnityEngine;
using TMPro;
using static StringUtility;

// 对TextMeshPro的Text组件的封装
public class myUGUITextTMP : myUGUIObject
{
	protected TextMeshProUGUI mText;	// TextMeshPro的Text组件
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mText))
		{
			mText = mObject.AddComponent<TextMeshProUGUI>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public void setText(string text)
	{
		if (mText.text != text)
		{
			mText.text = text;
		}
	}
	public void setText(int value)
	{
		setText(IToS(value));
	}
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
	public float getFontSize() { return mText.fontSize; }
	public void setFontSize(float fontSize) { mText.fontSize = fontSize; }
	public TMP_FontAsset getFont() { return mText.font; }
	public float getPreferredWidth() { return mText.preferredWidth; }
	public float getPreferredHeight() { return mText.preferredHeight; }
	public void setAlignment(TextAlignmentOptions textAnchor) { mText.alignment = textAnchor; }
}
#endif