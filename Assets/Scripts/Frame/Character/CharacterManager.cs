using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterManager : FrameSystem
{
	protected Dictionary<Type, Dictionary<long, Character>> mCharacterTypeList;    // 角色分类列表
	protected SafeDictionary<long, Character> mCharacterGUIDList;	// 角色ID索引表
	protected Dictionary<long, Character> mFixedUpdateList;			// 需要在FixedUpdate中更新的列表,如果直接使用mCharacterGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储
	protected Character mMyself;									// 玩家自己,方便获取
	public CharacterManager()
	{
		mCharacterTypeList = new Dictionary<Type, Dictionary<long, Character>>();
		mCharacterGUIDList = new SafeDictionary<long, Character>();
		mFixedUpdateList = new Dictionary<long, Character>();
		mCreateObject = true;
	}
	public override void destroy()
	{
		base.destroy();
		var updateList = mCharacterGUIDList.getMainList();
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
		var updateList = mCharacterGUIDList.startForeach();
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
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		foreach (var item in mCharacterGUIDList.startForeach())
		{
			Character character = item.Value;
			if (character != null && character.isActive())
			{
				if (!character.isIgnoreTimeScale())
				{
					character.lateUpdate(elapsedTime);
				}
				else
				{
					character.lateUpdate(Time.unscaledDeltaTime);
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
	public Character getMyself() { return mMyself; }
	public Character getCharacter(long characterID)
	{
		mCharacterGUIDList.tryGetValue(characterID, out Character character);
		return character;
	}
	public Dictionary<long, Character> getCharacterList() { return mCharacterGUIDList.getMainList(); }
	public void activeCharacter(long id, bool active)
	{
		activeCharacter(getCharacter(id), active);
	}
	public void activeCharacter(Character character, bool active)
	{
		character.setActive(active);
	}
	public Dictionary<long, Character> getCharacterListByType(Type type)
	{
		mCharacterTypeList.TryGetValue(type, out Dictionary<long, Character> characterList);
		return characterList;
	}
	public Character createCharacter(string name, Type type, long id, bool createNode)
	{
		if (mCharacterGUIDList.containsKey(id))
		{
			logError("there is a character id : " + id + "! can not create again!");
			return null;
		}
		var newCharacter = CLASS(type) as Character;
		newCharacter.setName(name);
		newCharacter.setCharacterType(type);
		// 如果是玩家自己,则记录下来
		if (newCharacter.isMyself())
		{
			if (mMyself != null)
			{
				logError("Myself has exist ! can not create again, name : " + (name != null ? name : EMPTY));
				return null;
			}
			mMyself = newCharacter;
		}
		// 将角色挂接到管理器下
		if(createNode)
		{
			GameObject charNode = createGameObject(newCharacter.getName(), mObject);
			newCharacter.setObject(charNode, true);
		}
		newCharacter.setID(id);
		newCharacter.init();
		addCharacterToList(newCharacter);
		return newCharacter;
	}
	public void destroyAllCharacter()
	{
		var updateList = mCharacterGUIDList.startForeach();
		foreach(var item in updateList)
		{
			destroyCharacter(item.Value);
		}
		mCharacterGUIDList.clear();
	}
	public void destroyCharacter(long id)
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
		long guid = character.getGUID();
		// 加入到角色分类列表
		if (!mCharacterTypeList.TryGetValue(type, out Dictionary<long, Character> characterList))
		{
			characterList = new Dictionary<long, Character>();
			mCharacterTypeList.Add(type, characterList);
		}
		characterList.Add(guid, character);
		// 加入ID索引表
		if (mCharacterGUIDList.containsKey(guid))
		{
			logError("there is a character id : " + guid + ", can not add again!");
		}
		mCharacterGUIDList.add(guid, character);
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
		long guid = character.getGUID();
		// 从角色分类列表中移除
		if (mCharacterTypeList.TryGetValue(character.getType(), out Dictionary<long, Character> characterList))
		{
			characterList.Remove(guid);
		}
		// 从ID索引表中移除
		mCharacterGUIDList.remove(guid);
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
		UN_CLASS(character);
	}
}
