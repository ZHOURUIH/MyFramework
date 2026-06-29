using UnityEngine;

// 动画轨迹记录组件,从Animator中获得动画的播放事件
public class AnimationRecorder : StateMachineBehaviour
{
	public PathRecorder mPathRecorder;		// 轨迹记录工具
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		mPathRecorder?.notifyStartRecord();
	}
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		mPathRecorder?.notifyRecording();
	}
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		mPathRecorder?.notifyEndRecord();
	}
}