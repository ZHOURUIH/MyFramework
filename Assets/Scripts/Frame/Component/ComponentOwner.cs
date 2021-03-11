using UnityEngine;
using System;

public abstract class ComponentOwner : CommandReceiver
{
	protected SafeDictionary<Type, GameComponent> mAllComponentTypeList;	// 组件类型列表,first是组件的类型名
	protected SafeList<GameComponent> mComponentList;                       // 组件列表,保存着组件之间的更新顺序
	protected bool mIgnoreTimeScale;
	public ComponentOwner()
	{
		mAllComponentTypeList = new SafeDictionary<Type, GameComponent>();
		mComponentList = new SafeList<GameComponent>();
	}
	public override void destroy()
	{
		var componentList = mAllComponentTypeList.getUpdateList();
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
		var componentList = mAllComponentTypeList.getUpdateList();
		foreach (var item in componentList)
		{
			item.Value.notifyOwnerActive(active);
		}
	}
	// 更新正常更新的组件
	public virtual void update(float elapsedTime)
	{
		var updateList = mComponentList.getUpdateList();
		int rootComponentCount = updateList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			// 数量为0,表示在更新组件的过程中当前对象被销毁
			if (updateList.Count == 0)
			{
				return;
			}
			GameComponent component = updateList[i];
			if (component != null && component.isActive())
			{
				if (!component.isIgnoreTimeScale())
				{
					component.update(elapsedTime);
				}
				else
				{
					component.update(Time.unscaledDeltaTime);
				}
			}
		}
	}
	// 后更新
	public virtual void lateUpdate(float elapsedTime)
	{
		var updateList = mComponentList.getUpdateList();
		int rootComponentCount = updateList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent component = updateList[i];
			if (component != null && component.isActive())
			{
				if (!component.isIgnoreTimeScale())
				{
					component.lateUpdate(elapsedTime);
				}
				else
				{
					component.lateUpdate(Time.unscaledDeltaTime);
				}
			}
		}
	}
	// 物理更新
	public virtual void fixedUpdate(float elapsedTime)
	{
		var updateList = mComponentList.getUpdateList();
		int rootComponentCount = updateList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent component = updateList[i];
			if (component != null && component.isActive())
			{
				if (!component.isIgnoreTimeScale())
				{
					component.fixedUpdate(elapsedTime);
				}
				else
				{
					component.fixedUpdate(Time.unscaledDeltaTime);
				}
			}
		}
	}
	public virtual void notifyAddComponent(GameComponent component) { }
	public GameComponent addComponent(Type type, bool active = false)
	{
		// 不能创建重名的组件
		if (mAllComponentTypeList.containsKey(type))
		{
			logError("there is component : " + type);
			return null;
		}
		var component = CLASS<GameComponent>(type);
		if (component == null)
		{
			return null;
		}
		component.setType(type);
		component.init(this);
		// 将组件加入列表
		addComponentToList(component);
		// 通知创建了组件
		notifyAddComponent(component);
		component.setActive(active);
		component.setDefaultActive(active);
		return component;
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
	public T getComponent<T>(out T component, bool needActive = false, bool addIfNull = true) where T : GameComponent
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用getComponent<T>(out T component, bool needActive, bool addIfNull)获取非主工程中的组件");
			logError("如果需要获取非主工程中的组件,使用T getComponent<T>(Type type, bool needActive, bool addIfNull)");
		}
		return component = getComponent(type, needActive, addIfNull) as T;
	}
	public GameComponent getComponent(Type type, bool needActive = false, bool addIfNull = true)
	{
		if (mAllComponentTypeList.tryGetValue(type, out GameComponent component))
		{
			if (!needActive || component.isActive())
			{
				return component;
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
		GameComponent component = getComponent(type, false, addIfNull);
		component?.setActive(active);
	}
	public void notifyComponentStart(GameComponent component)
	{
		if (component is IComponentModifyAlpha)
		{
			breakComponent<IComponentModifyAlpha>(component);
		}
		if (component is IComponentModifyColor)
		{
			breakComponent<IComponentModifyColor>(component);
		}
		if (component is IComponentModifyPosition)
		{
			breakComponent<IComponentModifyPosition>(component);
		}
		if (component is IComponentModifyRotation)
		{
			breakComponent<IComponentModifyRotation>(component);
		}
		if (component is IComponentModifyScale)
		{
			breakComponent<IComponentModifyScale>(component);
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
	//---------------------------------------------------------------------------------------------------
	// 此函数由子类调用
	protected virtual void initComponents() { }
	protected void addComponentToList(GameComponent component)
	{
		mComponentList.add(component);
		mAllComponentTypeList.add(Typeof(component), component);
	}
	protected void breakComponent<T>(GameComponent exceptComponent)
	{
		// 中断所有可中断的组件
		var componentList = mAllComponentTypeList.getUpdateList();
		foreach (var item in componentList)
		{
			GameComponent component = item.Value;
			if (component.isActive() &&
				component is T &&
				component is IComponentBreakable &&
				component != exceptComponent)
			{
				(component as IComponentBreakable).notifyBreak();
				component.setActive(false);
			}
		}
	}
}