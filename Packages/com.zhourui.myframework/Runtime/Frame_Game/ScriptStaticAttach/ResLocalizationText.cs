using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static FrameBaseDefine;

// 非热更代码使用,需要手动将所有语言的文本都填进去,因为没办法读表
// 用于挂到静态的文本上,也就是只是在界面显示,不会在代码中访问和操作的文本
// 如果是会在代码中访问操作的文本对象则不需要挂此脚本
// 物体被隐藏时也不会注销多语言监听,只有对象被销毁时才会注销
[RequireComponent(typeof(Text))]
public class ResLocalizationText : MonoBehaviour
{
	protected float mFontSizeScale;								// 由于自适应而造成的字体缩放
	protected Text mText;										// UGUI的Text组件
	protected TextMeshProUGUI mTextTMP;							// TextMeshPro的Text组件
	public int mChineseOriginFontSize;							// 非运行时的中文字体大小
	public List<FontSizeInfo> mLanguageOriginFontSize = new();	// 非运行时的多语言字体大小
	public string mLocalzation;									// 中文简体的文本
	public string mEnglish;                                     // 英文的文本
	public string mChineseTraditional;                          // 中文繁体的文本
	public static string mCurLanguage;							// 当前的语言类型
	public void Start()
    {
		initTextComponent();
		if (!isTextComponentValid())
		{
			return;
		}
		if (!Application.isPlaying)
		{
			mChineseOriginFontSize = getFontSize();
		}
		else
		{
			if (mChineseOriginFontSize > 0)
			{
				mFontSizeScale = getFontSize() / mChineseOriginFontSize;
			}
			else
			{
				mFontSizeScale = 1.0f;
			}
		}
		mLocalzation = getText();

		if (mCurLanguage == LANGUAGE_CHINESE)
		{
			setText(mLocalzation);
		}
		else if (mCurLanguage == LANGUAGE_ENGLISH)
		{
			setText(mEnglish);
		}
		else if (mCurLanguage == LANGUAGE_CHINESE_TRADITIONAL)
		{
			setText(mChineseTraditional);
		}
		foreach (FontSizeInfo item in mLanguageOriginFontSize)
		{
			if (item.mLanguage == mCurLanguage)
			{
				setFontSize(Mathf.RoundToInt(item.mFontSize * mFontSizeScale));
			}
		}
	}
	public void OnValidate()
	{
		if (Application.isPlaying)
        {
			return;
		}
		initTextComponent();
		if (!isTextComponentValid())
        {
            return;
        }
		mLocalzation = getText();
		mChineseOriginFontSize = getFontSize();
		// 加上默认字体
		// 中文简体,同时也要保证中文的字体大小是实时更新的
		int chineseIndex = mLanguageOriginFontSize.FindIndex((item) => { return item.mLanguage == LANGUAGE_CHINESE; });
		if (chineseIndex < 0)
		{
			mLanguageOriginFontSize.Add(new(LANGUAGE_CHINESE, mChineseOriginFontSize));
		}
		else
		{
			FontSizeInfo info = mLanguageOriginFontSize[chineseIndex];
			info.mFontSize = mChineseOriginFontSize;
			mLanguageOriginFontSize[chineseIndex] = info;
		}
		// 中文繁体
		int chineseTradiIndex = mLanguageOriginFontSize.FindIndex((item) => { return item.mLanguage == LANGUAGE_CHINESE_TRADITIONAL; });
		if (chineseTradiIndex < 0)
		{
			mLanguageOriginFontSize.Add(new(LANGUAGE_CHINESE_TRADITIONAL, mChineseOriginFontSize));
		}
		else
		{
			FontSizeInfo info = mLanguageOriginFontSize[chineseTradiIndex];
			info.mFontSize = mChineseOriginFontSize;
			mLanguageOriginFontSize[chineseTradiIndex] = info;
		}
		// 英文
		if (mLanguageOriginFontSize.FindIndex((item) => { return item.mLanguage == LANGUAGE_ENGLISH; }) < 0)
		{
			mLanguageOriginFontSize.Add(new(LANGUAGE_ENGLISH, mChineseOriginFontSize));
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private bool isTextComponentValid() { return mText != null || mTextTMP != null; }
	private void initTextComponent()
	{
		if (mText == null && TryGetComponent(out mText))
		{
			return;
		}
		if (mTextTMP == null && TryGetComponent(out mTextTMP))
		{
			return;
		}
	}
	private int getFontSize()
	{
		if (mText != null)
		{
			return mText.fontSize;
		}
		if (mTextTMP != null)
		{
			return (int)mTextTMP.fontSize;
		}
		return 0;
	}
	private void setFontSize(int size)
	{
		if (mText != null)
		{
			mText.fontSize = size;
			return;
		}
		if (mTextTMP != null)
		{
			mTextTMP.fontSize = size;
		}
	}
	private string getText()
	{
		if (mText != null)
		{
			return mText.text;
		}
		if (mTextTMP != null)
		{
			return mTextTMP.text;
		}
		return null;
	}
	private void setText(string text)
	{
		if (mText != null)
		{
			mText.text = text;
		}
		if (mTextTMP != null)
		{
			mTextTMP.text = text;
		}
	}
}