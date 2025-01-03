using UnityEngine;

// 按键映射信息
public struct KeyMapping
{
	public int mCustomKey;			// 自定义按键值,也就是KeyCode映射到的值,一般是一个枚举
	public KeyCode mKey;			// 键盘按键
	public string mCustomKeyName;	// 自定义按键名
	public KeyMapping(int customKey, KeyCode key, string customKeyName)
	{
		mCustomKey = customKey;
		mKey = key;
		mCustomKeyName = customKeyName;
	}
}