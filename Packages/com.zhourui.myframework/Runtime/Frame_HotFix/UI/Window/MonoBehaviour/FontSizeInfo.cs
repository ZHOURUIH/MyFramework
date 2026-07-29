using System;

[Serializable]
// 结构体,存储不同语言的字号信息
public struct FontSizeInfo
{
	public string mLanguage;
	public int mFontSize;
	public FontSizeInfo(string language, int fontSize)
	{
		mLanguage = language;
		mFontSize = fontSize;
	}
}