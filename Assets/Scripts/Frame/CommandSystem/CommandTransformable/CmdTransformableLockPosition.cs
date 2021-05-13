using UnityEngine;

public class CmdTransformableLockPosition : Command
{
	public Vector3 mLockPosition;
	public bool mLockX;
	public bool mLockY;
	public bool mLockZ;
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
#if UNITY_EDITOR
		if (obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if ((mLockX || mLockY || mLockZ) && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out COMTransformableLockPosition com);
		com.setActive(true);
		com.setLockPosition(mLockPosition);
		com.setLock(mLockX, mLockY, mLockZ);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mLockPosition:", mLockPosition).
				Append(", mLockX:", mLockX).
				Append(", mLockY:", mLockY).
				Append(", mLockZ:", mLockZ);
	}
}