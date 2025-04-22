using System;
using System.Collections.Generic;
using static FrameUtility;
using static StringUtility;
using static UnityUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 本地化系统,用于翻译文本,需要外部设置多语言文本内容
public class LocalizationManager : FrameSystem
{
	protected Dictionary<IUGUIText, TextObjectLocalization> mTextList = new();		// 注册的多语言显示物体
	protected Dictionary<IUGUIImage, ImageObjectLocalization> mImageList = new();	// 注册的多语言显示物体
	protected Dictionary<string, string> mLocalizationLanguage = new();				// 当前本地语言的列表,以中文为key,value是当前切换的语言
	protected Dictionary<int, string> mLocalizationLanguageID = new();				// 当前本地语言的列表,以ID为key,value是当前切换的语言
	protected string mCurrentLanguage;												// 当前选择的语言
	protected Dictionary<string, string> mLocaleMap = new();						// 语言的locale,key是Chinese,English等,Value是zh_CN,en-US等
	protected Action mLanguageCallback;												// 语言发生改变时的回调
	protected OnReloadLanguage mReloadLanguageCallback;                             // 需要外部设置的刷新语言列表的函数
	protected StringIntCallback mCheckLanguageCallback;								// 需要外部设置的检测语言是否配置正确的函数
	public LocalizationManager()
	{
		mLocaleMap.add(LANGUAGE_CHINESE, "zh_CN");
		mLocaleMap.add(LANGUAGE_CHINESE_TRADITIONAL, "zh_CN");
		mLocaleMap.add(LANGUAGE_ENGLISH, "en-US");
	}
	public void setReloadLanguageCallback(OnReloadLanguage callback) { mReloadLanguageCallback = callback; }
	public void setCheckLanguageCallback(StringIntCallback callback) { mCheckLanguageCallback = callback; }
	public void setCurrentLanguage(string language)
	{
		mLocalizationLanguage.Clear();
		mLocalizationLanguageID.Clear();
		mReloadLanguageCallback?.Invoke(language, mLocalizationLanguage, mLocalizationLanguageID);
		mCurrentLanguage = language;
		mLanguageCallback?.Invoke();
		foreach (TextObjectLocalization item in mTextList.Values)
		{
			// 有回调就只是调用回调
			if (item.mCallback != null)
			{
				invokeLocalizationCallback(item);
			}
			// 没有就按当前语言直接设置文本,将文本和参数都翻译后再传参
			else
			{
				if (item.mID != 0)
				{
					item.mObject.setText(getLocalize(item.mID, item.mParam));
				}
				else
				{
					item.mObject.setText(getLocalize(item.mText, item.mParam));
				}
			}
		}
		foreach (ImageObjectLocalization item in mImageList.Values)
		{
			item.mObject.setSpriteName(item.mImageNameWithoutSuffix + mCurrentLanguage);
		}
	}
	public string getLocalize(string text) 
	{
		mCheckLanguageCallback?.Invoke(text, 0);
		return mLocalizationLanguage.get(text, text);
	}
	public string getLocalize(int id) 
	{
		mCheckLanguageCallback?.Invoke(null, 0);
		return mLocalizationLanguageID.get(id);
	}
	public string getLocalize(string str, params string[] param)
	{
		string tip = getLocalize(str);
		if (!param.isEmpty())
		{
			using var a = new ListScope<string>(out var tempParam);
			foreach (string item in param)
			{
				tempParam.Add(getLocalize(item));
			}
			tip = format(tip, tempParam);
		}
		return tip;
	}
	public string getLocalize(int id, params string[] param)
	{
		string tip = getLocalize(id);
		if (!param.isEmpty())
		{
			using var a = new ListScope<string>(out var tempParam);
			foreach (string item in param)
			{
				tempParam.Add(getLocalize(item));
			}
			tip = format(tip, tempParam);
		}
		return tip;
	}
	public string getLocalize(string str, IList<string> param)
	{
		string tip = getLocalize(str);
		if (!param.isEmpty())
		{
			using var a = new ListScope<string>(out var tempParam);
			foreach (string item in param)
			{
				tempParam.Add(getLocalize(item));
			}
			tip = format(tip, tempParam);
		}
		return tip;
	}
	public string getLocalize(int id, IList<string> param)
	{
		string tip = getLocalize(id);
		if (!param.isEmpty())
		{
			using var a = new ListScope<string>(out var tempParam);
			foreach (string item in param)
			{
				tempParam.Add(getLocalize(item));
			}
			tip = format(tip, tempParam);
		}
		return tip;
	}
	// 注册语言切换时的回调
	public void registeAction(Action langchange) { mLanguageCallback += langchange; }
	// 注销语言切换时的回调
	public void unregisteAction(Action langchange) { mLanguageCallback -= langchange; }
	public void registeLocalization(IUGUIImage obj, string chineseSpriteName)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationImage>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationImage脚本:" + obj.getName());
		}
		if (mImageList.getOrAddClass(obj, out ImageObjectLocalization localization))
		{
			localization.mImageNameWithoutSuffix = null;
		}
		if (chineseSpriteName.endWith("_" + LANGUAGE_CHINESE))
		{
			logError("多语言图片名需要以_" + LANGUAGE_CHINESE + "结尾");
		}
		chineseSpriteName = chineseSpriteName.removeEndString(LANGUAGE_CHINESE);
		localization.mImageNameWithoutSuffix = chineseSpriteName;
		localization.mObject.setSpriteName(localization.mImageNameWithoutSuffix + mCurrentLanguage);
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string text)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本:" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mObject = obj;
		localization.mText = text;
		localization.mID = 0;
		localization.mParam.Clear();
		localization.mCallback = null;
		localization.mObject.setText(getLocalize(localization.mText, localization.mParam));
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string text, string param)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mObject = obj;
		localization.mText = text;
		localization.mID = 0;
		localization.mParam.Clear();
		localization.mParam.Add(param);
		localization.mCallback = null;
		localization.mObject.setText(getLocalize(localization.mText, localization.mParam));
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string text, string param0, string param1)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mCallback = null;
		localization.mObject = obj;
		localization.mText = text;
		localization.mParam.Clear();
		localization.mParam.Add(param0);
		localization.mParam.Add(param1);
		localization.mObject.setText(getLocalize(localization.mText, localization.mParam));
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string text, string param0, string param1, string param2)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mCallback = null;
		localization.mObject = obj;
		localization.mText = text;
		localization.mParam.Clear();
		localization.mParam.Add(param0);
		localization.mParam.Add(param1);
		localization.mParam.Add(param2);
		localization.mObject.setText(getLocalize(localization.mText, localization.mParam));
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string text, string param0, string param1, string param2, string param3)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mCallback = null;
		localization.mObject = obj;
		localization.mText = text;
		localization.mParam.Clear();
		localization.mParam.Add(param0);
		localization.mParam.Add(param1);
		localization.mParam.Add(param2);
		localization.mParam.Add(param3);
		localization.mObject.setText(getLocalize(localization.mText, localization.mParam));
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string text, Span<string> param)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mCallback = null;
		localization.mObject = obj;
		localization.mText = text;
		localization.mParam.setRangeSpan(param);
		localization.mObject.setText(getLocalize(localization.mText, localization.mParam));
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string text, IList<string> param)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mCallback = null;
		localization.mObject = obj;
		localization.mText = text;
		localization.mParam.setRange(param);
		localization.mObject.setText(getLocalize(localization.mText, localization.mParam));
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, int id)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mText = null;
		localization.mParam.Clear();
		localization.mCallback = null;
		localization.mObject = obj;
		localization.mID = id;
		localization.mObject.setText(getLocalize(localization.mID, localization.mParam));
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string mainText, OnLocalization callback)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mParam.Clear();
		localization.mObject = obj;
		localization.mText = mainText;
		localization.mCallback = callback;
		invokeLocalizationCallback(localization);
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string mainText, string param, OnLocalization callback)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mObject = obj;
		localization.mText = mainText;
		localization.mParam.Clear();
		localization.mParam.Add(param);
		localization.mCallback = callback;
		invokeLocalizationCallback(localization);
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string mainText, string param0, string param1, OnLocalization callback)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mObject = obj;
		localization.mText = mainText;
		localization.mParam.Clear();
		localization.mParam.Add(param0);
		localization.mParam.Add(param1);
		localization.mCallback = callback;
		invokeLocalizationCallback(localization);
	}
	// 注册需要切换多语言的文本对象,可重复注册
	public void registeLocalization(IUGUIText obj, string mainText, IList<string> paramList, OnLocalization callback)
	{
		if (isEditor() && obj.tryGetUnityComponent<LocalizationText>() != null)
		{
			logError("动态访问的文本对象不需要挂接LocalizationText脚本" + obj.getName());
		}
		TextObjectLocalization localization = mTextList.getOrAddClass(obj);
		localization.mID = 0;
		localization.mObject = obj;
		localization.mText = mainText;
		localization.mParam.setRange(paramList);
		localization.mCallback = callback;
		invokeLocalizationCallback(localization);
	}
	public void unregisteLocalization(ICollection<IUGUIObject> objList)
	{
		foreach (IUGUIObject obj in objList.safe())
		{
			if (obj is IUGUIText text && mTextList.Remove(text, out TextObjectLocalization textLocalization))
			{
				UN_CLASS(ref textLocalization);
			}
			else if (obj is myUGUIImage image && mImageList.Remove(image, out ImageObjectLocalization imageLocalization))
			{
				UN_CLASS(ref imageLocalization);
			}
		}
	}
	// 注销需要切换多语言的文本对象
	public void unregisteLocalization(myUIObject obj)
	{
		if (obj is IUGUIText text && mTextList.Remove(text, out TextObjectLocalization textLocalization))
		{
			UN_CLASS(ref textLocalization);
		}
		else if (obj is myUGUIImage image && mImageList.Remove(image, out ImageObjectLocalization imageLocalization))
		{
			UN_CLASS(ref imageLocalization);
		}
	}
	// 获取当前设置的语言类型,比如Chinese,English
	public string getCurrentLanguage() { return mCurrentLanguage; }
	// 获取当前设置的语言类型,比如zh_CN,en-US,与Chinese,English对应
	public string getCurrentLocale() { return mLocaleMap.get(mCurrentLanguage); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void invokeLocalizationCallback(TextObjectLocalization item)
	{
		// 需要将主文本和参数文本全部翻译后再传参
		string str = item.mID != 0 ? getLocalize(item.mID) : getLocalize(item.mText);
		if (item.mParam.Count > 0)
		{
			using var a = new ListScope<string>(out var tempList);
			foreach (string param in item.mParam)
			{
				tempList.Add(getLocalize(param));
			}
			item.mCallback(item.mObject, str, tempList);
		}
		else
		{
			item.mCallback(item.mObject, str, null);
		}
	}
}