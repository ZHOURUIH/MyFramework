using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityUtility;
using static FrameUtility;
using static StringUtility;
using static MathUtility;
using static CSharpUtility;

// 角色管理器
public class CharacterManager : FrameSystem
{
	protected Dictionary<Type, Dictionary<long, Character>> mCharacterTypeList = new(); // 角色分类列表
	protected SafeDictionary<long, Character> mCharacterUpdateList = new();				// 用于更新角色的列表
	protected Dictionary<long, Character> mCharacterGUIDList = new();					// 角色ID索引表
	protected Dictionary<long, Character> mFixedUpdateList = new();						// 需要在FixedUpdate中更新的列表,如果直接使用mCharacterGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储
	protected Character mMyself;														// 玩家自己,方便获取
	public CharacterManager()
	{
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
		using var a = new SafeDictionaryReader<long, Character>(mCharacterUpdateList);
		foreach (Character character in a.mReadList.Values)
		{
			if (character == null || !character.isActiveInHierarchy())
			{
				continue;
			}
			character.update(!character.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime);
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		using var a = new SafeDictionaryReader<long, Character>(mCharacterUpdateList);
		foreach (Character character in a.mReadList.Values)
		{
			if (character != null && character.isActiveInHierarchy())
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
		foreach (Character character in mFixedUpdateList.Values)
		{
			if (character != null && character.isActiveInHierarchy())
			{
				character.fixedUpdate(elapsedTime);
			}
		}
	}
	public Character getMyself() { return mMyself; }
	public Character getCharacter(long characterID) { return mCharacterGUIDList.get(characterID); }
	public Dictionary<long, Character> getCharacterList() { return mCharacterGUIDList; }
	public Dictionary<long, Character> getCharacterListByType<T>() where T : Character { return mCharacterTypeList.get(typeof(T)); }
	public Dictionary<long, Character> getCharacterListByType(Type type) { return mCharacterTypeList.get(type); }
	public T createCharacter<T>(string name, long id = 0, bool managed = true) where T : Character
	{
		return createCharacter(name, typeof(T), id, managed) as T;
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
		var character = CLASS<Character>(type);
		character.setName(name);
		character.setCharacterType(type);
		// 如果是玩家自己,则记录下来
		if (character.isMyself())
		{
			if (mMyself != null)
			{
				logError("Myself has exist ! can not create again, name : " + (name ?? EMPTY));
				return null;
			}
			mMyself = character;
		}
		// 将角色挂接到管理器下
		character.setID(id);
		character.init();
		addCharacterToList(character, managed);
		return character;
	}
	public void destroyAllCharacter()
	{
		UN_CLASS_LIST(mCharacterGUIDList);
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
		mCharacterTypeList.get(character.getType())?.Remove(guid);
		// 从ID索引表中移除
		mCharacterUpdateList.remove(guid);
		mCharacterGUIDList.Remove(guid);
		mFixedUpdateList.Remove(guid);
		if (mMyself == character)
		{
			mMyself = null;
		}
		UN_CLASS(ref character);
	}
	public void destroyCharacterList<T>(IList<T> characterList) where T : Character
	{
		foreach (T character in characterList.safe())
		{
			long guid = character.getGUID();
			// 从角色分类列表中移除
			mCharacterTypeList.get(character.getType())?.Remove(guid);
			// 从ID索引表中移除
			mCharacterUpdateList.remove(guid);
			mCharacterGUIDList.Remove(guid);
			mFixedUpdateList.Remove(guid);
			if (mMyself == character)
			{
				mMyself = null;
			}
		}
		UN_CLASS_LIST(characterList);
	}
	public void destroyCharacterList<T0, T1>(IDictionary<T0, T1> characterList) where T1 : Character
	{
		foreach (T1 character in (characterList?.Values).safe())
		{
			long guid = character.getGUID();
			// 从角色分类列表中移除
			mCharacterTypeList.get(character.getType())?.Remove(guid);
			// 从ID索引表中移除
			mCharacterUpdateList.remove(guid);
			mCharacterGUIDList.Remove(guid);
			mFixedUpdateList.Remove(guid);
			if (mMyself == character)
			{
				mMyself = null;
			}
		}
		UN_CLASS_LIST(characterList);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addCharacterToList(Character character, bool managed)
	{
		if (character == null)
		{
			return;
		}
		long guid = character.getGUID();
		// 加入到角色分类列表
		mCharacterTypeList.getOrAddNew(character.getType()).Add(guid, character);
		// 加入ID索引表
		if (!mCharacterGUIDList.TryAdd(guid, character))
		{
			logError("there is a character id : " + guid + ", can not add again!");
		}
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