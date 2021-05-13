using UnityEngine;

public class CmdTransformableRotateFixedPhysics : Command
{
	public Vector3 mFixedEuler;
	public bool mActive;
	public override void resetProperty()
	{
		base.resetProperty();
		mFixedEuler = Vector3.zero;
		mActive = true;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
#if UNITY_EDITOR
		if (obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if (mActive && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out COMTransformableRotateFixedPhysics com);
		com.setActive(mActive);
		com.setFixedEuler(mFixedEuler);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mActive:", mActive).
				Append(", mFixedEuler:", mFixedEuler);
	}
}