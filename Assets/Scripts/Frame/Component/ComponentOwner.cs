using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static CSharpUtility;

// 组件拥有者,只有继承了组件拥有者的类才能够添加组件
public abstract class ComponentOwner : CommandReceiver
{
	protected SafeDictionary<Type, GameComponent> mAllComponentTypeList;	// 组件类型列表,first是组件的类型名
	protected SafeList<GameComponent> mComponentList;						// 组件列表,保存着组件之间的更新顺序
	protected HashSet<Type> mDontAutoCreateType;                            // 不需要自动的组件类型
	protected HashSet<Type> mDisableTypeList;								// 不需要更新的组件类型,为了不需要关心组件的添加或者销毁,只是想禁用部分组件的更新
	protected bool mIgnoreTimeScale;                                        // 是否忽略时间缩放
	protected bool mDestroying;												// 是否正在销毁中,避免销毁时执行无用的操作
	public ComponentOwner()
	{
		mAllComponentTypeList = new SafeDictionary<Type, GameComponent>();
		mComponentList = new SafeList<GameComponent>();
	}
	public override void destroy()
	{
		mDestroying = true;
		foreach (var item in mAllComponentTypeList.startForeach())
		{
			var temp = item.Value;
			if (temp == null)
			{
				continue;
			}
			temp.destroy();
			UN_CLASS(ref temp);
		}
		mDestroying = false;
		mAllComponentTypeList.endForeach();
		mAllComponentTypeList.clear();
		mComponentList.clear();
		mDontAutoCreateType?.Clear();
		mDisableTypeList?.Clear();
		base.destroy();
	}
	public virtual void setActive(bool active)
	{
		if (mDestroying)
		{
			return;
		}
		foreach (var item in mAllComponentTypeList.startForeach())
		{
			item.Value?.notifyOwnerActive(active);
		}
		mAllComponentTypeList.endForeach();
	}
	// 更新正常更新的组件
	public virtual void update(float elapsedTime)
	{
		if (mComponentList.count() == 0)
		{
			return;
		}

		var updateList = mComponentList.startForeach();
		int rootComponentCount = updateList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			// 数量为0,表示在更新组件的过程中当前对象被销毁
			if (updateList.Count == 0)
			{
				mComponentList.endForeach();
				return;
			}
			GameComponent com = updateList[i];
			if (com != null && com.isActive() && (mDisableTypeList == null || !mDisableTypeList.Contains(com.getType())))
			{
				if (!com.isIgnoreTimeScale())
				{
					com.update(elapsedTime);
				}
				else
				{
					com.update(Time.unscaledDeltaTime);
				}
			}
		}
		mComponentList.endForeach();
	}
	// 后更新
	public virtual void lateUpdate(float elapsedTime)
	{
		var updateList = mComponentList.startForeach();
		int rootComponentCount = updateList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent com = updateList[i];
			if (com != null && com.isActive() && (mDisableTypeList == null || !mDisableTypeList.Contains(com.getType())))
			{
				if (!com.isIgnoreTimeScale())
				{
					com.lateUpdate(elapsedTime);
				}
				else
				{
					com.lateUpdate(Time.unscaledDeltaTime);
				}
			}
		}
		mComponentList.endForeach();
	}
	// 物理更新
	public virtual void fixedUpdate(float elapsedTime)
	{
		var updateList = mComponentList.startForeach();
		int rootComponentCount = updateList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent com = updateList[i];
			if (com != null && com.isActive() && (mDisableTypeList == null || !mDisableTypeList.Contains(com.getType())))
			{
				if (!com.isIgnoreTimeScale())
				{
					com.fixedUpdate(elapsedTime);
				}
				else
				{
					com.fixedUpdate(Time.unscaledDeltaTime);
				}
			}
		}
		mComponentList.endForeach();
	}
	public virtual void notifyAddComponent(GameComponent com) { }
	public GameComponent addComponent(Type type, bool active = false)
	{
		// 不能创建重名的组件
		if (mAllComponentTypeList.containsKey(type))
		{
			logError("已经存在相同类型的组件了:" + type);
			return null;
		}
		var com = CLASS(type) as GameComponent;
		if (com == null)
		{
			return null;
		}
		com.setType(type);
		com.init(this);
		// 将组件加入列表
		addComponentToList(com);
		// 通知创建了组件
		notifyAddComponent(com);
		com.setActive(active);
		com.setDefaultActive(active);
		return com;
	}
	public T addComponent<T>(bool active = false) where T : GameComponent
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用addComponent<T>(bool active = false)添加非主工程中的组件");
			logError("如果需要获取非主工程中的组件,使用GameComponent addComponent(Type type, bool active = false)");
		}
		// 不能创建重名的组件
		if (mAllComponentTypeList.containsKey(type))
		{
			logError("已经存在相同类型的组件了:" + type);
			return null;
		}
		T com = CLASS<T>();
		if (com == null)
		{
			return null;
		}
		com.setType(type);
		com.init(this);
		// 将组件加入列表
		addComponentToList(com);
		// 通知创建了组件
		notifyAddComponent(com);
		com.setActive(active);
		com.setDefaultActive(active);
		return com;
	}
	public void addComponent<T>(out T component, bool active = false) where T : GameComponent
	{
		component = addComponent<T>(active);
	}
	public GameComponent addInitComponent(Type type, bool active = false)
	{
		if (mDontAutoCreateType != null && mDontAutoCreateType.Contains(type))
		{
			return null;
		}
		return addComponent(type, active);
	}
	public T addInitComponent<T>(bool active = false) where T : GameComponent
	{
		if (mDontAutoCreateType != null && mDontAutoCreateType.Contains(typeof(T)))
		{
			return null;
		}
		return addComponent<T>(active);
	}
	public void addInitComponent<T>(out T component, bool active = false) where T : GameComponent
	{
		if (mDontAutoCreateType != null && mDontAutoCreateType.Contains(typeof(T)))
		{
			component = null;
			return;
		}
		component = addComponent<T>(active);
	}
	public SafeDictionary<Type, GameComponent> getAllComponent() { return mAllComponentTypeList; }
	public T getComponent<T>(bool needActive = false, bool addIfNull = true) where T : GameComponent
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用getComponent<T>(bool needActive, bool addIfNull)获取非主工程中的组件");
			logError("如果需要获取非主工程中的组件,使用T getComponent<T>(Type type, bool needActive, bool addIfNull)");
			return null;
		}
		return getComponent(type, needActive, addIfNull) as T;
	}
	public T getComponent<T>(out T com, bool needActive = false, bool addIfNull = true) where T : GameComponent
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用getComponent<T>(out T com, bool needActive, bool addIfNull)获取非主工程中的组件");
			logError("如果需要获取非主工程中的组件,使用T getComponent<T>(Type type, bool needActive, bool addIfNull)");
			return com = null;
		}
		return com = getComponent(type, needActive, addIfNull) as T;
	}
	public GameComponent getComponent(Type type, bool needActive = false, bool addIfNull = true)
	{
		if (mAllComponentTypeList.tryGetValue(type, out GameComponent com))
		{
			if (!needActive || com.isActive())
			{
				return com;
			}
		}
		if (addIfNull)
		{
			return addComponent(type);
		}
		return null;
	}
	public void removeDisableComponent(Type type)
	{
		mDisableTypeList?.Remove(type);
	}
	public void addDisableComponent(Type type)
	{
		if (mDisableTypeList == null)
		{
			mDisableTypeList = new HashSet<Type>();
		}
		mDisableTypeList.Add(type);
	}
	public void activeComponent(Type type, bool active = true, bool addIfNull = false)
	{
		GameComponent com = getComponent(type, false, addIfNull);
		com?.setActive(active);
	}
	public void notifyComponentStart(GameComponent com)
	{
		if (com is IComponentModifyAlpha)
		{
			breakComponent<IComponentModifyAlpha>(com);
		}
		if (com is IComponentModifyColor)
		{
			breakComponent<IComponentModifyColor>(com);
		}
		if (com is IComponentModifyPosition)
		{
			breakComponent<IComponentModifyPosition>(com);
		}
		if (com is IComponentModifyRotation)
		{
			breakComponent<IComponentModifyRotation>(com);
		}
		if (com is IComponentModifyScale)
		{
			breakComponent<IComponentModifyScale>(com);
		}
	}
	public SafeList<GameComponent> getComponentList() { return mComponentList; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	// componentOnly表示是否只设置组件不受时间缩放影响,如果只有组件受影响,则Owner本身还是受到时间缩放影响的
	public virtual void setIgnoreTimeScale(bool ignore, bool componentOnly = false)
	{
		if (!componentOnly)
		{
			mIgnoreTimeScale = ignore;
		}
		var mainList = mComponentList.getMainList();
		int count = mainList.Count;
		for (int i = 0; i < count; ++i)
		{
			mainList[i].setIgnoreTimeScale(ignore);
		}
	}
	public void addDontAutoCreate<T>() where T : GameComponent
	{
		if (mDontAutoCreateType == null)
		{
			mDontAutoCreateType = new HashSet<Type>();
		}
		mDontAutoCreateType.Add(typeof(T));
	}
	public void addDontAutoCreate(Type type)
	{
		if (mDontAutoCreateType == null)
		{
			mDontAutoCreateType = new HashSet<Type>();
		}
		mDontAutoCreateType.Add(type);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mAllComponentTypeList.clear();
		mComponentList.clear();
		mDontAutoCreateType = null;
		mDisableTypeList = null;
		mIgnoreTimeScale = false;
		mDestroying = false;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 此函数由子类调用
	protected virtual void initComponents() { }
	protected void addComponentToList(GameComponent com)
	{
		mComponentList.add(com);
		mAllComponentTypeList.add(Typeof(com), com);
	}
	protected void breakComponent<T>(GameComponent exceptComponent)
	{
		if (mDestroying)
		{
			return;
		}
		// 中断所有可中断的组件
		foreach (var item in mAllComponentTypeList.startForeach())
		{
			GameComponent com = item.Value;
			if (com.isActive() &&
				com is T &&
				com is IComponentBreakable &&
				com != exceptComponent)
			{
				(com as IComponentBreakable).notifyBreak();
				com.setActive(false);
			}
		}
		mAllComponentTypeList.endForeach();
	}
}