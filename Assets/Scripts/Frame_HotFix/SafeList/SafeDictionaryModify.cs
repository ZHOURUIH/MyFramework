
// 安全字典的修改信息,修改的数据,以及修改操作类型
public struct SafeDictionaryModify<Key, Value>
{
	public Value mValue;		// 数据的Value
	public Key mKey;			// 数据的Key
	public bool mAdd;			// 是否为添加操作
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