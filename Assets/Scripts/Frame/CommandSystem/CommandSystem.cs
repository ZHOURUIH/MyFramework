using UnityEngine;
using System.Collections.Generic;
using System;

public class CommandSystem : FrameSystem
{
	protected List<Command> mCommandBufferProcess;	// 用于处理的命令列表
	protected List<Command> mCommandBufferInput;	// 用于放入命令的命令列表,收集一帧中各个线程的命令
	protected List<Command> mExecuteList;           // 即将在这一帧执行的命令
	protected CommandPool mCommandPool;
	protected ThreadLock mBufferLock;
	public CommandSystem()
	{
		mBufferLock = new ThreadLock();
		mCommandPool = new CommandPool();
		mCommandBufferProcess = new List<Command>();
		mCommandBufferInput = new List<Command>();
		mExecuteList = new List<Command>();
	}
	public override void init()
	{
		base.init();
	}
	public override void destroy()
	{
		mCommandPool.destroy();
		mCommandPool = null;
		mCommandBufferInput.Clear();
		mCommandBufferProcess.Clear();
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 同步命令输入列表到命令处理列表中
		syncCommandBuffer();
		// 执行之前需要先清空列表
		mExecuteList.Clear();
		// 开始处理命令处理列表
		int count = mCommandBufferProcess.Count;
		for (int i = 0; i < count; ++i)
		{
			Command cmd = mCommandBufferProcess[i];
			if (!cmd.isIgnoreTimeScale())
			{
				cmd.setDelayTime(cmd.getDelayTime() - elapsedTime);
			}
			else
			{
				cmd.setDelayTime(cmd.getDelayTime() - Time.unscaledDeltaTime);
			}
			if (cmd.getDelayTime() <= 0.0f)
			{
				// 命令的延迟执行时间到了,则执行命令
				mExecuteList.Add(cmd);
				mCommandBufferProcess.RemoveAt(i);
				--i;
				--count;
			}
		}
		int executeCount = mExecuteList.Count;
		for (int i = 0; i < executeCount; ++i)
		{
			Command cmd = mExecuteList[i];
			cmd.setDelayCommand(false);
			if (cmd.getReceiver() != null)
			{
				pushCommand(cmd, cmd.getReceiver());
			}
		}
		// 执行完后清空列表
		mExecuteList.Clear();
	}
	// 创建命令
	public Command newCmd(Type type, bool show = true, bool delay = false)
	{
		// 如果命令系统已经销毁了,则不能再创建命令
		if (mDestroy)
		{
			return null;
		}
		return mCommandPool.newCmd(type, show, delay);
	}
	public void interruptCommand(List<int> assignIDList, bool showError = true)
	{
		int count = assignIDList.Count;
		for(int i = 0; i < count; ++i)
		{
			interruptCommand(assignIDList[i], showError);
		}
	}
	// 中断命令
	public bool interruptCommand(int assignID, bool showError = true)
	{
		// 如果命令系统已经销毁了,则不能再中断命令
		if(mDestroy)
		{
			return true;
		}
		if (assignID < 0)
		{
			if(showError)
			{
				logError("assignID invalid! : " + assignID);
			}
			return false;
		}

		syncCommandBuffer();
		int count = mCommandBufferProcess.Count;
		for(int i = 0; i < count; ++i)
		{
			Command cmd = mCommandBufferProcess[i];
			if (cmd.mAssignID == assignID)
			{
				log("CommandSystem : interrupt command " + assignID + " : " + cmd.showDebugInfo() + ", receiver : " + cmd.getReceiver().getName(), LOG_LEVEL.HIGH);
				mCommandBufferProcess.Remove(cmd);
				// 销毁回收命令
				mCommandPool.destroyCmd(cmd);
				return true;
			}
		}

		bool success = false;
		// 在即将执行的列表中查找,不能删除列表元素，只能将接收者设置为空来阻止命令执行,如果正在执行该命令,则没有效果
		int executeCount = mExecuteList.Count;
		for(int i = 0; i < executeCount; ++i)
		{
			Command cmd = mExecuteList[i];
			if (cmd.mAssignID == assignID)
			{
				cmd.setReceiver(null);
				success = true;
				break;
			}
		}
		if(!success && showError)
		{
			logError("not find cmd with assignID! " + assignID);
		}
		return success;
	}
	public void pushCommand(Type type, CommandReceiver cmdReceiver, bool show = true)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		Command cmd = newCmd(type, show, false);
		pushCommand(cmd, cmdReceiver);
	}
	// 执行命令
	public new void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		if (cmd == null)
		{
			logError("cmd is null! receiver : " + (cmdReceiver != null ? cmdReceiver.getName() : EMPTY));
			return;
		}
		if (cmdReceiver == null)
		{
			logError("receiver is null! cmd : " + (cmd != null ? cmd.GetType().ToString() : EMPTY));
			return;
		}
		if (!cmd.isValid())
		{
			logError("cmd is invalid! make sure create cmd use CommandSystem.newCmd! pushCommand cmd type : "
				+ Typeof(cmd) + "cmd id : " + cmd.mAssignID);
			return;
		}
		if (cmd.isDelayCommand())
		{
			logError("cmd is a delay cmd! can not use pushCommand!" + cmd.mAssignID + ", " + cmd.showDebugInfo());
			return;
		}
		cmd.setReceiver(cmdReceiver);
		if (cmd.isShowDebugInfo())
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			log("CommandSystem : " + cmd.mAssignID + ", " + cmd.showDebugInfo() + ", receiver : " + cmdReceiver.getName(), LOG_LEVEL.NORMAL);
#endif
		}
		cmdReceiver.receiveCommand(cmd);
		// 销毁回收命令
		mCommandPool?.destroyCmd(cmd);
	}
	public Command pushDelayCommand(Type type, CommandReceiver cmdReceiver, float delayExecute, bool show, IDelayCmdWatcher watcher)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return null;
		}
		Command cmd = newCmd(type, show, true);
		pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	// delayExecute是命令延时执行的时间,默认为0,只有new出来的命令才能延时执行
	// 子线程中发出的命令必须是延时执行的命令!
	public new void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute, IDelayCmdWatcher watcher)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		if (cmd == null)
		{
			logError("cmd is null! receiver : " + (cmdReceiver != null ? cmdReceiver.getName() : EMPTY));
			return;
		}
		if(cmdReceiver == null)
		{
			logError("receiver is null! cmd : " + (cmd != null ? cmd.GetType().ToString() : EMPTY));
			return;
		}
		if (!cmd.isValid())
		{
			logError("cmd is invalid! make sure create cmd use CommandSystem.newCmd! pushDelayCommand cmd type : "
				+ Typeof(cmd) + "cmd id : " + cmd.mAssignID);
			return;
		}
		if (!cmd.isDelayCommand())
		{
			logError("cmd is not a delay command, Command : " + cmd.mAssignID + ", " + cmd.showDebugInfo());
			return;
		}
		clampMin(ref delayExecute);
		if (cmd.isShowDebugInfo())
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			log("CommandSystem : delay cmd : " + cmd.mAssignID + ", " + delayExecute + ", info : " + cmd.showDebugInfo() + ", receiver : " + cmdReceiver.getName(), LOG_LEVEL.NORMAL);
#endif
		}
		cmd.setDelayTime(delayExecute);
		cmd.setReceiver(cmdReceiver);
		mBufferLock.waitForUnlock();
		mCommandBufferInput.Add(cmd);
		mBufferLock.unlock();
		watcher?.addDelayCmd(cmd);
	}
	public virtual void notifyReceiverDestroied(CommandReceiver receiver)
	{
		if (mDestroy)
		{
			return;
		}
		// 先同步命令列表
		syncCommandBuffer();
		int processCount = mCommandBufferProcess.Count;
		for (int i = 0; i < processCount; ++i)
		{
			Command cmd = mCommandBufferProcess[i];
			if (cmd.mReceiver == receiver)
			{
				mCommandPool.destroyCmd(cmd);
				mCommandBufferProcess.RemoveAt(i);
				--i;
				--processCount;
			}
		}
		// 执行列表中
		int count = mExecuteList.Count;
		for(int i = 0; i < count; ++i)
		{
			Command cmd = mExecuteList[i];
			// 已执行或正在执行的命令不作判断,该列表无法删除元素,只能将接收者设置为null
			if (cmd.mReceiver == receiver && cmd.mCmdState == EXECUTE_STATE.NOT_EXECUTE)
			{
				cmd.mReceiver = null;
			}
		}
	}
	//-----------------------------------------------------------------------------------------------------------------------------------------------------------------
	protected void syncCommandBuffer()
	{
		mBufferLock.waitForUnlock();
		mCommandBufferProcess.AddRange(mCommandBufferInput);
		mCommandBufferInput.Clear();
		mBufferLock.unlock();
	}
}