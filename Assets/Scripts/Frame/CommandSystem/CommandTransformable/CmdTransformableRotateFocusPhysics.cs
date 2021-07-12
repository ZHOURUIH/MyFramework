using System;
using UnityEngine;

public class CmdTransformableRotateFocusPhysics : Command
{
	public Transformable mTarget;
	public Vector3 mOffset;
	public override void resetProperty()
	{
		base.resetProperty();
		mTarget = null;
		mOffset = Vector3.zero;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
#if UNITY_EDITOR
		if (obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if (mTarget != null && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out COMTransformableRotateFocusPhysics com);
		com.setActive(true);
		com.setFocusTarget(mTarget);
		com.setFocusOffset(mOffset);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.Append(": target:", mTarget?.getName());
	}
}