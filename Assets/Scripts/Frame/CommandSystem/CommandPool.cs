using System;
using System.Collections.Generic;

public class CommandPool : FrameBase
{
	protected Dictionary<Type, Stack<Command>> mUnusedList;
	protected Dictionary<Type, List<Command>> mInusedList;
	protected ThreadLock mNewCmdLock;	// 只需要添加创建命令的锁就可以,只要不分配出重复的命令,回收命令时就不会发生冲突
	protected ThreadLock mInuseLock;
	protected ThreadLock mUnuseLock;
	protected int mNewCount;
	protected static int mAssignIDSeed;
	protected static int mIDSeed;
	public CommandPool()
	{
		mInusedList = new Dictionary<Type, List<Command>>();
		mUnusedList = new Dictionary<Type, Stack<Command>>();
		mInuseLock = new ThreadLock();
		mUnuseLock = new ThreadLock();
		mNewCmdLock = new ThreadLock();
	}
	public void destroy()
	{
		mInusedList.Clear();
		mUnusedList.Clear();
		mInusedList = null;
		mUnusedList = null;
	}
	public Command newCmd(Type type, bool show = true, bool delay = false)
	{
		mNewCmdLock.waitForUnlock();
		// 首先从未使用的列表中获取,获取不到再重新创建一个
		Command cmd = null;
		if (mUnusedList.TryGetValue(type, out Stack<Command> cmdStack) && cmdStack.Count > 0)
		{
			// 从未使用列表中移除
			mUnuseLock.waitForUnlock();
			cmd = cmdStack.Pop();
			mUnuseLock.unlock();
		}
		// 没有找到可以用的,则创建一个
		if (cmd == null)
		{
			cmd = createInstance<Command>(type);
			cmd.setID(++mIDSeed);
			cmd.init();
			++mNewCount;
		}
		// 设置为可用命令
		cmd.setValid(true);
		if (delay)
		{
			cmd.setAssignID(++mAssignIDSeed);
		}
		else
		{
			cmd.setAssignID(-1);
		}
		cmd.setShowDebugInfo(show);
		cmd.setDelayCommand(delay);
		// 加入已使用列表
		addInuse(cmd);
		mNewCmdLock.unlock();
		return cmd;
	}
	public void destroyCmd(Command cmd) 
	{
		// 销毁命令时,初始化命令数据,并设置为不可用命令
		cmd.init();
		cmd.setValid(false);
		addUnuse(cmd);
		removeInuse(cmd);
	}
	//------------------------------------------------------------------------------------------------------------------
	protected void addInuse(Command cmd)
	{
		mInuseLock.waitForUnlock();
		// 添加到使用列表中
		Type type = cmd.GetType();
		if (!mInusedList.TryGetValue(type, out List<Command> cmdList))
		{
			cmdList = new List<Command>();
			mInusedList.Add(type, cmdList);
		}
		cmdList.Add(cmd);
		mInuseLock.unlock();
	}
	protected void addUnuse(Command cmd)
	{
		mUnuseLock.waitForUnlock();
		// 添加到未使用列表中
		Type type = cmd.GetType();
		if (!mUnusedList.TryGetValue(type, out Stack<Command> cmdList))
		{
			cmdList = new Stack<Command>();
			mUnusedList.Add(type, cmdList);
		}
		cmdList.Push(cmd);
		mUnuseLock.unlock();
	}
	protected void removeInuse(Command cmd)
	{
		mInuseLock.waitForUnlock();
		Type type = cmd.GetType();
		if (mInusedList.TryGetValue(type, out List<Command> cmdList))
		{
			cmdList.Remove(cmd);
		}
		mInuseLock.unlock();
	}
}
