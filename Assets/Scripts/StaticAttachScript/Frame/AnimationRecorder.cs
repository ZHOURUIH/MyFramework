using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRecorder : StateMachineBehaviour
{
    public PathRecorder mPathRecorder;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mPathRecorder?.notifyStartRecord();
        Debug.Log("OnStateEnter");
    }
    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mPathRecorder?.notifyRecording();
        Debug.Log("OnStateUpdate");
    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mPathRecorder?.notifyEndRecord();
        Debug.Log("OnStateExit");
    }
}
