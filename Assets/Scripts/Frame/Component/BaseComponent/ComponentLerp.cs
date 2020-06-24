using System;
using System.Collections.Generic;
using UnityEngine;

public class ComponentLerp : GameComponent, IComponentBreakable
{
	protected LerpCallback mLerpingCallBack;
	protected LerpCallback mLerpDoneCallBack;
	protected PLAY_STATE mPlayState;
	protected float mLerpSpeed;
	public override void setActive(bool active)
	{
		base.setActive(active);
		if (!active)
		{
			stop();
		}
	}
	public virtual void play()
	{
		if(isFloatZero(mLerpSpeed))
		{
			stop();
			return;
		}
		mPlayState = PLAY_STATE.PS_PLAY;
		update(0.0f);
	}
	public virtual void stop(bool force = false)
	{
		// 如果已经是停止的状态,并且不是要强制停止,则不再执行
		if (mPlayState == PLAY_STATE.PS_STOP && !force)
		{
			return;
		}
		mPlayState = PLAY_STATE.PS_STOP;
	}
	public virtual void pause() { mPlayState = PLAY_STATE.PS_PAUSE; }
	public void setState(PLAY_STATE state)
	{
		if (mPlayState == state)
		{
			return;
		}
		if (state == PLAY_STATE.PS_PLAY)
		{
			play();
		}
		else if (state == PLAY_STATE.PS_STOP)
		{
			stop();
		}
		else if (state == PLAY_STATE.PS_PAUSE)
		{
			pause();
		}
	}
	public void setLerpingCallback(LerpCallback callback){setCallback(callback, ref mLerpingCallBack, this);}
	public void setLerpDoneCallback(LerpCallback callback){setCallback(callback, ref mLerpDoneCallBack, this);}
	public void setLerpSpeed(float speed) { mLerpSpeed = speed; }
	public float getLerpSpeed() { return mLerpSpeed; }
	public void notifyBreak()
	{
		setLerpingCallback(null);
		setLerpDoneCallback(null);
	}
	//----------------------------------------------------------------------------------------------------------------------------
	protected static void setCallback(LerpCallback callback, ref LerpCallback curCallback, ComponentLerp component)
	{
		LerpCallback tempCallback = curCallback;
		curCallback = null;
		// 如果回调函数当前不为空,则是中断了正在进行的变化
		tempCallback?.Invoke(component, true);
		curCallback = callback;
	}
	protected static void doneCallback(ref LerpCallback curDoneCallback, ComponentLerp component)
	{
		// 先保存回调,然后再调用回调之前就清空回调,确保在回调函数执行时已经完全完成
		LerpCallback tempCallback = curDoneCallback;
		component.clearCallback();
		tempCallback?.Invoke(component, false);
	}
	protected void clearCallback()
    {
		mLerpingCallBack = null;
		mLerpDoneCallBack = null;
    }
    protected void afterApplyLerp(bool done)
    {
		mLerpingCallBack?.Invoke(this, false);
        if (done)
        {
            setActive(false);
            // 强制停止组件
            stop(true);
            doneCallback(ref mLerpDoneCallBack, this);
        }
    }
}