using System.Collections.Generic;
using System;

public abstract class SceneProcedure : IDelayCmdWatcher
{
	protected Dictionary<Type, SceneProcedure> mChildProcedureList; // 子流程列表
	protected HashSet<long> mDelayCmdList;		// 流程进入时的延迟命令列表,当命令执行时,会从列表中移除该命令	
	protected CommandCallback mCmdStartCallback;// 命令开始的回调,保存变量是为了避免GC
	protected SceneProcedure mParentProcedure;	// 父流程
	protected SceneProcedure mPrepareNext;      // 准备退出到的流程
	protected GameScene mGameScene;             // 流程所属的场景
	protected MyTimer mPrepareTimer;            // 准备退出的计时
	protected Type mType;						// 当前实例的类型,因为有些流程可能是在ILRuntime中,此处无法直接获取正确类型,所以需要保存一个变量
	protected string mPrepareIntent;			// 传递参数用
	protected bool mInited;                     // 是否已经初始化,子节点在初始化时需要先确保父节点已经初始化
	public SceneProcedure()
	{
		mDelayCmdList = new HashSet<long>();
		mChildProcedureList = new Dictionary<Type, SceneProcedure>();
		mPrepareTimer = new MyTimer();
		mPrepareIntent = null;
		mCmdStartCallback = onCmdStarted;
	}
	// 销毁场景时会调用流程的销毁
	public virtual void destroy() { }
	public void setType(Type type) { mType = type; }
	public void setGameScene(GameScene gameScene) { mGameScene = gameScene; }
	// 从自己的子流程进入当前流程
	protected virtual void onInitFromChild(SceneProcedure lastProcedure, string intent) { }
	// 在进入流程时调用
	// 在onInit中如果要跳转流程,必须使用延迟命令进行跳转
	protected abstract void onInit(SceneProcedure lastProcedure, string intent);
	// 更新流程时调用
	protected virtual void onUpdate(float elapsedTime) { }
	protected virtual void onLateUpdate(float elapsedTime) { }
	// 更新流程时调用
	protected virtual void onKeyProcess(float elapsedTime) { }
	// 退出当前流程,进入的不是自己的子流程时调用
	protected abstract void onExit(SceneProcedure nextProcedure);
	// 退出当前流程,进入自己的子流程时调用
	protected virtual void onExitToChild(SceneProcedure nextProcedure) { }
	// 退出当前流程进入其他任何流程时调用
	protected virtual void onExitSelf() { }
	// 进入的目标流程已经准备完成(资源加载完毕等等)时的回调
	public virtual void onNextProcedurePrepared(SceneProcedure nextPreocedure) { }
	protected virtual void onPrepareExit(SceneProcedure nextPreocedure) { }
	// 由GameScene调用
	// 进入流程
	public void init(SceneProcedure lastProcedure, string intent)
	{
		// 如果父节点还没有初始化,则先初始化父节点
		if (mParentProcedure != null && !mParentProcedure.mInited)
		{
			mParentProcedure.init(lastProcedure, intent);
			// 退出父节点自身而进入子节点
			mParentProcedure.onExitToChild(this);
			mParentProcedure.onExitSelf();
		}
		// 再初始化自己,如果是从子节点返回到父节点,则需要调用另外一个初始化函数
		if (lastProcedure != null && lastProcedure.isThisOrParent(mType))
		{
			onInitFromChild(lastProcedure, intent);
		}
		else
		{
			onInit(lastProcedure, intent);
		}
		mInited = true;
	}
	// 更新流程
	public void update(float elapsedTime)
	{
		// 先更新父节点
		mParentProcedure?.update(elapsedTime);
		// 再更新自己
		onUpdate(elapsedTime);
		// 检查准备退出流程
		if (mPrepareTimer.tickTimer(elapsedTime))
		{
			// 超过了准备时间,强制跳转流程
			CMD(out CmdGameSceneChangeProcedure cmd);
			cmd.mProcedure = mPrepareNext.mType;
			cmd.mIntent = mPrepareIntent;
			pushCommand(cmd, mGameScene);
		}
	}
	public void lateUpdate(float elapsedTime)
	{
		mParentProcedure?.lateUpdate(elapsedTime);
		onLateUpdate(elapsedTime);
	}
	// 退出流程
	public void exit(SceneProcedure exitTo, SceneProcedure nextPro)
	{
		// 中断自己所有未执行的命令
		interruptAllCommand();
		// 当停止目标为自己时,则不再退出,此时需要判断当前将要进入的流程是否为当前流程的子流程
		// 如果是,则需要调用onExitToChild,执行退出当前并且进入子流程的操作
		// 如果不是则不需要调用,不需要执行任何退出操作
		if (this == exitTo)
		{
			if (nextPro != null && nextPro.isThisOrParent(mType))
			{
				onExitToChild(nextPro);
				onExitSelf();
			}
			return;
		}
		// 先退出自己
		onExit(nextPro);
		onExitSelf();
		mInited = false;
		// 再退出父节点
		mParentProcedure?.exit(exitTo, nextPro);
		// 退出完毕后就修改标记
		mPrepareTimer.stop();
		mPrepareNext = null;
		mPrepareIntent = null;
	}
	public void prepareExit(SceneProcedure next, float time, string intent)
	{
		mPrepareTimer.init(0.0f, time, false);
		mPrepareNext = next;
		mPrepareIntent = intent;
		// 通知自己准备退出
		onPrepareExit(next);
	}
	public void keyProcess(float elapsedTime)
	{
		// 先处理父节点按键响应
		mParentProcedure?.keyProcess(elapsedTime);
		// 然后再处理自己的按键响应
		onKeyProcess(elapsedTime);
	}
	public void getParentList(List<SceneProcedure> parentList)
	{
		// 由于父节点列表中需要包含自己,所以先加入自己
		parentList.Add(this);
		// 再加入父节点的所有父节点
		mParentProcedure?.getParentList(parentList);
	}
	// 获得自己和otherProcedure的共同的父节点
	public SceneProcedure getSameParent(SceneProcedure otherProcedure)
	{
		// 获得两个流程的父节点列表
		LIST(out List<SceneProcedure> tempList0);
		LIST(out List<SceneProcedure> tempList1);
		getParentList(tempList0);
		otherProcedure.getParentList(tempList1);
		SceneProcedure sameParent = null;
		// 从前往后判断,找到第一个相同的父节点
		int count0 = tempList0.Count;
		for (int i = 0; i < count0; ++i)
		{
			SceneProcedure thisParent = tempList0[i];
			int count1 = tempList1.Count;
			for (int j = 0; j < count1; ++j)
			{
				if (thisParent == tempList1[j])
				{
					sameParent = thisParent;
					i = count0;
					break;
				}
			}
		}
		UN_LIST(tempList0);
		UN_LIST(tempList1);
		return sameParent;
	}
	public bool isThisOrParent(Type type)
	{
		// 判断是否是自己的类型
		if (mType == type)
		{
			return true;
		}
		// 判断是否为父节点的类型
		if (mParentProcedure != null)
		{
			return mParentProcedure.isThisOrParent(type);
		}
		// 没有父节点,返回false
		return false;
	}
	public Type getType() { return mType; }
	public GameScene getGameScene() { return mGameScene; }
	public SceneProcedure getParent() { return mParentProcedure; }
	public SceneProcedure getPrepareNext() { return mPrepareNext; }
	// 是否正在准备退出流程
	public bool isPreparingExit() { return mPrepareTimer.isCounting(); }
	public SceneProcedure getParent(Type type)
	{
		// 没有父节点,返回null
		if (mParentProcedure == null)
		{
			return null;
		}
		// 有父节点,则判断类型是否匹配,匹配则返回父节点
		if (mParentProcedure.mType == type)
		{
			return mParentProcedure;
		}
		// 不匹配,则继续向上查找
		return mParentProcedure.getParent(type);
	}
	public SceneProcedure getThisOrParent(Type type)
	{
		if (mType == type)
		{
			return this;
		}
		return getParent(type);
	}
	public SceneProcedure getChildProcedure(Type type)
	{
		mChildProcedureList.TryGetValue(type, out SceneProcedure procedure);
		return procedure;
	}
	public bool addChildProcedure(SceneProcedure child)
	{
		if (child == null)
		{
			return false;
		}
		if (mChildProcedureList.ContainsKey(child.mType))
		{
			return false;
		}
		child.setParent(this);
		mChildProcedureList.Add(child.mType, child);
		return true;
	}
	public override void addDelayCmd(Command cmd)
	{
		mDelayCmdList.Add(cmd.getAssignID());
		cmd.addStartCommandCallback(mCmdStartCallback);
	}
	public override void onCmdStarted(Command cmd)
	{
		if (!mDelayCmdList.Remove(cmd.getAssignID()))
		{
			logError("命令执行后移除流程命令失败");
		}
	}
	public override void interruptCommand(long assignID, bool showError = true)
	{
		if (mDelayCmdList.Remove(assignID))
		{
			mCommandSystem.interruptCommand(assignID, showError);
		}
	}
	public override void interruptAllCommand()
	{
		foreach(var item in mDelayCmdList)
		{
			mCommandSystem.interruptCommand(item, false);
		}
		mDelayCmdList.Clear();
	}
	//---------------------------------------------------------------------------------------------------------
	protected bool setParent(SceneProcedure parent)
	{
		if (mParentProcedure != null)
		{
			return false;
		}
		mParentProcedure = parent;
		return true;
	}
	
}