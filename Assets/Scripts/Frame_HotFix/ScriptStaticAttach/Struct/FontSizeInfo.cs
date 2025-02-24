using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static MathUtility;
using static FrameBaseHotFix;
using static FrameDefine;

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