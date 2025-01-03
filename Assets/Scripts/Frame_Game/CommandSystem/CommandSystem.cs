using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityUtility;
using static StringUtility;
using static CSharpUtility;
using static MathUtility;
using static FrameBase;
using static FrameEditorUtility;

// 命令系统,用于处理所有命令相关逻辑
public class CommandSystem : FrameSystem
{
	protected List<Command> mCommandBufferProcess = new();	// 用于处理的命令列表
	protected List<Command> mCommandBufferInput = new();	// 用于放入命令的命令列表,收集一帧中各个线程的命令
	protected List<Command> mExecuteList = new();			// 即将在这一帧执行的命令
	protected ThreadLock mBufferLock = new();				// mCommandBufferInput的线程锁
	public override void destroy()
	{
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
				mCommandBufferProcess.RemoveAt(i--);
				--count;
			}
		}
		foreach (Command cmd in mExecuteList)
		{
			cmd.setDelayCommand(false);
			CommandReceiver receiver = cmd.getReceiver();
			if (receiver != null)
			{
				receiver.removeReceiveDelayCmd();
				pushCommand(cmd, receiver);
			}
		}
		// 执行完后清空列表
		mExecuteList.Clear();
	}
	// 中断命令
	public bool interruptCommand(long assignID, bool showError = true)
	{
		// 如果命令系统已经销毁了,则不能再中断命令
		if (mDestroy)
		{
			return true;
		}
		if (!isMainThread())
		{
			logError("只能在主线程中中断命令");
		}
		if (assignID < 0)
		{
			if (showError)
			{
				logError("assignID invalid! : " + assignID);
			}
			return false;
		}

		syncCommandBuffer();
		int count = mCommandBufferProcess.Count;
		for (int i = 0; i < count; ++i)
		{
			Command cmd = mCommandBufferProcess[i];
			if (cmd.getAssignID() == assignID)
			{
				using var a = new MyStringBuilderScope(out var builder);
				cmd.onInterrupted();
				builder.append("CMD : interrupt command ", LToS(assignID), " : ");
				cmd.debugInfo(builder);
				builder.append(", receiver : ", cmd.getReceiver().getName());
				log(builder.ToString(), LOG_LEVEL.HIGH);
				mCommandBufferProcess.RemoveAt(i);
				// 销毁回收命令
				destroyCmd(cmd);
				return true;
			}
		}

		bool success = false;
		// 在即将执行的列表中查找,不能删除列表元素，只能将接收者设置为空来阻止命令执行,如果正在执行该命令,则没有效果
		foreach (Command cmd in mExecuteList)
		{
			// 为了确保一定不会出现在命令执行过程中中断了当前正在执行的命令,以及已经执行过的命令
			if (cmd.getAssignID() == assignID && cmd.getReceiver() != null && cmd.isDelayCommand())
			{
				cmd.getReceiver().removeReceiveDelayCmd();
				cmd.setReceiver(null);
				success = true;
				break;
			}
		}
		if (!success && showError)
		{
			logError("not find cmd with assignID! " + assignID);
		}
		return success;
	}
	// 在当前线程中执行命令
	public void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy || cmd == null || cmdReceiver == null)
		{
			return;
		}
		if (cmd.isDestroy())
		{
			logError("cmd is invalid! make sure create cmd use CommandSystem.newCmd! pushCommand cmd type : "
				+ cmd.GetType() + "cmd id : " + cmd.getAssignID());
			return;
		}
		if (cmd.isDelayCommand())
		{
			using var a = new MyStringBuilderScope(out var builder);
			builder.append("cmd is a delay cmd! can not use pushCommand!", LToS(cmd.getAssignID()), ", ");
			cmd.debugInfo(builder);
			logError(builder.ToString());
			return;
		}

		cmd.setReceiver(cmdReceiver);
		if (isEditor() || isDevelopment())
		{
			if (cmd.getCmdLogLevel() >= getLogLevel())
			{
				if (isMainThread())
				{
					using var a = new MyStringBuilderScope(out var builder);
					builder.append(cmd.GetType().ToString(), " : ", LToS(cmd.getAssignID()), ", ");
					cmd.debugInfo(builder);
					builder.append(", receiver : ", cmdReceiver.getName());
					log(builder.ToString(), cmd.getCmdLogLevel());
				}
				else
				{
					using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
					builder.append(cmd.GetType().ToString(), " : ", LToS(cmd.getAssignID()), ", ");
					cmd.debugInfo(builder);
					builder.append(", receiver : ", cmdReceiver.getName());
					log(builder.ToString(), cmd.getCmdLogLevel());
				}
			}
		}
		cmd.invokeStartCallBack();
		try
		{
			using var a = new ProfilerScope(cmd.GetType().ToString());
			cmd.execute();
		}
		catch (Exception e)
		{
			logException(e);
		}
		cmd.invokeEndCallBack();
		// 销毁回收命令
		destroyCmd(cmd);
	}
	// 延迟执行命令,会延迟到主线程执行
	public void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute, DelayCmdWatcher watcher)
	{
		// 如果命令系统已经销毁了,则不能再发送命令
		if (mDestroy || cmd == null || cmdReceiver == null)
		{
			return;
		}
		if (watcher != null && watcher.isDestroy())
		{
			return;
		}
		cmdReceiver.addReceiveDelayCmd();
		if (cmd.isDestroy())
		{
			logError("cmd is invalid! make sure create cmd use CommandSystem.newCmd! pushDelayCommand cmd type : "
				+ cmd.GetType() + "cmd id : " + cmd.getAssignID());
			return;
		}
		if (!cmd.isDelayCommand())
		{
			using var a = new MyStringBuilderScope(out var builder);
			builder.append("cmd is not a delay command, Command : ", LToS(cmd.getAssignID()), ", ");
			cmd.debugInfo(builder);
			logError(builder.ToString());
			return;
		}
		clampMin(ref delayExecute);
		if (isEditor() || isDevelopment())
		{
			if (cmd.getCmdLogLevel() >= getLogLevel())
			{
				if (isMainThread())
				{
					using var b = new MyStringBuilderScope(out var builder);
					builder.append("CMD : delay cmd : ", LToS(cmd.getAssignID()), ", ", FToS(delayExecute), ", info : ");
					cmd.debugInfo(builder);
					builder.append(", receiver : ", cmdReceiver.getName());
					log(builder.ToString(), cmd.getCmdLogLevel());
				}
				else
				{
					using var c = new ClassThreadScope<MyStringBuilder>(out var builder);
					builder.append("CMD : delay cmd : ", LToS(cmd.getAssignID()), ", ", FToS(delayExecute), ", info : ");
					cmd.debugInfo(builder);
					builder.append(", receiver : ", cmdReceiver.getName());
					log(builder.ToString(), cmd.getCmdLogLevel());
				}
			}
		}
		cmd.setDelayTime(delayExecute);
		cmd.setReceiver(cmdReceiver);
		if (isMainThread())
		{
			mCommandBufferProcess.Add(cmd);
		}
		else
		{
			using (new ThreadLockScope(mBufferLock))
			{
				mCommandBufferInput.Add(cmd);
			}
		}
		watcher?.addDelayCmd(cmd);
	}
	public void notifyReceiverDestroied(CommandReceiver receiver)
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
			if (cmd.getReceiver() == receiver)
			{
				destroyCmd(cmd);
				mCommandBufferProcess.RemoveAt(i--);
				--processCount;
			}
		}
		// 执行列表中
		foreach (Command cmd in mExecuteList)
		{
			// 已执行或正在执行的命令不作判断,该列表无法删除元素,只能将接收者设置为null
			if (cmd.getReceiver() == receiver)
			{
				cmd.setReceiver(null);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void syncCommandBuffer()
	{
		if (mCommandBufferInput.Count == 0)
		{
			return;
		}
		using (new ThreadLockScope(mBufferLock))
		{
			mCommandBufferProcess.move(mCommandBufferInput);
		}
	}
	protected void destroyCmd(Command cmd)
	{
		if (cmd == null)
		{
			return;
		}
		if (cmd.isThreadCommand())
		{
			mClassPoolThread?.destroyClass(ref cmd);
		}
		else
		{
			mClassPool?.destroyClass(ref cmd);
		}
	}
}