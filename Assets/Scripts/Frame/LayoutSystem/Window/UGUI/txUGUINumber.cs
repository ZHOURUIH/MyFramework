using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class txUGUINumber : txUGUIImage
{
	protected List<txUGUIImage> mNumberList;
	protected DOCKING_POSITION mDockingPosition;
	protected NUMBER_DIRECTION mDirection;
	protected Sprite[] mSpriteList;         // 该列表只有10个数字的图片
	protected Sprite mAddSprite;
	protected Sprite mMinusSprite;
	protected Sprite mDotSprite;
	protected string mNumberStyle;
	protected string mNumber;
	protected const char mAddMark = '+';
	protected const char mMinusMark = '-';
	protected const char mDotMark = '.';
	protected int mInterval;
	protected int mMaxCount;
	public txUGUINumber()
	{
		mSpriteList = new Sprite[10];
		mNumberList = new List<txUGUIImage>();
		mNumberStyle = EMPTY_STRING;
		mNumber = EMPTY_STRING;
		mInterval = 5;
		mDockingPosition = DOCKING_POSITION.DP_LEFT;
		mDirection = NUMBER_DIRECTION.ND_HORIZONTAL;
	}
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		if (mImage == null)
		{
			return;
		}
		string imageName = mImage.sprite.name;
		int lastPos = imageName.LastIndexOf('_');
		if (lastPos == -1)
		{
			return;
		}
		mNumberStyle = imageName.Substring(0, lastPos);
		// 在GameAtlas/NumberStyle中查找同名的图集,如果找不到同名的图集,则使用公共的数字图集
		string atlasName = CommonDefine.R_ATLAS_NUMBER_STYLE_PATH + mNumberStyle;
		if (mTPSpriteManager.getSprites(atlasName, false) == null)
		{
			atlasName = CommonDefine.R_ATLAS_NUMBER_STYLE_PATH + CommonDefine.COMMON_NUMBER_STYLE;
		}
		for (int i = 0; i < 10; ++i)
		{
			mSpriteList[i] = mTPSpriteManager.getSprite(atlasName, mNumberStyle + "_" + i);
		}
		mAddSprite = mTPSpriteManager.getSprite(atlasName, mNumberStyle + "_add");
		mMinusSprite = mTPSpriteManager.getSprite(atlasName, mNumberStyle + "_minus");
		mDotSprite = mTPSpriteManager.getSprite(atlasName, mNumberStyle + "_dot");
		setMaxCount(10);
		mImage.enabled = false;
	}
	public override void notifyAnchorApply()
	{
		base.notifyAnchorApply();
		// 此处默认数字窗口都是以ASPECT_BASE.AB_AUTO的方式等比放大
		if (mDirection == NUMBER_DIRECTION.ND_HORIZONTAL)
		{
			mInterval = (int)(mInterval * adjustScreenScale(false).x);
		}
		else
		{
			mInterval = (int)(mInterval * adjustScreenScale(false).y);
		}
	}
	public override void cloneFrom(txUIObject obj)
	{
		base.cloneFrom(obj);
		txUGUINumber source = obj as txUGUINumber;
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
	// 获得内容横向排列时的总宽度
	public int getContentWidth()
	{
		int width = 0;
		for (int i = 0; i < mNumberList.Count; ++i)
		{
			if (i >= mNumber.Length)
			{
				break;
			}
			width += (int)mNumberList[i].getSpriteSize().x;
		}
		width += mInterval * (mNumber.Length - 1);
		return width;
	}
	// 获得内容纵向排列时的总高度
	public int getContentHeight()
	{
		int height = 0;
		for (int i = 0; i < mNumberList.Count; ++i)
		{
			if (i >= mNumber.Length)
			{
				break;
			}
			height += (int)mNumberList[i].getSpriteSize().y;
		}
		height += mInterval * (mNumber.Length - 1);
		return height;
	}
	protected void refreshNumber()
	{
		if (mNumber.Length == 0)
		{
			int numCount = mNumberList.Count;
			for (int i = 0; i < numCount; ++i)
			{
				mNumberList[i].setActive(false);
			}
			return;
		}
		int numberStartPos = getFirstNumberPos(mNumber);
		// 数字前最多只允许有一个加号或者减号
		if (numberStartPos == 1)
		{
			if (mNumber[0] == mAddMark)
			{
				mNumberList[0].setSpriteOnly(mAddSprite);
			}
			else if (mNumber[0] == mMinusMark)
			{
				mNumberList[0].setSpriteOnly(mMinusSprite);
			}
		}
		// 整数部分
		int dotPos = mNumber.LastIndexOf(mDotMark);
		if (mNumber.Length > 0 && (dotPos == 0 || dotPos == mNumber.Length - 1))
		{
			logError("number can not start or end with dot!");
			return;
		}
		string intPart = dotPos != -1 ? mNumber.Substring(numberStartPos, dotPos - numberStartPos) : mNumber.Substring(numberStartPos);
		for (int i = 0; i < intPart.Length; ++i)
		{
			mNumberList[i + numberStartPos].setSpriteOnly(mSpriteList[intPart[i] - '0']);
		}
		// 小数点和小数部分
		if (dotPos != -1)
		{
			mNumberList[dotPos].setSpriteOnly(mDotSprite);
			string floatPart = mNumber.Substring(dotPos + 1, mNumber.Length - dotPos - 1);
			for (int i = 0; i < floatPart.Length; ++i)
			{
				mNumberList[i + dotPos + 1].setSpriteOnly(mSpriteList[floatPart[i] - '0']);
			}
		}
		// 根据当前窗口的大小调整所有数字的大小
		Vector2 numberSize = Vector2.zero;
		float numberScale = 0.0f;
		Vector2 windowSize = getWindowSize();
		int numberLength = mNumber.Length;
		if (numberLength > 0)
		{
			if (mDirection == NUMBER_DIRECTION.ND_HORIZONTAL)
			{
				Sprite sprite = mSpriteList[mNumber[numberStartPos] - '0'];
				float ratio = sprite.rect.width / sprite.rect.height;
				numberSize.y = windowSize.y;
				numberSize.x = ratio * numberSize.y;
				numberScale = windowSize.y / sprite.rect.height;
			}
			else if (mDirection == NUMBER_DIRECTION.ND_VERTICAL)
			{
				Sprite sprite = mSpriteList[mNumber[numberStartPos] - '0'];
				float ratio = sprite.rect.width / sprite.rect.height;
				numberSize.x = windowSize.x;
				numberSize.y = numberSize.x / ratio;
				numberScale = windowSize.x / sprite.rect.width;
			}
		}
		for (int i = 0; i < numberLength; ++i)
		{
			if (mNumber[i] <= '9' && mNumber[i] >= '0')
			{
				mNumberList[i].setWindowSize(numberSize);
			}
			else if (mNumber[i] == mDotMark)
			{
				mNumberList[i].setWindowSize(mDotSprite.rect.size * numberScale);
			}
			else if (mNumber[i] == mAddMark)
			{
				mNumberList[i].setWindowSize(mAddSprite.rect.size * numberScale);
			}
			else if (mNumber[i] == mMinusMark)
			{
				mNumberList[i].setWindowSize(mMinusSprite.rect.size * numberScale);
			}
		}
		// 调整窗口位置,隐藏不需要显示的窗口
		Vector2 leftOrTop = Vector2.zero;
		if (mDirection == NUMBER_DIRECTION.ND_HORIZONTAL)
		{
			int contentWidth = getContentWidth();
			if (mDockingPosition == DOCKING_POSITION.DP_RIGHT)
			{
				leftOrTop = new Vector2(windowSize.x - contentWidth, 0) - windowSize * 0.5f;
			}
			else if (mDockingPosition == DOCKING_POSITION.DP_CENTER)
			{
				leftOrTop = new Vector2((windowSize.x - contentWidth) * 0.5f, 0) - windowSize * 0.5f;
			}
			else if (mDockingPosition == DOCKING_POSITION.DP_LEFT)
			{
				leftOrTop = -windowSize * 0.5f;
			}
		}
		else if (mDirection == NUMBER_DIRECTION.ND_VERTICAL)
		{
			int contentHeight = getContentHeight();
			if (mDockingPosition == DOCKING_POSITION.DP_BOTTOM)
			{
				leftOrTop = new Vector2(0, windowSize.y - contentHeight) - new Vector2(-windowSize.x * 0.5f, windowSize.y * 0.5f);
			}
			else if (mDockingPosition == DOCKING_POSITION.DP_CENTER)
			{
				leftOrTop = new Vector2(0, (windowSize.y - contentHeight) * 0.5f) - new Vector2(-windowSize.x * 0.5f, windowSize.y * 0.5f);
			}
			else if (mDockingPosition == DOCKING_POSITION.DP_TOP)
			{
				leftOrTop = new Vector2(-windowSize.x * 0.5f, windowSize.y * 0.5f);
			}
		}
		int count = mNumberList.Count;
		for (int i = 0; i < count; ++i)
		{
			mNumberList[i].setActive(i < numberLength);
			if (i >= numberLength)
			{
				continue;
			}
			if (mDirection == NUMBER_DIRECTION.ND_HORIZONTAL)
			{
				Vector2 size = mNumberList[i].getWindowSize();
				Vector3 newPos = leftOrTop + size * 0.5f;
				mNumberList[i].setPosition(newPos);
				leftOrTop.x += size.x + mInterval;
			}
			else if (mDirection == NUMBER_DIRECTION.ND_VERTICAL)
			{
				Vector2 size = mNumberList[i].getWindowSize();
				Vector3 newPos = leftOrTop + new Vector2(size.x * 0.5f, -size.y * 0.5f);
				mNumberList[i].setPosition(newPos);
				leftOrTop.y -= size.y + mInterval;
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
			mNumber = mNumber.Substring(0, mMaxCount);
		}
		mNumberList.Clear();
		for (int i = 0; i < mMaxCount + 1; ++i)
		{
			string name = mName + "_" + i;
			mNumberList.Add(mLayout.getScript().createObject<txUGUIImage>(this, name, false));
		}
		refreshNumber();
	}
	public void setNumber(int num, int limitLen = 0)
	{
		setNumber(intToString(num, limitLen));
	}
	public void setNumber(string num)
	{
		mNumber = checkUIntString(num, "" + mAddMark + mMinusMark + mDotMark);
		// 设置的数字字符串不能超过最大数量
		if (mNumber.Length > mMaxCount)
		{
			mNumber = mNumber.Substring(0, mMaxCount);
		}
		refreshNumber();
	}
	public int getMaxCount() { return mMaxCount; }
	public string getNumber() { return mNumber; }
	public int getInterval() { return mInterval; }
	public string getNumberStyle() { return mNumberStyle; }
	public DOCKING_POSITION getDockingPosition() { return mDockingPosition; }
	//----------------------------------------------------------------------------------------------------------------
}