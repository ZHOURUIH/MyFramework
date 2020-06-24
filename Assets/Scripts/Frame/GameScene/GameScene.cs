using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class GameScene : ComponentOwner
{
	protected Dictionary<Type, SceneProcedure>	mSceneProcedureList;
	protected List<SceneProcedure>				mLastProcedureList;  // 所进入过的所有流程
	protected Type				mSceneType;
	protected Type				mStartProcedure;
    protected Type				mTempStartProcedure;
	protected Type				mExitProcedure;			// 场景的退出流程,一般用作资源卸载
	protected Type				mLastProcedureType;
	protected SceneProcedure	mCurProcedure;
	protected GameObject		mSceneObject;
	protected AudioSource		mAudioSource;
	protected const int			mMaxLastProcedureCount = 8;
	protected string			mTempStartIntent;
	public GameScene(string name) 
        :base(name)
    {
		mLastProcedureType = null;
        mTempStartProcedure = null;
        mSceneProcedureList = new Dictionary<Type, SceneProcedure>();
		mLastProcedureList = new List<SceneProcedure>();
		// 创建场景对应的物体,并挂接到场景管理器下
		mSceneObject = createGameObject(name, mGameSceneManager.getObject());
		mAudioSource = mSceneObject.GetComponent<AudioSource>();
	}
	public void setType(Type type) { mSceneType = type; }
	// 进入场景时初始化
    public virtual void init()
    {
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
	public void enterStartProcedure()
	{
		CommandGameSceneChangeProcedure cmd = newCmd(out cmd);
		cmd.mProcedure = mTempStartProcedure != null ? mTempStartProcedure : mStartProcedure;
		cmd.mIntent = mTempStartIntent;
		pushCommand(cmd, this);
	}
	public override void destroy()
	{
		base.destroy();
		// 销毁所有流程
		foreach(var item in mSceneProcedureList)
		{
			item.Value.destroy();
		}
		mSceneProcedureList.Clear();
		destroyAllComponents();
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
		if(!mGameFramework.isEnableKeyboard())
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
		CommandGameSceneChangeProcedure cmd = newCmd(out cmd);
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
	public bool atProcedure<T>() where T : SceneProcedure
	{
		return atProcedure(typeof(T));
	}
	public bool atProcedure(Type type)
	{
		if (mCurProcedure == null)
		{
			return false;
		}
		return mCurProcedure.isThisOrParent(type);
	}
	// 是否在指定的流程,不考虑子流程
	public bool atSelfProcedure<T>() where T : SceneProcedure
	{
		return atSelfProcedure(typeof(T));
	}
	public bool atSelfProcedure(Type type)
	{
		if(mCurProcedure == null)
		{
			return false;
		}
		return mCurProcedure.getProcedureType() == type;
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
	public bool changeProcedure(Type procedure, string intent, bool addToLastList = true)
    {
        if (mSceneProcedureList.ContainsKey(procedure))
        {
			// 将上一个流程记录到返回列表中
			if (mCurProcedure != null && addToLastList)
			{
				mLastProcedureList.Add(mCurProcedure);
				if(mLastProcedureList.Count > mMaxLastProcedureCount)
				{
					mLastProcedureList.RemoveAt(0);
				}
			}
			if (mCurProcedure == null || mCurProcedure.getProcedureType() != procedure)
			{
				SceneProcedure targetProcedure = mSceneProcedureList[procedure];
				// 如果当前已经在一个流程中了,则要先退出当前流程,但是不要销毁流程
				if (mCurProcedure != null)
				{
					// 需要找到共同的父节点,退到该父节点时则不再退出
					SceneProcedure exitTo = mCurProcedure.getSameParent(targetProcedure);
					SceneProcedure nextPro = targetProcedure;
					mCurProcedure.exit(exitTo, nextPro);
				}
				SceneProcedure lastProcedure = mCurProcedure;
				mCurProcedure = targetProcedure;
				mCurProcedure.init(lastProcedure, intent);
			}
            return true;
        }
        else
        {
			logError("can not find scene procedure : " + procedure);
        }
        return false;
    }
	// 流程调用,通知场景当前流程已经准备完毕
	public void notifyProcedurePrepared()
	{
		if(mLastProcedureList.Count > 0)
		{
			mLastProcedureList[mLastProcedureList.Count - 1].onNextProcedurePrepared(mCurProcedure);
		}
	}
	//  获取上一个流程
	public Type getLastProcedureType()
	{
		return mLastProcedureList[mLastProcedureList.Count - 1].getProcedureType();
	}
	public T getProcedure<T>() where T : SceneProcedure
	{
		return getProcedure(typeof(T)) as T;
	}
	public SceneProcedure getProcedure(Type type)
    {
		return mSceneProcedureList.ContainsKey(type) ? mSceneProcedureList[type] : null;
    }
	public SceneProcedure getProcedure(string typeStr)
	{
		foreach (var item in mSceneProcedureList)
		{
			if(item.Key.ToString() == typeStr)
			{
				return item.Value;
			}
		}
		return null;
	}
	public Type getCurProcedureType() { return mCurProcedure.getProcedureType(); }
	public SceneProcedure getCurProcedure() { return mCurProcedure; }
	public T getCurOrParentProcedure<T>() where T : SceneProcedure
	{
		return mCurProcedure.getThisOrParent(typeof(T)) as T;
	}
    public void setTempStartProcedure(Type procedure ) { mTempStartProcedure = procedure; }
    public void setTempStartIntent( string intent) { mTempStartIntent = intent; }
	public Type getSceneType() { return mSceneType; }
	public static T createProcedure<T>(GameScene gameScene) where T : SceneProcedure
	{
		T procedure = createInstance<T>(typeof(T), gameScene);
		procedure.setType(typeof(T));
		return procedure;
	}
	public T addProcedure<T>(Type parent = null) where T : SceneProcedure
	{
		SceneProcedure procedure = createProcedure<T>(this);
		if (parent != null)
		{
			SceneProcedure parentProcedure = getProcedure(parent);
			if(parentProcedure == null)
			{
				logError("invalid parent procedure, procedure:" + procedure.getProcedureType());
			}
			parentProcedure.addChildProcedure(procedure);
		}
		mSceneProcedureList.Add(procedure.getProcedureType(), procedure);
		return procedure as T;
	}
}