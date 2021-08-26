using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RedPointCount : RedPoint
{
	protected myUGUIText mPointCountText;
	protected int mCount;
	public override void resetProperty()
	{
		base.resetProperty();
		mCount = 0;
		mPointCountText = null;
	}
	public void setPointCountText(myUGUIText text)
	{
		mPointCountText = text;
		mPointCountText?.setText(mCount);
	}
	public void setCount(int count)
	{
		mCount = count;
		mPointCountText?.setText(mCount);
	}
}