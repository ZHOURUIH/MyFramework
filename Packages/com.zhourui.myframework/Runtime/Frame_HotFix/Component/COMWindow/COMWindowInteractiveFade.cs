using UnityEngine;

// 图片被点击时的一些通用变化效果,比如点击时的位置偏移,颜色变暗
public class COMWindowInteractiveFade : GameComponent
{
	protected myUGUIImageSimple mUIImageSimple;
	protected Vector3 mOriginPosition;
	protected Color mOriginColor;
	public override void resetProperty()
	{
		base.resetProperty();
		mUIImageSimple = null;
		mOriginPosition = Vector3.zero;
		mOriginColor = Color.white;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mUIImageSimple = owner as myUGUIImageSimple;
		initUIState();
		mUIImageSimple.setHoverCallback(hover => onButtonHover(hover));
		mUIImageSimple.setPressCallback(press => onButtonPress(press));
		mUIImageSimple.setOnScreenTouchUp((Vector3 pos, int touchID) => { onScreenTouchUp(); });
	}
	public void resetUIState()
	{
		mUIImageSimple.setPosition(mOriginPosition);
		mUIImageSimple.setColor(mOriginColor);
	}
	public void initUIState()
	{
		mOriginPosition = mUIImageSimple.getPosition();
		mOriginColor = mUIImageSimple.getColor();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onButtonPress(bool press)
	{
		// 只执行按下时的缩小,抬起的恢复由全局鼠标检测执行
		if (press)
		{
			mUIImageSimple.setPosition(mOriginPosition + new Vector3(0.0f, -2.0f));
			mUIImageSimple.setColor(mOriginColor);
		}
	}
	protected void onButtonHover(bool hover)
	{
		mUIImageSimple.setColor(hover ? Color.grey : mOriginColor);
	}
	protected void onScreenTouchUp()
	{
		mUIImageSimple.setPosition(mOriginPosition);
		mUIImageSimple.setColor(mOriginColor);
	}
}