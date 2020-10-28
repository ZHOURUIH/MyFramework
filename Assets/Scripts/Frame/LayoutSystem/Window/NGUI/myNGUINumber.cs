using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI
public class myNGUINumber : myNGUISprite
{
	// 为了避免GC而创建的变量
	private static Dictionary<string, UISpriteData> mTempSpriteMap;
	protected List<myNGUISprite>	mNumberList;
	protected UISpriteData[]		mSpriteDataList;
	protected string[]				mSpriteNameList;		// 前10个是0~9,第11个是小数点
	protected DOCKING_POSITION		mDockingPosition;
	protected string                mNumberStyle;
	protected string				mNumber;
	protected int                   mInterval;
	protected int					mMaxCount;
	public myNGUINumber()
	{
		mInterval = 5;
		mNumberStyle = null;
		mNumber = EMPTY_STRING;
		mDockingPosition = DOCKING_POSITION.DP_LEFT;
		mSpriteNameList = new string[11];
		mSpriteDataList = new UISpriteData[11];
		mNumberList = new List<myNGUISprite>();
		if(mTempSpriteMap == null)
		{
			mTempSpriteMap = new Dictionary<string, UISpriteData>();
		}
	}
	public override void init()
	{
		base.init();
		if(mSprite == null || mSprite.atlas == null)
		{
			return;
		}
		// 将atlas中的所有图片放到一个map中,方便查找
		mTempSpriteMap.Clear();
		List<UISpriteData> sprites = mSprite.atlas.spriteList;
		int spriteCount = sprites.Count;
		for (int i = 0; i < spriteCount; ++i)
		{
			mTempSpriteMap.Add(sprites[i].name, sprites[i]);
		}
		// 获得所有数字图片的名字和图片
		string spriteName = getSpriteName();
		int lastPos = spriteName.LastIndexOf('_');
		if (lastPos == -1)
		{
			return;
		}
		mNumberStyle = spriteName.Substring(0, lastPos);
		for (int i = 0; i < 10; ++i)
		{
			mSpriteNameList[i] = mNumberStyle + "_" + intToString(i);
			// 在atlas中查找对应名字的图片
			if (mTempSpriteMap.ContainsKey(mSpriteNameList[i]))
			{
				mSpriteDataList[i] = mTempSpriteMap[mSpriteNameList[i]];
			}
		}
		mSpriteNameList[10] = mNumberStyle + "_dot";
		if (mTempSpriteMap.ContainsKey(mSpriteNameList[10]))
		{
			mSpriteDataList[10] = mTempSpriteMap[mSpriteNameList[10]];
		}
		setMaxCount(10);
		mSprite.spriteName = EMPTY_STRING;
	}
	public int getContentWidth()
	{
		int width = 0;
		for (int i = 0; i < mNumberList.Count; ++i)
		{
			if (i >= mNumber.Length)
			{
				break;
			}
			width += (int)mNumberList[i].getWindowSize().x;
		}
		width += mInterval * (mNumber.Length - 1);
		return width;
	}
	protected void refreshNumber()
	{
		if (isEmpty(mNumber))
		{
			int numCount = mNumberList.Count;
			for (int i = 0; i < numCount; ++i)
			{
				mNumberList[i].setActive(false);
			}
			return;
		}
		// 整数部分
		int dotPos = mNumber.LastIndexOf('.');
		if (!isEmpty(mNumber) && (dotPos == 0 || dotPos == mNumber.Length - 1))
		{
			logError("number can not start or end with dot!");
			return;
		}
		string intPart = dotPos != -1 ? mNumber.Substring(0, dotPos) : mNumber;
		for (int i = 0; i < intPart.Length; ++i)
		{
			mNumberList[i].setSpriteName(mSpriteNameList[intPart[i] - '0'], false);
		}
		// 小数点和小数部分
		if (dotPos != -1)
		{
			mNumberList[dotPos].setSpriteName(mSpriteNameList[10], false);
			string floatPart = mNumber.Substring(dotPos + 1, mNumber.Length - dotPos - 1);
			for (int i = 0; i < floatPart.Length; ++i)
			{
				mNumberList[i + dotPos + 1].setSpriteName(mSpriteNameList[floatPart[i] - '0'], false);
			}
		}
		// 调整所有数字的大小,此处的aspectRatio可能没有更新
		Vector2 numberSize = Vector2.zero;
		float numberScale = 0.0f;
		int numberLength = mNumber.Length;
		Vector2 windowSize = getWindowSize();
		if (numberLength > 0)
		{
			numberSize.y = windowSize.y;
			UISpriteData spriteData = mSpriteDataList[mNumber[0] - '0'];
			float inverseHeight = 1.0f / spriteData.height;
			float ratio = spriteData.width * inverseHeight;
			numberSize.x = ratio * numberSize.y;
			numberScale = windowSize.y * inverseHeight;
		}
		for (int i = 0; i < numberLength; ++i)
		{
			if (mNumber[i] != '.')
			{
				mNumberList[i].setWindowSize(numberSize);
			}
			else
			{
				mNumberList[i].setWindowSize(new Vector2(mSpriteDataList[10].width * numberScale, mSpriteDataList[10].height * numberScale));
			}
		}
		// 调整窗口位置,隐藏不需要显示的窗口
		int contentWidth = getContentWidth();
		Vector2 pos = Vector2.zero;
		if (mDockingPosition == DOCKING_POSITION.DP_RIGHT)
		{
			pos = new Vector2(windowSize.x - contentWidth, 0);
		}
		else if (mDockingPosition == DOCKING_POSITION.DP_CENTER)
		{
			pos = new Vector2((windowSize.x - contentWidth) * 0.5f, 0);
		}
		int count = mNumberList.Count;
		for (int i = 0; i < count; ++i)
		{
			mNumberList[i].setActive(i < numberLength);
			if (i < numberLength)
			{
				Vector2 size = mNumberList[i].getWindowSize();
				mNumberList[i].setLocalPosition(pos - windowSize * 0.5f + size * 0.5f);
				pos.x += size.x + mInterval;
			}
		}
	}
	public void setInterval(int interval)
	{
		mInterval = interval;
		refreshNumber();
	}
	public void setDockingPosition(DOCKING_POSITION position)
	{
		mDockingPosition = position;
		refreshNumber();
	}
	public void setMaxCount(int maxCount)
	{
		if (mMaxCount == maxCount)
		{
			return;
		}
		mMaxCount = maxCount;
		// 设置的数字字符串不能超过最大数量
		if (mNumber.Length > mMaxCount)
		{
			mNumber = mNumber.Substring(0, mMaxCount);
		}
		mNumberList.Clear();
		for (int i = 0; i < mMaxCount + 1; ++i)
		{
			string name = mName + "_" + intToString(i);
			// 由于所有数字的大小和位置都是由数字窗口自动计算的,所以不需要为子窗口添加自适应组件
			mNumberList.Add(mLayout.getScript().createObject<myNGUISprite>(this, name, false));
			mNumberList[i].setAtlas(mSprite.atlas);
			mNumberList[i].setDepth(mSprite.depth + 1);
		}
		refreshNumber();
	}
	public void setNumber(int num, int limitLen = 0)
	{
		setNumber(intToString(num, limitLen));
	}
	public void setNumber(string num)
	{
		mNumber = checkString(num, "0123456789.");
		// 设置的数字字符串不能超过最大数量
		if (mNumber.Length > mMaxCount)
		{
			mNumber = mNumber.Substring(0, mMaxCount);
		}
		refreshNumber();
	}
	public override void setDepth(int depth)
	{
		base.setDepth(depth);
		int count = mNumberList.Count;
		for (int i = 0; i < count; ++i)
		{
			mNumberList[i].setDepth(mSprite.depth + 1);
		}
	}
	public int getMaxCount(){return mMaxCount;}
	public string getNumber(){return mNumber;}
	public int getInterval(){return mInterval;}
	public string getNumberStyle(){return mNumberStyle;}
	public DOCKING_POSITION getDockingPosition(){return mDockingPosition;}
}

#endif