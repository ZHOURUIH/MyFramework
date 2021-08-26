#if USE_TMP
using UnityEngine;
using System.Collections;
using TMPro;

// 对TextMeshPro的Text组件的封装
public class myUGUITextTMP : myUGUIObject
{
	protected TextMeshProUGUI mText;	// TextMeshPro的Text组件
	public override void init()
	{
		base.init();
		mText = mObject.GetComponent<TextMeshProUGUI>();
		if (mText == null)
		{
			mText = mObject.AddComponent<TextMeshProUGUI>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mText == null)
		{
			logError(Typeof(this) + " can not find " + typeof(TextMeshProUGUI) + ", window:" + mName + ", layout:" + mLayout.getName());
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
	public void setAlignment(TextAlignmentOptions textAnchor) { mText.alignment = textAnchor; }
}
#endif