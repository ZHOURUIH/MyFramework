using UnityEngine;
using System;

public abstract class ComponentOwner : CommandReceiver
{
	protected SafeDictionary<Type, GameComponent> mAllComponentTypeList;	// 组件类型列表,first是组件的类型名
	protected SafeList<GameComponent> mComponentList;						// 组件列表,保存着组件之间的更新顺序
	protected bool mIgnoreTimeScale;
	public ComponentOwner()
	{
		mAllComponentTypeList = new SafeDictionary<Type, GameComponent>();
		mComponentList = new SafeList<GameComponent>();
	}
	public override void destroy()
	{
		var componentList = mAllComponentTypeList.startForeach();
		foreach (var item in componentList)
		{
			item.Value.destroy();
			UN_CLASS(item.Value);
		}
		mAllComponentTypeList.clear();
		mComponentList.clear();
		base.destroy();
	}
	public virtual void setActive(bool active)
	{
		var componentList = mAllComponentTypeList.startForeach();
		foreach (var item in componentList)
		{
			item.Value.notifyOwnerActive(active);
		}
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
				return;
			}
			GameComponent com = updateList[i];
			if (com != null && com.isActive())
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
	}
	// 后更新
	public virtual void lateUpdate(float elapsedTime)
	{
		var updateList = mComponentList.startForeach();
		int rootComponentCount = updateList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent com = updateList[i];
			if (com != null && com.isActive())
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
	}
	// 物理更新
	public virtual void fixedUpdate(float elapsedTime)
	{
		var updateList = mComponentList.startForeach();
		int rootComponentCount = updateList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent com = updateList[i];
			if (com != null && com.isActive())
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
	}
	public virtual void notifyAddComponent(GameComponent com) { }
	public GameComponent addComponent(Type type, bool active = false)
	{
		// 不能创建重名的组件
		if (mAllComponentTypeList.containsKey(type))
		{
			logError("there is com : " + type);
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
		return addComponent(type, active) as T;
	}
	public void addComponent<T>(out T component, bool active = false) where T : GameComponent
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用addComponent<T>(out T component, bool active = false)添加非主工程中的组件");
			logError("如果需要获取非主工程中的组件,使用GameComponent addComponent(Type type, bool active = false)");
		}
		component = addComponent(type, active) as T;
	}
	public SafeDictionary<Type, GameComponent> getAllComponent() { return mAllComponentTypeList; }
	public T getComponent<T>(bool needActive = false, bool addIfNull = true) where T : GameComponent
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用getComponent<T>(bool needActive, bool addIfNull)获取非主工程中的组件");
			logError("如果需要获取非主工程中的组件,使用T getComponent<T>(Type type, bool needActive, bool addIfNull)");
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
	public override void resetProperty()
	{
		base.resetProperty();
		mAllComponentTypeList.clear();
		mComponentList.clear();
		mIgnoreTimeScale = false;
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
		// 中断所有可中断的组件
		var componentList = mAllComponentTypeList.startForeach();
		foreach (var item in componentList)
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
	}
}