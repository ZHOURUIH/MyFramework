using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class txUGUIText : txUGUIObject
{
	protected Text mText;
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
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
			logError(GetType() + " can not find " + typeof(Text) + ", window:" + mName + ", layout:" + mLayout.getName());
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
		setText(intToString(value));
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
	public void setColor(Color color) { mText.color = color; }
	public Color getColor() { return mText.color; }
	public int getFontSize() { return mText.fontSize; }
	public void setFontSize(int fontSize) { mText.fontSize = fontSize; }
	public Font getFont() { return mText.font; }
	public void setAlignment(TextAnchor textAnchor) { mText.alignment = textAnchor; }
	public Text getTextComponent() { return mText; }
}