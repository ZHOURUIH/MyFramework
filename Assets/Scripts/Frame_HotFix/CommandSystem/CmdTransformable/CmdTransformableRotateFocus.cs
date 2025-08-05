using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;

// 使物体始终朝向指定目标
public class CmdTransformableRotateFocus : Command
{
	public Transformable mTarget;	// 朝向的目标
	public Vector3 mOffset;			// 目标位置偏移
	public override void resetProperty()
	{
		base.resetProperty();
		mTarget = null;
		mOffset = Vector3.zero;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			mTarget != null && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableRotateFocus com);
		com.setActive(true);
		com.setFocusTarget(mTarget);
		com.setFocusOffset(mOffset);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
}