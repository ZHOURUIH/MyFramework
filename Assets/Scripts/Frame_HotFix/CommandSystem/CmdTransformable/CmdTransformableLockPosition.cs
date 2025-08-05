using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;

// 锁定物体的世界坐标不变,可能会有误差
public class CmdTransformableLockPosition : Command
{
	public Vector3 mLockPosition;	// 锁定的目标位置
	public bool mLockX;				// 是否锁定X轴
	public bool mLockY;				// 是否锁定Y轴
	public bool mLockZ;				// 是否锁定Z轴
	public override void resetProperty()
	{
		base.resetProperty();
		mLockPosition = Vector3.zero;
		mLockX = false;
		mLockY = false;
		mLockZ = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			(mLockX || mLockY || mLockZ) && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableLockPosition com);
		com.setActive(true);
		com.setLockPosition(mLockPosition);
		com.setLock(mLockX, mLockY, mLockZ);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mLockPosition:", mLockPosition).
				append(", mLockX:", mLockX).
				append(", mLockY:", mLockY).
				append(", mLockZ:", mLockZ);
	}
}