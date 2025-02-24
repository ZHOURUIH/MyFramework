using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static MathUtility;
using static FrameBaseHotFix;
using static FrameDefine;

// 热更代码使用
// 如果是会在代码中访问操作的文本对象则需要挂此脚本,目的是为了方便资源检查,避免有太多无效检查或者有遗漏
[RequireComponent(typeof(Text))]
public class LocalizationRuntimeText : MonoBehaviour
{
	protected float mFontSizeScale;								// 由于自适应而造成的字体缩放
	public int mChineseOriginFontSize;                          // 非运行时的中文字体大小
	public List<FontSizeInfo> mLanguageOriginFontSize = new();      // 非运行时的多语言字体大小
	public Text mText;
	private void Awake()
	{
		if (gameObject.TryGetComponent<LocalizationText>(out _))
		{
			Debug.LogError("不允许同时添加LocalizationRuntimeText和LocalizationText");
		}
	}
	private void Start()
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
			mFontSizeScale = divide(mText.fontSize, mChineseOriginFontSize);
		}
		mLocalizationManager?.registeAction(onLanguageChanged);
		onLanguageChanged();
	}
	private void OnValidate()
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
	private void OnDestroy()
	{
		mLocalizationManager?.unregisteAction(onLanguageChanged);
	}
	private void onLanguageChanged()
	{
		if (mText != null && mLocalizationManager != null)
		{
			foreach (FontSizeInfo item in mLanguageOriginFontSize)
			{
				if (item.mLanguage == mLocalizationManager.getCurrentLanguage())
				{
					mText.fontSize = (int)(item.mFontSize * mFontSizeScale);
				}
			}
		}
	}
}