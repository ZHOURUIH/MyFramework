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
	public int mChineseOriginFontSize;							// 非运行时的中文字体大小
	public List<FontSizeInfo> mLanguageOriginFontSize = new();	// 非运行时的多语言字体大小
	public string mLocalzation;
	public string mEnglish;
	public string mChineseTraditional;
	public Text mText;
	public static string mCurLanguage;
	public void Start()
    {
		if (mText == null)
		{
			TryGetComponent(out mText);
		}
		if (mText == null)
        {
			Debug.LogError("找不到Text组件:" + gameObject.name);
			return;
        }
		if (!Application.isPlaying)
		{
			mChineseOriginFontSize = mText.fontSize;
		}
		else
		{
			if (mChineseOriginFontSize > 0)
			{
				mFontSizeScale = mText.fontSize / mChineseOriginFontSize;
			}
			else
			{
				mFontSizeScale = 1.0f;
			}
		}
		mLocalzation = mText.text;

		if (mText != null)
		{
			if (mCurLanguage == LANGUAGE_CHINESE)
			{
				mText.text = mLocalzation;
			}
			else if (mCurLanguage == LANGUAGE_ENGLISH)
			{
				mText.text = mEnglish;
			}
			else if (mCurLanguage == LANGUAGE_CHINESE_TRADITIONAL)
			{
				mText.text = mChineseTraditional;
			}
			foreach (FontSizeInfo item in mLanguageOriginFontSize)
			{
				if (item.mLanguage == mCurLanguage)
				{
					mText.fontSize = Mathf.RoundToInt(item.mFontSize * mFontSizeScale);
				}
			}
		}
	}
	public void OnValidate()
	{
		if (!Application.isPlaying)
        {
            if (mText == null)
            {
				TryGetComponent(out mText);
			}
            if (mText == null)
            {
                return;
            }
			mLocalzation = mText.text;
			mChineseOriginFontSize = mText.fontSize;
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
	}
}