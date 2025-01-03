using System.Collections.Generic;
using static CSharpUtility;

// 命令基类
public class Command : ClassObject
{
	protected List<CommandCallback> mStartCallback = new(); // 命令开始执行时的回调函数
	protected List<CommandCallback> mEndCallback = new();	// 命令执行完毕时的回调函数
	protected CommandReceiver mReceiver;					// 命令接受者
	protected BOOL mResult;									// 命令的执行结果,只用于部分需要知道执行结果的命令使用
	protected float mDelayTime;								// 命令当前延迟时间
	protected int mCmdID;									// 命令ID,每一个命令对象拥有一个唯一ID
	protected bool mIgnoreTimeScale;						// 命令的延迟时间是否不受时间缩放影响
	protected bool mThreadCommand;							// 是否是由多线程对象池创建的命令
	protected bool mDelayCommand;							// 是否是延迟执行的命令
	protected EXECUTE_STATE mCmdState;						// 命令执行状态
	protected LOG_LEVEL mCmdLogLevel = LOG_LEVEL.NORMAL;	// 当前命令的日志等级
	public Command()
	{
		mCmdID = makeID();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mStartCallback.Clear();
		mEndCallback.Clear();
		mReceiver = null;
		mResult = null;
		mDelayTime = 0.0f;
		mIgnoreTimeScale = false;
		mThreadCommand = false;
		mDelayCommand = false;
		mCmdState = EXECUTE_STATE.NOT_EXECUTE;
		mCmdLogLevel = LOG_LEVEL.NORMAL;
		// CmdID不重置
		// mCmdID = 0;
	}
	// 命令执行
	public virtual void execute() { }
	public virtual void onInterrupted() { }
	// 调试信息，由CommandSystem调用
	public virtual void debugInfo(MyStringBuilder builder) { builder.append(GetType().ToString()); }
	public LOG_LEVEL getCmdLogLevel() { return mCmdLogLevel; }
	public bool isDelayCommand() { return mDelayCommand; }
	public CommandReceiver getReceiver() { return mReceiver; }
	public EXECUTE_STATE getState() { return mCmdState; }
	public float getDelayTime() { return mDelayTime; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	public int getID() { return mCmdID; }
	public bool isThreadCommand() { return mThreadCommand; }
	public void setCmdLogLevel(LOG_LEVEL level) { mCmdLogLevel = level; }
	public void setDelayCommand(bool delay) { mDelayCommand = delay; }
	public void setReceiver(CommandReceiver Reciver) { mReceiver = Reciver; }
	public void setState(EXECUTE_STATE state) { mCmdState = state; }
	public void setDelayTime(float time) { mDelayTime = time; }
	public void setIgnoreTimeScale(bool ignore) { mIgnoreTimeScale = ignore; }
	public void setResultListen(BOOL result) { mResult = result; }
	public void setThreadCommand(bool threadCmd) { mThreadCommand = threadCmd; }
	public void addEndCommandCallback(CommandCallback cmdCallback)
	{
		mEndCallback.addNotNull(cmdCallback);
	}
	public void addStartCommandCallback(CommandCallback cmdCallback)
	{
		mStartCallback.addNotNull(cmdCallback);
	}
	public void invokeEndCallBack()
	{
		foreach (CommandCallback callback in mEndCallback)
		{
			callback(this);
		}
		mEndCallback.Clear();
	}
	public void invokeStartCallBack()
	{
		foreach (CommandCallback callback in mStartCallback)
		{
			callback(this);
		}
		mStartCallback.Clear();
	}
}