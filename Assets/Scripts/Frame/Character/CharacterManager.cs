using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharacterManager : FrameComponent
{
	protected Dictionary<Type, Dictionary<uint, Character>> mCharacterTypeList;    // 角色分类列表
	protected Dictionary<uint, Character> mCharacterGUIDList;	// 角色ID索引表
	protected Dictionary<uint, Character> mFixedUpdateList;		// 需要在FixedUpdate中更新的列表,如果直接使用mCharacterGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储
	protected ICharacterMyself mMyself;							// 玩家自己,方便获取
	public CharacterManager(string name)
		:base(name)
	{
		mCharacterTypeList = new Dictionary<Type, Dictionary<uint, Character>>();
		mCharacterGUIDList = new Dictionary<uint, Character>();
		mFixedUpdateList = new Dictionary<uint, Character>();
		mCreateObject = true;
	}
	public override void destroy()
	{
		base.destroy();
		foreach (var character in mCharacterGUIDList)
		{
			character.Value.destroy();
		}
		mCharacterTypeList = null;
		mCharacterGUIDList = null;
		mFixedUpdateList = null;
		mMyself = null;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (var item in mCharacterGUIDList)
		{
			Character character = item.Value;
			if (character != null && character.isActive())
			{
				if (!character.isIgnoreTimeScale())
				{
					character.update(elapsedTime);
				}
				else
				{
					character.update(Time.unscaledDeltaTime);
				}
			}
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		foreach (var item in mFixedUpdateList)
		{
			Character character = item.Value;
			if (character != null && character.isActive())
			{
				character.fixedUpdate(elapsedTime);
			}
		}
	}
	public T getMyself<T>() where T : Character { return mMyself as T; }
	public T getCharacter<T>(uint characterID) where T : Character
	{
		return getCharacter(characterID) as T;
	}
	public Character getCharacter(uint characterID)
	{
		return mCharacterGUIDList.ContainsKey(characterID) ? mCharacterGUIDList[characterID] : null;
	}
	public Dictionary<uint, Character> getCharacterList() { return mCharacterGUIDList; }
	public void activeCharacter(uint id, bool active)
	{
		activeCharacter(getCharacter(id), active);
	}
	public void activeCharacter(Character character, bool active)
	{
		character.setActive(active);
	}
	public Dictionary<uint, Character> getCharacterListByType<T>() where T : Character
	{
		Type type = typeof(T);
		if (!mCharacterTypeList.ContainsKey(type))
		{
			return null;
		}
		return mCharacterTypeList[type];
	}
	public Character createCharacter(string name, Type type, uint id, bool createNode)
	{
		if (mCharacterGUIDList.ContainsKey(id))
		{
			logError("there is a character id : " + id + "! can not create again!");
			return null;
		}
		Character newCharacter = createInstance<Character>(type, name);
		newCharacter.setCharacterType(type);
		// 如果是玩家自己,则记录下来
		if (newCharacter is ICharacterMyself)
		{
			if (mMyself != null)
			{
				logError("Myself has exist ! can not create again, name : " + (name != null ? name : ""));
				return null;
			}
			mMyself = newCharacter as ICharacterMyself;
		}
		// 将角色挂接到管理器下
		if(createNode)
		{
			GameObject charNode = createGameObject(newCharacter.getName(), mObject);
			newCharacter.setObject(charNode);
		}
		newCharacter.setID(id);
		newCharacter.init();
		addCharacterToList(newCharacter);
		return newCharacter;
	}
	public void destroyCharacter(uint id)
	{
		Character character = getCharacter(id);
		if(character != null)
		{
			destroyCharacter(character);
		}
	}
	public void notifyCharacterIDChanged(uint oldID)
	{
		if (mCharacterGUIDList.ContainsKey(oldID))
		{
			Character character = mCharacterGUIDList[oldID];
			mCharacterGUIDList.Remove(oldID);
			mCharacterGUIDList.Add(character.getGUID(), character);
		}
	}
	//------------------------------------------------------------------------------------------------------------
	protected void addCharacterToList(Character character)
	{
		if (character == null)
		{
			return;
		}
		Type type = character.getType();
		uint guid = character.getGUID();
		// 加入到角色分类列表
		if (!mCharacterTypeList.ContainsKey(type))
		{
			mCharacterTypeList.Add(type, new Dictionary<uint, Character>());
		}
		mCharacterTypeList[type].Add(guid, character);
		// 加入ID索引表
		if (!mCharacterGUIDList.ContainsKey(guid))
		{
			mCharacterGUIDList.Add(guid, character);
		}
		else
		{
			logError("there is a character id : " + guid + ", can not add again!");
		}
		if(character.isEnableFixedUpdate())
		{
			mFixedUpdateList.Add(guid, character);
		}
	}
	protected void removeCharacterFromList(Character character)
	{
		if (character == null)
		{
			return;
		}
		Type type = character.getType();
		uint guid = character.getGUID();
		// 从角色分类列表中移除
		if (mCharacterTypeList.ContainsKey(type))
		{
			mCharacterTypeList[type].Remove(guid);
		}
		// 从ID索引表中移除
		mCharacterGUIDList.Remove(guid);
		mFixedUpdateList.Remove(guid);
	}
	protected void destroyCharacter(Character character)
	{
		removeCharacterFromList(character);
		character.destroy();
		if (mMyself == character)
		{
			mMyself = null;
		}
	}
}
