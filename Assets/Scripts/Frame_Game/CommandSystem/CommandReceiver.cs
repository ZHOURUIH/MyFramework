using System.Threading;
using static CSharpUtility;
using static FrameBase;

// 命令接收者基类,只有命令接收者的子类可以接收命令
public class CommandReceiver : ClassObject
{
	protected string mName;						// 接收者名字
	protected long mDelayCmdCountSubThread;		// 子线程中此对象剩余未执行的延迟命令数量
	protected long mDelayCmdCountMainThread;    // 主线程中此对象剩余未执行的延迟命令数量
	public override void resetProperty()
	{
		base.resetProperty();
		mName = null;
		mDelayCmdCountSubThread = 0;
		mDelayCmdCountMainThread = 0;
	}
	public string getName() { return mName; }
	public virtual void setName(string name) { mName = name; }
	public void addReceiveDelayCmd()
	{
		if (isMainThread())
		{
			++mDelayCmdCountMainThread;
		}
		else
		{
			Interlocked.Increment(ref mDelayCmdCountSubThread);
		}
	}
	public void removeReceiveDelayCmd() 
	{
		if (isMainThread())
		{
			--mDelayCmdCountMainThread;
		}
		else
		{
			Interlocked.Decrement(ref mDelayCmdCountSubThread);
		}
	}
	public override void destroy()
	{
		base.destroy();
		// 通知命令系统有一个命令接受者已经被销毁了,需要取消命令缓冲区中的即将发给该接受者的命令
		if (Interlocked.Read(ref mDelayCmdCountSubThread) + mDelayCmdCountMainThread > 0)
		{
			mCommandSystem?.notifyReceiverDestroied(this);
		}
	}
}