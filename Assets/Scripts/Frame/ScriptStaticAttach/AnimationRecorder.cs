using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRecorder : StateMachineBehaviour
{
	public PathRecorder mPathRecorder;
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