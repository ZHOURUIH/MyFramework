using System;
using System.Collections.Generic;
using UnityEngine;

// 处理动作相关逻辑
public class COMCharacterAnimation : GameComponent
{
	protected List<AnimationLayer> mLayerList;
	protected Character mCharacter;				// 所属角色
	public COMCharacterAnimation()
	{
		mLayerList = new List<AnimationLayer>();
	}
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
		int count = mLayerList.Count;
		for(int i = 0; i < count; ++i)
		{
			UN_CLASS(mLayerList[i]);
		}
		mLayerList.Clear();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		int count = mLayerList.Count;
		for(int i = 0; i < count; ++i)
		{
			mLayerList[i].update();
		}
	}
	public void addLayer(Type defaultType, Type group)
	{
		CLASS(out AnimationLayer layer);
		layer.setCharacter(mCharacter);
		layer.setDefaultState(defaultType);
		layer.setGroup(group);
		layer.setLayerIndex(mLayerList.Count);
		mLayerList.Add(layer);
	}
}