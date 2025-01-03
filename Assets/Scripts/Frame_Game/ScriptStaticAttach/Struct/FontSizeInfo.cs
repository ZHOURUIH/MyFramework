using System;

[Serializable]
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