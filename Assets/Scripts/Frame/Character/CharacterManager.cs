using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityUtility;
using static FrameUtility;
using static StringUtility;
using static MathUtility;

// 角色管理器
public class CharacterManager : FrameSystem
{
	protected Dictionary<Type, Dictionary<long, Character>> mCharacterTypeList;	// 角色分类列表
	protected SafeDictionary<long, Character> mCharacterUpdateList;		// 用于更新角色的列表
	protected Dictionary<long, Character> mCharacterGUIDList;			// 角色ID索引表
	protected Dictionary<long, Character> mFixedUpdateList;				// 需要在FixedUpdate中更新的列表,如果直接使用mCharacterGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储
	protected Character mMyself;										// 玩家自己,方便获取
	public CharacterManager()
	{
		mCharacterTypeList = new Dictionary<Type, Dictionary<long, Character>>();
		mCharacterUpdateList = new SafeDictionary<long, Character>();
		mCharacterGUIDList = new Dictionary<long, Character>();
		mFixedUpdateList = new Dictionary<long, Character>();
		mCreateObject = true;
	}
	public override void destroy()
	{
		base.destroy();
		destroyAllCharacter();
		mCharacterTypeList = null;
		mCharacterGUIDList = null;
		mFixedUpdateList = null;
		mMyself = null;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (var item in mCharacterUpdateList.startForeach())
		{
			Character character = item.Value;
			if (character == null || !character.isActive())
			{
				continue;
			}
			character.update(!character.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime);
		}
		mCharacterUpdateList.endForeach();
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		foreach (var item in mCharacterUpdateList.startForeach())
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
		mCharacterUpdateList.endForeach();
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
		mCharacterGUIDList.TryGetValue(characterID, out Character character);
		return character;
	}
	public Dictionary<long, Character> getCharacterList() { return mCharacterGUIDList; }
	public Dictionary<long, Character> getCharacterListByType(Type type)
	{
		mCharacterTypeList.TryGetValue(type, out Dictionary<long, Character> characterList);
		return characterList;
	}
	public Character createCharacter(string name, Type type, long id = 0, bool managed = true)
	{
		if (id == 0)
		{
			id = generateGUID();
		}
		if (mCharacterGUIDList.ContainsKey(id))
		{
			logError("there is a character id : " + id + "! can not create again!");
			return null;
		}
		var character = CLASS(type) as Character;
		character.setName(name);
		character.setCharacterType(type);
		// 如果是玩家自己,则记录下来
		if (character.isMyself())
		{
			if (mMyself != null)
			{
				logError("Myself has exist ! can not create again, name : " + (name != null ? name : EMPTY));
				return null;
			}
			mMyself = character;
		}
		// 将角色挂接到管理器下
		character.createCharacterNode();
		character.setID(id);
		character.init();
		addCharacterToList(character, managed);
		return character;
	}
	public void destroyAllCharacter()
	{
		foreach(var item in mCharacterGUIDList)
		{
			var temp = item.Value;
			temp.destroy();
			UN_CLASS(ref temp);
		}
		mCharacterGUIDList.Clear();
		mCharacterTypeList.Clear();
		mCharacterUpdateList.clear();
		mFixedUpdateList.Clear();
		mMyself = null;
	}
	public void destroyCharacter(long id)
	{
		destroyCharacter(getCharacter(id));
	}
	public void destroyCharacter(Character character)
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
		mCharacterUpdateList.remove(guid);
		mCharacterGUIDList.Remove(guid);
		mFixedUpdateList.Remove(guid);
		character.destroy();
		if (mMyself == character)
		{
			mMyself = null;
		}
		UN_CLASS(ref character);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addCharacterToList(Character character, bool managed)
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
		if (mCharacterGUIDList.ContainsKey(guid))
		{
			logError("there is a character id : " + guid + ", can not add again!");
		}
		mCharacterGUIDList.Add(guid, character);
		if (managed)
		{
			mCharacterUpdateList.add(guid, character);
			if (character.isEnableFixedUpdate())
			{
				mFixedUpdateList.Add(guid, character);
			}
		}
	}
}