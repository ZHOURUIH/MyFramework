using UnityEngine;
using UnityEngine.UI;

// 对UGUI的Text的封装
public class myUGUIText : myUGUIObject
{
	protected Text mText;				// UGUI的Text组件
	protected CanvasGroup mCanvasGroup; // 用于是否显示
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
			logError(Typeof(this) + " can not find " + typeof(Text) + ", window:" + mName + ", layout:" + mLayout.getName());
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
			mCanvasGroup = getUnityComponent<CanvasGroup>();
		}
		mCanvasGroup.alpha = isCull ? 0.0f : 1.0f;
	}
	public bool isCull() { return mCanvasGroup != null && isFloatZero(mCanvasGroup.alpha); }
	public void setText(MyStringBuilder text, bool preferredHeight = false)
	{
		setText(END_STRING(text), preferredHeight);
	}
	public void setText(string text, bool preferredHeight = false)
	{
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
}