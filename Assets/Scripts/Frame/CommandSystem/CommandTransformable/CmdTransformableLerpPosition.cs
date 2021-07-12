using UnityEngine;

public class CmdTransformableLerpPosition : Command
{
	public LerpCallback mDoingCallBack;
	public LerpCallback mDoneCallBack;
	public Vector3 mTargetPosition;
	public float mLerpSpeed;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallBack = null;
		mDoneCallBack = null;
		mTargetPosition = Vector3.zero;
		mLerpSpeed = 0.0f;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
#if UNITY_EDITOR
		if(obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if (!isFloatZero(mLerpSpeed) && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out COMTransformableLerpPosition com);
		com.setLerpingCallback(mDoingCallBack);
		com.setLerpDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setTargetPosition(mTargetPosition);
		com.setLerpSpeed(mLerpSpeed);
		com.play();
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.Append(": mLerpSpeed:", mLerpSpeed).
				Append(", mTargetPosition:", mTargetPosition);
	}
}