using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharacterManager : FrameSystem
{
	protected Dictionary<Type, Dictionary<ulong, Character>> mCharacterTypeList;    // 角色分类列表
	protected SafeDictionary<ulong, Character> mCharacterGUIDList;	// 角色ID索引表
	protected Dictionary<ulong, Character> mFixedUpdateList;		// 需要在FixedUpdate中更新的列表,如果直接使用mCharacterGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储
	protected ICharacterMyself mMyself;							// 玩家自己,方便获取
	public CharacterManager()
	{
		mCharacterTypeList = new Dictionary<Type, Dictionary<ulong, Character>>();
		mCharacterGUIDList = new SafeDictionary<ulong, Character>();
		mFixedUpdateList = new Dictionary<ulong, Character>();
		mCreateObject = true;
	}
	public override void destroy()
	{
		base.destroy();
		var updateList = mCharacterGUIDList.GetMainList();
		foreach (var character in updateList)
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
		var updateList = mCharacterGUIDList.GetUpdateList();
		foreach (var item in updateList)
		{
			Character character = item.Value;
			if (character == null || !character.isActive())
			{
				continue;
			}
			character.update(!character.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime);
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
	public ICharacterMyself getIMyself() { return mMyself; }
	public Character getCharacter(ulong characterID)
	{
		mCharacterGUIDList.TryGetValue(characterID, out Character character);
		return character;
	}
	public Dictionary<ulong, Character> getCharacterList() { return mCharacterGUIDList.GetMainList(); }
	public void activeCharacter(ulong id, bool active)
	{
		activeCharacter(getCharacter(id), active);
	}
	public void activeCharacter(Character character, bool active)
	{
		character.setActive(active);
	}
	public Dictionary<ulong, Character> getCharacterListByType(Type type)
	{
		mCharacterTypeList.TryGetValue(type, out Dictionary<ulong, Character> characterList);
		return characterList;
	}
	public Character createCharacter(string name, Type type, ulong id, bool createNode)
	{
		if (mCharacterGUIDList.ContainsKey(id))
		{
			logError("there is a character id : " + id + "! can not create again!");
			return null;
		}
		Character newCharacter = createInstance<Character>(type);
		newCharacter.setName(name);
		newCharacter.setCharacterType(type);
		// 如果是玩家自己,则记录下来
		if (newCharacter is ICharacterMyself)
		{
			if (mMyself != null)
			{
				logError("Myself has exist ! can not create again, name : " + (name != null ? name : EMPTY));
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
	public void destroyCharacter(ulong id)
	{
		Character character = getCharacter(id);
		if(character != null)
		{
			destroyCharacter(character);
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
		ulong guid = character.getGUID();
		// 加入到角色分类列表
		if (!mCharacterTypeList.TryGetValue(type, out Dictionary<ulong, Character> characterList))
		{
			characterList = new Dictionary<ulong, Character>();
			mCharacterTypeList.Add(type, characterList);
		}
		characterList.Add(guid, character);
		// 加入ID索引表
		if (mCharacterGUIDList.ContainsKey(guid))
		{
			logError("there is a character id : " + guid + ", can not add again!");
		}
		mCharacterGUIDList.Add(guid, character);
		if (character.isEnableFixedUpdate())
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
		ulong guid = character.getGUID();
		// 从角色分类列表中移除
		if (mCharacterTypeList.TryGetValue(type, out Dictionary<ulong, Character> characterList))
		{
			characterList.Remove(guid);
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
