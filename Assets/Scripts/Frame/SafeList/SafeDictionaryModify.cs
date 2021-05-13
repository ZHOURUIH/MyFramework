using System.Collections.Generic;

public struct SafeDictionaryModify<Key, Value>
{
	public Key mKey;
	public Value mValue;
	public bool mAdd;
	public SafeDictionaryModify(Key key)
	{
		mKey = key;
		mValue = default;
		mAdd = false;
	}
	public SafeDictionaryModify(Key key, Value value)
	{
		mKey = key;
		mValue = value;
		mAdd = true;
	}
}