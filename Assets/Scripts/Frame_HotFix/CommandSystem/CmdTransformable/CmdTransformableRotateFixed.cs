using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;

// 锁定物体的旋转
public class CmdTransformableRotateFixed : Command
{
	public Vector3 mFixedEuler;		// 锁定的旋转值
	public bool mActive;			// 是否锁定
	public override void resetProperty()
	{
		base.resetProperty();
		mFixedEuler = Vector3.zero;
		mActive = true;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			mActive && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableRotateFixed com);
		com.setActive(mActive);
		com.setFixedEuler(mFixedEuler);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mActive:", mActive).
				append(", mFixedEuler:", mFixedEuler);
	}
}