using System;
using System.Collections.Generic;

public class Command : GameBasePooledObject
{
	protected List<CommandCallback> mStartCallback;	// 命令开始执行时的回调函数
	protected List<CommandCallback> mEndCallback;	// 命令执行完毕时的回调函数
	protected CommandReceiver mReceiver;			// 命令接受者
	protected BOOL mResult;							// 命令的执行结果,只用于部分需要知道执行结果的命令使用
	protected float mDelayTime;						// 命令当前延迟时间
	protected uint mCmdID;							// 命令ID,每一个命令对象拥有一个唯一ID
	protected bool mIgnoreTimeScale;				// 命令的延迟时间是否不受时间缩放影响
	protected bool mShowDebugInfo;					// 是否显示调试信息
	protected bool mDelayCommand;					// 是否是延迟执行的命令
	protected EXECUTE_STATE mCmdState;				// 命令执行状态
	public Command()
	{
		mCmdID = makeID();
		mEndCallback = new List<CommandCallback>();
		mStartCallback = new List<CommandCallback>();
		mDestroy = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mReceiver = null;
		mResult = null;
		mShowDebugInfo = true;
		mDelayCommand = false;
		mIgnoreTimeScale = false;
		mCmdState = EXECUTE_STATE.NOT_EXECUTE;
		mEndCallback.Clear();
		mStartCallback.Clear();
		mDelayTime = 0.0f;
		// CmdID不重置
		// mCmdID = 0;
	}
	// 命令执行
	public virtual void execute() { }
	// 调试信息，由CommandSystem调用
	public virtual string showDebugInfo()			{ return Typeof(this).ToString(); }
	public bool isShowDebugInfo()					{ return mShowDebugInfo; }
	public bool isDelayCommand()					{ return mDelayCommand; }
	public CommandReceiver getReceiver()			{ return mReceiver; }
	public EXECUTE_STATE getState()					{ return mCmdState; }
	public float getDelayTime()						{ return mDelayTime; }
	public bool isIgnoreTimeScale()					{ return mIgnoreTimeScale; }
	public uint getID()								{ return mCmdID; }
	public void setShowDebugInfo(bool show)			{ mShowDebugInfo = show; }
	public void setDelayCommand(bool delay)			{ mDelayCommand = delay; }
	public void setReceiver(CommandReceiver Reciver){ mReceiver = Reciver; }
	public void setState(EXECUTE_STATE state)		{ mCmdState = state; }
	public void setDelayTime(float time)			{ mDelayTime = time; }
	public void setIgnoreTimeScale(bool ignore)		{ mIgnoreTimeScale = ignore; }
	public void setResultListen(BOOL result)		{ mResult = result; }
	public void addEndCommandCallback(CommandCallback cmdCallback)
	{
		if (cmdCallback != null)
		{
			mEndCallback.Add(cmdCallback);
		}
	}
	public void addStartCommandCallback(CommandCallback cmdCallback)
	{
		if (cmdCallback != null)
		{
			mStartCallback.Add(cmdCallback);
		}
	}
	public void invokeEndCallBack()
	{
		int count = mEndCallback.Count;
		for(int i = 0; i < count; ++i)
		{
			mEndCallback[i](this);
		}
		mEndCallback.Clear();
	}
	public void invokeStartCallBack()
	{
		int count = mStartCallback.Count;
		for(int i = 0; i < count; ++i)
		{
			mStartCallback[i](this);
		}
		mStartCallback.Clear();
	}
}