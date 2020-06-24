using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CommandSystem : FrameComponent
{
	protected CommandPool mCommandPool;
	protected List<Command> mCommandBufferProcess;	// 用于处理的命令列表
	protected List<Command> mCommandBufferInput;	// 用于放入命令的命令列表
	protected List<Command> mExecuteList;			// 即将在这一帧执行的命令
	protected ThreadLock mBufferLock;
	protected bool mTraceCommand;   // 是否追踪命令的来源
	public CommandSystem(string name)
		:base(name)
	{
		mBufferLock = new ThreadLock();
		mTraceCommand = false;
		mCommandPool = new CommandPool();
		mCommandBufferProcess = new List<Command>();
		mCommandBufferInput = new List<Command>();
		mExecuteList = new List<Command>();
	}
	public override void init()
	{
		base.init();
		mCommandPool.init();
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
		for (int i = 0; i < mCommandBufferProcess.Count; ++i)
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
			}
		}
		int executeCount = mExecuteList.Count;
		for (int i = 0; i < executeCount; ++i)
		{
			mExecuteList[i].setDelayCommand(false);
			if(mExecuteList[i].getReceiver() != null)
			{
				pushCommand(mExecuteList[i], mExecuteList[i].getReceiver());
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
		Command cmd = mCommandPool.newCmd(type, show, delay);
#if UNITY_EDITOR
		if (mTraceCommand)
		{
			int line = 0;
			string file = EMPTY_STRING;
			int frame = 2;
			while (true)
			{
				file = getCurSourceFileName(frame);
				line = getLineNum(frame);
				if (!file.EndsWith("LayoutTools.cs"))
				{
					break;
				}
				++frame;
			}
			cmd.mLine = line;
			cmd.mFile = file;
		}
#endif
		return cmd;
	}
	// 创建命令
	public T newCmd<T>(bool show = true, bool delay = false) where T : Command, new()
	{
		return newCmd(typeof(T), show, delay) as T;
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
		bool success = false;
		syncCommandBuffer();
		foreach (var item in mCommandBufferProcess)
		{
			if (item.mAssignID == assignID)
			{
				logInfo("CommandSystem : interrupt command " + assignID + " : " + item.showDebugInfo() + ", receiver : " + item.getReceiver().getName(), LOG_LEVEL.LL_HIGH);
				mCommandBufferProcess.Remove(item);
				// 销毁回收命令
				mCommandPool.destroyCmd(item);
				success = true;
				return true;
			}
		}
		// 在即将执行的列表中查找,不能删除列表元素，只能将接收者设置为空来阻止命令执行,如果正在执行该命令,则没有效果
		foreach (var item in mExecuteList)
		{
			if (item.mAssignID == assignID)
			{
				item.setReceiver(null);
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
	public new void pushCommand<T>(CommandReceiver cmdReceiver, bool show = true) where T : Command, new()
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		T cmd = newCmd<T>(show, false);
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
			logError("cmd is null! receiver : " + (cmdReceiver != null ? cmdReceiver.getName() : EMPTY_STRING));
			return;
		}
		if (cmdReceiver == null)
		{
			logError("receiver is null! cmd : " + (cmd != null ? cmd.getType().ToString() : EMPTY_STRING));
			return;
		}
		if (!cmd.isValid())
		{
			logError("cmd is invalid! make sure create cmd use CommandSystem.newCmd! pushCommand cmd type : "
				+ cmd.GetType().ToString() + "cmd id : " + cmd.mAssignID);
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
			logInfo("CommandSystem : " + cmd.mAssignID + ", " + cmd.showDebugInfo() + ", receiver : " + cmdReceiver.getName(), LOG_LEVEL.LL_NORMAL);
		}
		cmdReceiver.receiveCommand(cmd);
		// 销毁回收命令
		mCommandPool?.destroyCmd(cmd);
	}
	public new T pushDelayCommand<T>(CommandReceiver cmdReceiver, float delayExecute = 0.001f, bool show = true) where T : Command, new()
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return null;
		}
		T cmd = newCmd<T>(show, true);
		pushDelayCommand(cmd, cmdReceiver, delayExecute);
		return cmd;
	}
	// delayExecute是命令延时执行的时间,默认为0,只有new出来的命令才能延时执行
	// 子线程中发出的命令必须是延时执行的命令!
	public new void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute = 0.001f)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy)
		{
			return;
		}
		if (cmd == null)
		{
			logError("cmd is null! receiver : " + (cmdReceiver != null ? cmdReceiver.getName() : EMPTY_STRING));
			return;
		}
		if(cmdReceiver == null)
		{
			logError("receiver is null! cmd : " + (cmd != null ? cmd.getType().ToString() : EMPTY_STRING));
			return;
		}
		if (!cmd.isValid())
		{
			logError("cmd is invalid! make sure create cmd use CommandSystem.newCmd! pushDelayCommand cmd type : "
				+ cmd.GetType().ToString() + "cmd id : " + cmd.mAssignID);
			return;
		}
		if (!cmd.isDelayCommand())
		{
			logError("cmd is not a delay command, Command : " + cmd.mAssignID + ", " + cmd.showDebugInfo());
			return;
		}
		clampMin(ref delayExecute, 0.0f);
		if (cmd.isShowDebugInfo())
		{
			logInfo("CommandSystem : delay cmd : " + cmd.mAssignID + ", " + delayExecute + ", info : " + cmd.showDebugInfo() + ", receiver : " + cmdReceiver.getName(), LOG_LEVEL.LL_NORMAL);
		}
		mBufferLock.waitForUnlock();
		cmd.setDelayTime(delayExecute);
		cmd.setReceiver(cmdReceiver);
		mCommandBufferInput.Add(cmd);
		mBufferLock.unlock();
	}
	public virtual void notifyReceiverDestroied(CommandReceiver receiver)
	{
		if (mDestroy)
		{
			return;
		}
		// 先同步命令列表
		syncCommandBuffer();
		for (int i = 0; i < mCommandBufferProcess.Count; ++i)
		{
			if (mCommandBufferProcess[i].mReceiver == receiver)
			{
				mCommandPool.destroyCmd(mCommandBufferProcess[i]);
				mCommandBufferProcess.RemoveAt(i);
				--i;
			}
		}
		// 执行列表中
		int count = mExecuteList.Count;
		for(int i = 0; i < count; ++i)
		{
			// 已执行或正在执行的命令不作判断,该列表无法删除元素,只能将接收者设置为null
			if (mExecuteList[i].mReceiver == receiver && mExecuteList[i].mExecuteState == EXECUTE_STATE.ES_NOT_EXECUTE)
			{
				mExecuteList[i].mReceiver = null;
			}
		}
	}
	//-----------------------------------------------------------------------------------------------------------------------------------------------------------------
	protected void syncCommandBuffer()
	{
		mBufferLock.waitForUnlock();
		int inputCount = mCommandBufferInput.Count;
		for (int i = 0; i < inputCount; ++i)
		{
			mCommandBufferProcess.Add(mCommandBufferInput[i]);
		}
		mCommandBufferInput.Clear();
		mBufferLock.unlock();
	}
}