using System;
using System.Collections.Generic;

// Tweener是用于代替Dotween这种的缓动操作
public class TweenerManager : FrameSystem
{
	protected SafeDictionary<long, MyTweener> mTweenerList;
	public TweenerManager()
	{
		mTweenerList = new SafeDictionary<long, MyTweener>();
	}
	public override void init()
	{
		base.init();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach(var item in mTweenerList.startForeach())
		{
			item.Value.update(elapsedTime);
		}
	}
	public MyTweenerFloat createTweenerFloat()
	{
		CLASS_MAIN(out MyTweenerFloat tweener);
		tweener.init();
		mTweenerList.add(tweener.getAssignID(), tweener);
		return tweener;
	}
	public void destroyTweener(MyTweener tweener)
	{
		if(tweener == null)
		{
			return;
		}
		mTweenerList.remove(tweener.getAssignID());
		UN_CLASS(tweener);
	}
}