using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class ComponentOwner : CommandReceiver, IClassObject
{
	protected Dictionary<Type, GameComponent> mAllComponentTypeList;// 组件类型列表,first是组件的类型名
	protected List<GameComponent> mComponentList;					// 组件列表,保存着组件之间的更新顺序
	protected bool mIgnoreTimeScale;
	public ComponentOwner(string name)
		:base(name)
	{
		mComponentList = new List<GameComponent>();
		mAllComponentTypeList = new Dictionary<Type, GameComponent>();
	}
	public override void destroy()
	{
		foreach(var item in mAllComponentTypeList)
		{
			item.Value.notifyOwnerDestroy();
		}
		base.destroy();
	}
	public virtual void setActive(bool active)
	{
		foreach (var item in mAllComponentTypeList)
		{
			item.Value.notifyOwnerActive(active);
		}
	}
	// 更新正常更新的组件
	public virtual void update(float elapsedTime)
	{
		int rootComponentCount = mComponentList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent component = mComponentList[i];
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
		int rootComponentCount = mComponentList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent component = mComponentList[i];
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
		int rootComponentCount = mComponentList.Count;
		for (int i = 0; i < rootComponentCount; ++i)
		{
			GameComponent component = mComponentList[i];
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
	public virtual void notifyComponentDetached(GameComponent component) { removeComponentFromList(component); }
	public virtual void notifyComponentAttached(GameComponent component)
	{
		if (component == null)
		{
			return;
		}
		if (!mAllComponentTypeList.ContainsKey(component.GetType()))
		{
			addComponentToList(component);
		}
	}
	public static GameComponent createIndependentComponent(Type type, bool initComponent = true)
	{
		GameComponent component = createInstance<GameComponent>(type);
		// 创建组件并且设置拥有者,然后初始化
		if (initComponent)
		{
			component?.init(null);
		}
		return component;
	}
	public static T createIndependentComponent<T>(bool initComponent = true) where T : GameComponent
	{
		return createIndependentComponent(typeof(T), initComponent) as T;
	}
	public T addComponent<T>(bool active = false) where T : GameComponent
	{
		// 不能创建重名的组件
		if (mAllComponentTypeList.ContainsKey(typeof(T)))
		{
			logError("there is component : " + typeof(T).ToString());
			return null;
		}
		GameComponent component = createIndependentComponent<T>(false);
		if (component == null)
		{
			return null;
		}
		component.init(this);
		// 将组件加入列表
		addComponentToList(component);
		// 通知创建了组件
		notifyAddComponent(component);
		component.setActive(active);
		component.setDefaultActive(active);
		return component as T;
	}
	public void destroyAllComponents()
	{
		List<Type> keys = mListPool.newList(out keys);
		keys.AddRange(mAllComponentTypeList.Keys);
		for (int i = 0; i < keys.Count; ++i)
		{
			mAllComponentTypeList[keys[i]].destroy();
		}
		mListPool.destroyList(keys);
	}
	public Dictionary<Type, GameComponent> getAllComponent() { return mAllComponentTypeList; }
	public T getComponent<T>(out T component, bool needActive = false, bool addIfNull = true) where T : GameComponent
	{
		component = getComponent<T>(needActive, addIfNull);
		return component;
	}
	public T getComponent<T>(bool needActive = false, bool addIfNull = true) where T : GameComponent
	{
		Type type = typeof(T);
		if (mAllComponentTypeList.ContainsKey(type))
		{
			GameComponent component = mAllComponentTypeList[type];
			if(!needActive || component.isActive())
			{
				return component as T;
			}
		}
		if(addIfNull)
		{
			return addComponent<T>();
		}
		return null;
	}
	public void activeComponent<T>(bool active = true, bool addIfNull = false) where T : GameComponent
	{
		T component = getComponent<T>(false, addIfNull);
		component?.setActive(active);
	}
	public virtual void notifyComponentDestroied(GameComponent component)
	{
		removeComponentFromList(component);
	}
	public void breakComponent<T>(Type exceptComponent)
	{
		foreach (var item in mAllComponentTypeList)
		{
			GameComponent component = item.Value;
			if (component.isActive() && 
				component is T && 
				component is IComponentBreakable &&
				component.GetType() != exceptComponent)
			{
				(component as IComponentBreakable).notifyBreak();
				component.setActive(false);
			}
		}
	}
	public Dictionary<Type, GameComponent> getComponentTypeList() { return mAllComponentTypeList; }
	public List<GameComponent> getComponentList() { return mComponentList; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	// componentOnly表示是否只设置组件不受时间缩放影响,如果只有组件受影响,则Owner本身还是受到时间缩放影响的
	public virtual void setIgnoreTimeScale(bool ignore, bool componentOnly = false)
	{
		if(!componentOnly)
		{
			mIgnoreTimeScale = ignore;
		}
		foreach (var item in mComponentList)
		{
			item.setIgnoreTimeScale(ignore);
		}
	}
	public virtual void resetProperty()
	{
		setIgnoreTimeScale(false);
		// 停止所有组件
		foreach (var item in mAllComponentTypeList)
		{
			item.Value.resetProperty();
			item.Value.setActive(item.Value.isDefaultActive());
		}
	}
	//---------------------------------------------------------------------------------------------------
	// 此函数由子类调用
	protected virtual void initComponents() { }
	protected void addComponentToList(GameComponent component, int componentPos = -1)
	{
		Type type = component.GetType();
		if (componentPos == -1)
		{
			mComponentList.Add(component);
		}
		else
		{
			mComponentList.Insert(componentPos, component);
		}
		mAllComponentTypeList.Add(type, component);
	}
	protected void removeComponentFromList(GameComponent component)
	{
		mComponentList.Remove(component);
		mAllComponentTypeList.Remove(component.GetType());
	}
}