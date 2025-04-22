
// 可显示数字的红点
public class RedPointCount : RedPoint
{
	protected myUGUIText mPointCountText;	// 数字UI
	protected int mCount;					// 显示的数字
	public override void resetProperty()
	{
		base.resetProperty();
		mPointCountText = null;
		mCount = 0;
	}
	public void setPointUI(myUGUIObject point, myUGUIText text)
	{
		addPointUI(point);
		mPointCountText = text;
		mPointCountText?.setTextInt(mCount);
	}
	public void removePointUI(myUGUIObject point, myUGUIText text)
	{
		removePointUI(point);
		if (mPointCountText == text)
		{
			mPointCountText = null;
		}
	}
	public void setCount(int count)
	{
		mCount = count;
		mPointCountText?.setTextInt(mCount);
		// 数量大于0时肯定会显示红点,为0则不显示
		setEnable(mCount > 0);
	}
}