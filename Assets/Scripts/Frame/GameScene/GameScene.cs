using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class GameScene : ComponentOwner
{
	protected Dictionary<Type, SceneProcedure> mSceneProcedureList; // 场景的流程列表
	protected List<SceneProcedure> mLastProcedureList;  // 所进入过的所有流程
	protected SceneProcedure mCurProcedure;         // 当前流程
	protected GameObject mSceneObject;              // 场景对应的GameObject
	protected AudioSource mAudioSource;             // 场景音频源
	protected Type mStartProcedure;                 // 起始流程类型,进入场景时会默认进入该流程
	protected Type mTempStartProcedure;             // 仅使用一次的起始流程类型,设置后进入场景时会默认进入该流程,生效后就清除
	protected Type mExitProcedure;                  // 场景的退出流程,退出场景进入其他场景时会先进入该流程,一般用作资源卸载
	protected const int mMaxLastProcedureCount = 8; // mLastProcedureList列表的最大长度,当超过该长度时,会移除列表开始的元素
	protected string mTempStartIntent;              // 进入mTempStartProcedure时的参数
	public GameScene()
	{
		mSceneProcedureList = new Dictionary<Type, SceneProcedure>();
		mLastProcedureList = new List<SceneProcedure>();
	}
	// 进入场景时初始化
	public virtual void init()
	{
		// 创建场景对应的物体,并挂接到场景管理器下
		mSceneObject = createGameObject(mName, mGameSceneManager.getObject());
		mAudioSource = mSceneObject.GetComponent<AudioSource>();
#if UNITY_EDITOR
		mSceneObject.AddComponent<GameSceneDebug>();
#endif
		initComponents();
		// 创建出所有的场景流程
		createSceneProcedure();
		// 设置起始流程名
		assignStartExitProcedure();
		// 开始执行起始流程
		enterStartProcedure();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mSceneProcedureList.Clear();
		mLastProcedureList.Clear();
		mCurProcedure = null;
		mSceneObject = null;
		mAudioSource = null;
		mStartProcedure = null;
		mTempStartProcedure = null;
		mExitProcedure = null;
		mTempStartIntent = null;
	}
	public void enterStartProcedure()
	{
		CMD(out CommandGameSceneChangeProcedure cmd);
		cmd.mProcedure = mTempStartProcedure != null ? mTempStartProcedure : mStartProcedure;
		cmd.mIntent = mTempStartIntent;
		pushCommand(cmd, this);
		mTempStartProcedure = null;
	}
	public override void destroy()
	{
		base.destroy();
		// 销毁所有流程
		foreach (var item in mSceneProcedureList)
		{
			item.Value.destroy();
		}
		mSceneProcedureList.Clear();
		destroyGameObject(ref mSceneObject);
	}
	public override void update(float elapsedTime)
	{
		// 更新组件
		base.update(elapsedTime);
		// 更新当前流程
		keyProcess(elapsedTime);
		mCurProcedure?.update(elapsedTime);
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		mCurProcedure?.lateUpdate(elapsedTime);
	}
	public virtual void keyProcess(float elapsedTime)
	{
		if (!mGameFramework.isEnableKeyboard())
		{
			return;
		}
		// 在准备退出当前流程时,不响应任何按键操作
		if (mCurProcedure != null && !mCurProcedure.isPreparingExit())
		{
			mCurProcedure.keyProcess(elapsedTime);
		}
	}
	// 退出场景
	public virtual void exit()
	{
		// 首先进入退出流程,然后再退出最后的流程
		CMD(out CommandGameSceneChangeProcedure cmd);
		cmd.mProcedure = mExitProcedure;
		pushCommand(cmd, this);
		mCurProcedure?.exit(null, null);
		mCurProcedure = null;
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}
	public AudioSource getAudioSource() { return mAudioSource; }
	public abstract void assignStartExitProcedure();
	public AudioSource createAudioSource()
	{
		return mAudioSource = mSceneObject.AddComponent<AudioSource>();
	}
	public virtual void createSceneProcedure() { }
	public bool atProcedure(Type type)
	{
		if (mCurProcedure == null)
		{
			return false;
		}
		return mCurProcedure.isThisOrParent(type);
	}
	// 是否在指定的流程,不考虑子流程
	public bool atSelfProcedure(Type type)
	{
		if (mCurProcedure == null)
		{
			return false;
		}
		return mCurProcedure.getType() == type;
	}
	public new void prepareChangeProcedure(Type procedure, float time, string intent)
	{
		SceneProcedure targetProcedure = mSceneProcedureList[procedure];
		mCurProcedure.prepareExit(targetProcedure, time, intent);
	}
	public void backToLastProcedure(string intend)
	{
		if (mLastProcedureList.Count == 0)
		{
			return;
		}
		// 获得上一次进入的流程
		Type lastType = getLastProcedureType();
		changeProcedure(lastType, intend, false);
		mLastProcedureList.RemoveAt(mLastProcedureList.Count - 1);
	}
	public bool changeProcedure(Type procedureType, string intent, bool addToLastList = true)
	{
		if (!mSceneProcedureList.TryGetValue(procedureType, out SceneProcedure targetProcedure))
		{
			logError("can not find scene procedure : " + procedureType);
			return false;
		}
		// 将上一个流程记录到返回列表中
		if (mCurProcedure != null && addToLastList)
		{
			mLastProcedureList.Add(mCurProcedure);
			if (mLastProcedureList.Count > mMaxLastProcedureCount)
			{
				mLastProcedureList.RemoveAt(0);
			}
		}
		if (mCurProcedure == null || mCurProcedure.getType() != procedureType)
		{
			// 如果当前已经在一个流程中了,则要先退出当前流程,但是不要销毁流程
			if (mCurProcedure != null)
			{
				// 需要找到共同的父节点,退到该父节点时则不再退出
				mCurProcedure.exit(mCurProcedure.getSameParent(targetProcedure), targetProcedure);
			}
			SceneProcedure lastProcedure = mCurProcedure;
			mCurProcedure = targetProcedure;
			mCurProcedure.init(lastProcedure, intent);
		}
		return true;
	}
	// 流程调用,通知场景当前流程已经准备完毕
	public void notifyProcedurePrepared()
	{
		if (mLastProcedureList.Count > 0)
		{
			mLastProcedureList[mLastProcedureList.Count - 1].onNextProcedurePrepared(mCurProcedure);
		}
	}
	//  获取上一个流程
	public Type getLastProcedureType()
	{
		return mLastProcedureList[mLastProcedureList.Count - 1].getType();
	}
	public SceneProcedure getProcedure(Type type)
	{
		mSceneProcedureList.TryGetValue(type, out SceneProcedure procedure);
		return procedure;
	}
	public Type getCurProcedureType() { return mCurProcedure.getType(); }
	public SceneProcedure getCurProcedure() { return mCurProcedure; }
	// 获取当前场景的当前流程或父流程中指定类型的流程
	public SceneProcedure getCurOrParentProcedure(Type type)
	{
		return mCurProcedure.getThisOrParent(type);
	}
	public void setTempStartProcedure(Type procedure, string intent)
	{
		mTempStartProcedure = procedure;
		mTempStartIntent = intent;
	}
	public SceneProcedure addProcedure(Type type, Type parent = null)
	{
		SceneProcedure procedure = createInstance<SceneProcedure>(type);
		procedure.setGameScene(this);
		procedure.setType(type);
		if (parent != null)
		{
			SceneProcedure parentProcedure = getProcedure(parent);
			if (parentProcedure == null)
			{
				logError("invalid parent procedure, procedure:" + procedure.getType());
			}
			parentProcedure.addChildProcedure(procedure);
		}
		mSceneProcedureList.Add(procedure.getType(), procedure);
		return procedure;
	}
}