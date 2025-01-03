using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static StringUtility;
using static MathUtility;

// 可显示数字的窗口,支持带+-符号,小数点
public class myUGUINumber : myUGUIImage
{
	protected List<myUGUIImageSimple> mNumberList = new();	// 数字窗口列表
	protected DOCKING_POSITION mDockingPosition;		// 数字停靠方式
	protected NUMBER_DIRECTION mDirection;				// 数字显示方向
	protected Sprite[] mSpriteList;						// 该列表只有10个数字的图片
	protected Sprite mAddSprite;						// +号的图片
	protected Sprite mMinusSprite;						// -号的图片
	protected Sprite mDotSprite;						// .号的图片
	protected string mNumberStyle;						// 数字图集名
	protected string mNumber = EMPTY;					// 当前显示的数字
	protected const char ADD_MARK = '+';				// +号
	protected const char MINUS_MARK = '-';				// -号
	protected const char DOT_MARK = '.';				// .号
	protected static string mAllMark = EMPTY + ADD_MARK + MINUS_MARK + DOT_MARK;	// 允许显示的除数字以外的符号
	protected int mInterval = 5;						// 数字显示间隔
	protected int mMaxCount;                            // 数字最大个数
	public myUGUINumber()
	{
		mSpriteList = new Sprite[10];
		mDockingPosition = DOCKING_POSITION.LEFT;
		mDirection = NUMBER_DIRECTION.HORIZONTAL;
	}
	public override void init()
	{
		base.init();
		if (mImage == null)
		{
			return;
		}
		mNumberStyle = mImage.sprite.name.rangeToLast('_');
		for (int i = 0; i < 10; ++i)
		{
			mSpriteList[i] = getSpriteInAtlas(mNumberStyle + "_" + IToS(i));
		}
		mAddSprite = getSpriteInAtlas(mNumberStyle + "_add");
		mMinusSprite = getSpriteInAtlas(mNumberStyle + "_minus");
		mDotSprite = getSpriteInAtlas(mNumberStyle + "_dot");
		setMaxCount(10);
		mImage.enabled = false;
	}
	public override void notifyAnchorApply()
	{
		base.notifyAnchorApply();
		// 此处默认数字窗口都是以ASPECT_BASE.AB_AUTO的方式等比放大
		Vector2 screenScale = getScreenScale(ASPECT_BASE.AUTO);
		mInterval = (int)(mInterval * (mDirection == NUMBER_DIRECTION.HORIZONTAL ? screenScale.x : screenScale.y));
	}
	public override void cloneFrom(myUIObject obj)
	{
		base.cloneFrom(obj);
		var source = obj as myUGUINumber;
		mInterval = source.mInterval;
		mNumberStyle = source.mNumberStyle;
		mNumber = source.mNumber;
		mAddSprite = source.mAddSprite;
		mMinusSprite = source.mMinusSprite;
		mDotSprite = source.mDotSprite;
		int count = mSpriteList.Length;
		for(int i = 0; i < count; ++i)
		{
			mSpriteList[i] = source.mSpriteList[i];
		}
		mDirection = source.mDirection;
		mDockingPosition = source.mDockingPosition;
		setMaxCount(source.mMaxCount);
	}
	// 获得内容横向排列时的实际显示内容总宽度
	public int getContentWidth()
	{
		int width = 0;
		int count = getMin(mNumberList.Count, mNumber.Length);
		for (int i = 0; i < count; ++i)
		{
			width += (int)mNumberList[i].getWindowSize().x;
		}
		return width + mInterval * (mNumber.Length - 1);
	}
	// 获得内容横向排列时的图片总宽度
	public int getAllSpriteWidth()
	{
		int width = 0;
		int count = getMin(mNumberList.Count, mNumber.Length);
		for (int i = 0; i < count; ++i)
		{
			width += (int)mNumberList[i].getSpriteSize().x;
		}
		return width + mInterval * (mNumber.Length - 1);
	}
	// 获得内容纵向排列时的实际显示内容总高度
	public int getContentHeight()
	{
		int height = 0;
		int count = getMin(mNumberList.Count, mNumber.Length);
		for (int i = 0; i < count; ++i)
		{
			height += (int)mNumberList[i].getWindowSize().y;
		}
		return height + mInterval * (mNumber.Length - 1);
	}
	// 获得内容纵向排列时的图片总高度
	public int getAllSpriteHeight()
	{
		int height = 0;
		int count = getMin(mNumberList.Count, mNumber.Length);
		for (int i = 0; i < count; ++i)
		{
			height += (int)mNumberList[i].getSpriteSize().y;
		}
		return height + mInterval * (mNumber.Length - 1);
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
	public void setDirection(NUMBER_DIRECTION direction)
	{
		mDirection = direction;
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
			mNumber = mNumber.startString(mMaxCount);
		}
		mNumberList.Clear();
		for (int i = 0; i < mMaxCount + 1; ++i)
		{
			mNumberList.Add(mLayout.getScript().createUGUIObject<myUGUIImageSimple>(this, mName + "_" + IToS(i), true));
		}
		refreshNumber();
	}
	public void setNumber(int num, int limitLen = 0)
	{
		setNumber(IToS(num, limitLen));
	}
	public void setNumber(string num)
	{
		mNumber = num;
		checkUIntString(mNumber, mAllMark);
		// 设置的数字字符串不能超过最大数量
		if (mNumber.Length > mMaxCount)
		{
			mNumber = mNumber.startString(mMaxCount);
		}
		refreshNumber();
	}
	public int getMaxCount() { return mMaxCount; }
	public string getNumber() { return mNumber; }
	public int getInterval() { return mInterval; }
	public string getNumberStyle() { return mNumberStyle; }
	public DOCKING_POSITION getDockingPosition() { return mDockingPosition; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshNumber()
	{
		if (mNumber.isEmpty())
		{
			foreach (myUGUIImageSimple item in mNumberList)
			{
				item.getImage().enabled = false;
			}
			return;
		}
		int numberStartPos = getFirstNumberPos(mNumber);
		// 数字前最多只允许有一个加号或者减号
		if (numberStartPos == 1)
		{
			if (mNumber[0] == ADD_MARK)
			{
				mNumberList[0].setSpriteOnly(mAddSprite);
			}
			else if (mNumber[0] == MINUS_MARK)
			{
				mNumberList[0].setSpriteOnly(mMinusSprite);
			}
		}
		int dotPos = mNumber.LastIndexOf(DOT_MARK);
		if (dotPos == 0 || dotPos == mNumber.Length - 1)
		{
			logError("number can not start or end with dot!");
			return;
		}
		// 只有整数,且不带符号
		if (dotPos == -1 && numberStartPos == 0)
		{
			for (int i = 0; i < mNumber.Length; ++i)
			{
				mNumberList[i].setSpriteOnly(mSpriteList[mNumber[i] - '0']);
			}
		}
		// 带小数或者带符号
		else
		{
			// 整数部分
			// 带符号
			if (numberStartPos > 0)
			{
				string intPart = mNumber.range(numberStartPos, dotPos);
				for (int i = 0; i < intPart.Length; ++i)
				{
					mNumberList[i + numberStartPos].setSpriteOnly(mSpriteList[intPart[i] - '0']);
				}
			}
			// 不带符号
			else
			{
				string intPart = dotPos != -1 ? mNumber.startString(dotPos) : mNumber;
				for (int i = 0; i < intPart.Length; ++i)
				{
					mNumberList[i].setSpriteOnly(mSpriteList[intPart[i] - '0']);
				}
			}

			// 小数点和小数部分
			if (dotPos != -1)
			{
				mNumberList[dotPos].setSpriteOnly(mDotSprite);
				string floatPart = mNumber.removeStartCount(dotPos + 1);
				for (int i = 0; i < floatPart.Length; ++i)
				{
					mNumberList[i + dotPos + 1].setSpriteOnly(mSpriteList[floatPart[i] - '0']);
				}
			}
		}
		
		// 根据当前窗口的大小调整所有数字的大小
		Vector2 windowSize = getWindowSize();
		int numberLength = mNumber.Length;
		Vector2 numberSize = Vector2.zero;
		float numberScale = 0.0f;
		Sprite sprite = mSpriteList[mNumber[numberStartPos] - '0'];
		if (mDirection == NUMBER_DIRECTION.HORIZONTAL)
		{
			float inverseHeight = divide(1.0f, sprite.rect.height);
			numberSize.x = windowSize.y * sprite.rect.width * inverseHeight;
			numberSize.y = windowSize.y;
			numberScale = windowSize.y * inverseHeight;
		}
		else if (mDirection == NUMBER_DIRECTION.VERTICAL)
		{
			float inverseWidth = divide(1.0f, sprite.rect.width);
			numberSize.x = windowSize.x;
			numberSize.y = windowSize.x * sprite.rect.height * inverseWidth;
			numberScale = windowSize.x * inverseWidth;
		}
		for (int i = 0; i < numberLength; ++i)
		{
			char curChar = mNumber[i];
			if (curChar <= '9' && curChar >= '0')
			{
				mNumberList[i].setWindowSize(numberSize);
			}
			else if (curChar == DOT_MARK)
			{
				mNumberList[i].setWindowSize(mDotSprite.rect.size * numberScale);
			}
			else if (curChar == ADD_MARK)
			{
				mNumberList[i].setWindowSize(mAddSprite.rect.size * numberScale);
			}
			else if (curChar == MINUS_MARK)
			{
				mNumberList[i].setWindowSize(mMinusSprite.rect.size * numberScale);
			}
		}

		int count = mNumberList.Count;
		for (int i = 0; i < count; ++i)
		{
			mNumberList[i].getImage().enabled = i < numberLength;
		}

		// 调整窗口位置,隐藏不需要显示的窗口
		Vector2 leftOrTop = Vector2.zero;
		if (mDirection == NUMBER_DIRECTION.HORIZONTAL)
		{
			int contentWidth = getContentWidth();
			if (mDockingPosition == DOCKING_POSITION.RIGHT)
			{
				leftOrTop = new Vector2(windowSize.x - contentWidth, 0) - windowSize * 0.5f;
			}
			else if (mDockingPosition == DOCKING_POSITION.CENTER)
			{
				leftOrTop = new Vector2((windowSize.x - contentWidth) * 0.5f, 0) - windowSize * 0.5f;
			}
			else if (mDockingPosition == DOCKING_POSITION.LEFT)
			{
				leftOrTop = -windowSize * 0.5f;
			}
			for (int i = 0; i < numberLength; ++i)
			{
				myUGUIImageSimple number = mNumberList[i];
				Vector2 size = number.getWindowSize();
				number.setPosition(leftOrTop + size * 0.5f);
				leftOrTop.x += size.x + mInterval;
			}
		}
		else if (mDirection == NUMBER_DIRECTION.VERTICAL)
		{
			int contentHeight = getContentHeight();
			if (mDockingPosition == DOCKING_POSITION.BOTTOM)
			{
				leftOrTop = new Vector2(0, windowSize.y - contentHeight) - new Vector2(-windowSize.x * 0.5f, windowSize.y * 0.5f);
			}
			else if (mDockingPosition == DOCKING_POSITION.CENTER)
			{
				leftOrTop = new Vector2(0, (windowSize.y - contentHeight) * 0.5f) - new Vector2(-windowSize.x * 0.5f, windowSize.y * 0.5f);
			}
			else if (mDockingPosition == DOCKING_POSITION.TOP)
			{
				leftOrTop = new(-windowSize.x * 0.5f, windowSize.y * 0.5f);
			}
			for (int i = 0; i < numberLength; ++i)
			{
				myUGUIImageSimple number = mNumberList[i];
				Vector2 size = number.getWindowSize();
				number.setPosition(leftOrTop + new Vector2(size.x * 0.5f, -size.y * 0.5f));
				leftOrTop.y -= size.y + mInterval;
			}
		}
	}
}