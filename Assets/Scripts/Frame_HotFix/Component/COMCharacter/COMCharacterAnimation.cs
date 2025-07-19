using System;
using System.Collections.Generic;
using static FrameUtility;

// 处理动作相关逻辑
public class COMCharacterAnimation : GameComponent
{
	protected List<AnimationLayer> mLayerList = new();	// 动画层列表
	protected Character mCharacter;						// 所属角色
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mCharacter = mComponentOwner as Character;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLayerList.Clear();
		mCharacter = null;
	}
	public override void destroy()
	{
		base.destroy();
		clear();
	}
	public void clear()
	{
		UN_CLASS_LIST(mLayerList);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (AnimationLayer layer in mLayerList)
		{
			layer.update();
		}
	}
	public void addLayer<T0, T1>() where T0 : CharacterState where T1 : StateGroup
	{
		addLayer(typeof(T0), typeof(T1));
	}
	public void addLayer(Type defaultType, Type group)
	{
		AnimationLayer layer = mLayerList.addClass();
		layer.setCharacter(mCharacter);
		layer.setDefaultState(defaultType);
		layer.setGroup(group);
		layer.setLayerIndex(mLayerList.Count);
	}
}