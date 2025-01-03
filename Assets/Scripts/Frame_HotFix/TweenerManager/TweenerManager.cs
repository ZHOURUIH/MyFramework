using UnityEngine;
using static FrameUtility;

// Tweener是用于代替Dotween这种的缓动操作
public class TweenerManager : FrameSystem
{
	protected SafeDictionary<long, MyTweener> mTweenerList = new(); // 渐变类的列表
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		using var a = new SafeDictionaryReader<long, MyTweener>(mTweenerList);
		foreach (MyTweener item in a.mReadList.Values)
		{
			item.update(item.isIgnoreTimeScale() ? Time.unscaledDeltaTime : elapsedTime);
		}
	}
	public MyTweenerFloat createTweenerFloat()
	{
		CLASS(out MyTweenerFloat tweener).init();
		mTweenerList.add(tweener.getAssignID(), tweener);
		return tweener;
	}
	public void destroyTweener(MyTweener tweener)
	{
		if (tweener == null)
		{
			return;
		}
		mTweenerList.remove(tweener.getAssignID());
		UN_CLASS(ref tweener);
	}
}