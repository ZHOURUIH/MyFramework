using System;
using UnityEngine;

public struct ScrollAreaScope : IDisposable
{
	public ScrollArea mScrollArea;
	public ScrollAreaScope(ScrollArea scrollArea)
	{
		mScrollArea = scrollArea;
		mScrollArea.startScroll();
	}
	public void Dispose()
	{
		mScrollArea.endScroll();
	}
}

// 搭配ScrollAreaScope使用,using(var a = new ScrollAreaScope(scrollArea)){}
public class ScrollArea
{
	protected Vector2 mScrollPosition;          // 滚动条的位置
	protected int mWidth;
	protected int mHeight;
	protected bool mStarted;
	public void init(int width, int height)
	{
		mWidth = width;
		mHeight = height;
		mStarted = false;
	}
	public void startScroll()
	{
		if (mWidth == 0 || mHeight == 0)
		{
			Debug.LogError("滚动区域大小错误,请先调用init设置滚动区域大小");
			return;
		}
		if (mStarted)
		{
			Debug.LogError("已经开始滚动区域,不能再开始");
			return;
		}
		mScrollPosition = GUILayout.BeginScrollView(mScrollPosition, GUILayout.Width(mWidth), GUILayout.Height(mHeight));
		mStarted = true;
	}
	public void endScroll()
	{
		if (!mStarted)
		{
			Debug.LogError("已经结束滚动区域,不能再结束");
			return;
		}
		GUILayout.EndScrollView();
		mStarted = false;
	}
}