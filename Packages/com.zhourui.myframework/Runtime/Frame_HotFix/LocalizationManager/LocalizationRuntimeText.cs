using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static FrameBaseDefine;
using static FrameBaseHotFix;
using static MathUtility;

// 热更代码使用
// 如果是会在代码中访问操作的文本对象则需要挂此脚本,目的是为了方便资源检查,避免有太多无效检查或者有遗漏
public class LocalizationRuntimeText : MonoBehaviour
{
	public int mChineseOriginFontSize;                          // 非运行时的中文字体大小
	public List<FontSizeInfo> mLanguageOriginFontSize = new();  // 非运行时的多语言字体大小
	protected float mFontSizeScale;								// 由于自适应而造成的字体缩放
	protected Text mText;										// UGUI的Text组件
	protected TextMeshProUGUI mTextTMP;							// TextMeshPro的组件
	private void Awake()
	{
		if (gameObject.TryGetComponent<LocalizationText>(out _))
		{
			Debug.LogError("不允许同时添加LocalizationRuntimeText和LocalizationText");
		}
	}
	private void Start()
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
			mFontSizeScale = divide(getFontSize(), mChineseOriginFontSize);
		}
		mLocalizationManager?.registeAction(onLanguageChanged);
		onLanguageChanged();
	}
	private void OnValidate()
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
	private void OnDestroy()
	{
		mLocalizationManager?.unregisteAction(onLanguageChanged);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private void onLanguageChanged()
	{
		if (mText == null || mLocalizationManager == null)
		{
			return;
		}
		foreach (FontSizeInfo item in mLanguageOriginFontSize)
		{
			if (item.mLanguage == mLocalizationManager.getCurrentLanguage())
			{
				setFontSize((int)(item.mFontSize * mFontSizeScale));
			}
		}
	}
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
}