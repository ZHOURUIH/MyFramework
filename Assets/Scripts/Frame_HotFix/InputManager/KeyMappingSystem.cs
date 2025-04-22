using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;

// 按键映射系统,用于实现游戏中的快捷键以及快捷键设置功能
public class KeyMappingSystem : FrameSystem
{
	protected Dictionary<KeyCode, string> mKeyNameList = new();         // 快捷键名字列表
	protected Dictionary<int, KeyMapping> mKeyMapping = new();          // 按键映射表,key是自定义的一个整数,用于当作具体功能的枚举,value是对应的按键
	public override void init()
	{
		base.init();
		initKey();
	}
	public Dictionary<int, KeyMapping> getKeyMappingList()		{ return mKeyMapping; }
	public string getKeyMappingName(int keyMapping)				{ return getKeyListName(getKeyMapping(keyMapping)); }
	public string getKeyName(KeyCode key)						{ return mKeyNameList.get(key, ""); }
	public string getKeyListName(KeyCode key)					{ return key != KeyCode.None ? getKeyName(key) : ""; }
	public bool isMappingKeyCurrentDown(int mapping)			{ return mInputSystem.isKeyCurrentDown(getKeyMapping(mapping), FOCUS_MASK.SCENE); }
	public bool isMappingKeyDown(int mapping)					{ return mInputSystem.isKeyDown(getKeyMapping(mapping), FOCUS_MASK.SCENE); }
	public string getKeyMappingActionName(int mapping)			{ return mKeyMapping.TryGetValue(mapping, out KeyMapping info) ? info.mMappingName : ""; }
	public KeyCode getKeyMapping(int mapping)					{ return mKeyMapping.TryGetValue(mapping, out KeyMapping info) ? info.mKey : KeyCode.None; }
	public KeyCode getDefaultMappingKey(int mapping)			{ return mKeyMapping.TryGetValue(mapping, out KeyMapping key) ? key.mDefaultKey : KeyCode.None; }
	public void setDefaultKeyMapping(int mapping, KeyCode key)	{ mKeyMapping.get(mapping).mDefaultKey = key; }
	// 可以重复调用,将会保留最后一次的设置
	public bool setKeyMapping(int mappingID, KeyCode key, string actionName = null)
	{
		if (key != KeyCode.None)
		{
			// 需要检测是否与当前按键映射冲突
			foreach (KeyMapping item in mKeyMapping.Values)
			{
				if (item.mKey == key && item.mMappingID != mappingID)
				{
					return false;
				}
			}
		}

		// 更新或者添加
		mKeyMapping.getOrAddClass(mappingID, out KeyMapping curMapping);
		if (!actionName.isEmpty())
		{
			curMapping.mMappingName = actionName;
		}
		curMapping.mMappingID = mappingID;
		curMapping.mKey = key;
		return true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void initKey()
	{
		initSingleKey(KeyCode.A, "A");
		initSingleKey(KeyCode.B, "B");
		initSingleKey(KeyCode.C, "C");
		initSingleKey(KeyCode.D, "D");
		initSingleKey(KeyCode.E, "E");
		initSingleKey(KeyCode.F, "F");
		initSingleKey(KeyCode.G, "G");
		initSingleKey(KeyCode.H, "H");
		initSingleKey(KeyCode.I, "I");
		initSingleKey(KeyCode.J, "J");
		initSingleKey(KeyCode.K, "K");
		initSingleKey(KeyCode.L, "L");
		initSingleKey(KeyCode.M, "M");
		initSingleKey(KeyCode.N, "N");
		initSingleKey(KeyCode.O, "O");
		initSingleKey(KeyCode.P, "P");
		initSingleKey(KeyCode.Q, "Q");
		initSingleKey(KeyCode.R, "R");
		initSingleKey(KeyCode.S, "S");
		initSingleKey(KeyCode.T, "T");
		initSingleKey(KeyCode.U, "U");
		initSingleKey(KeyCode.V, "V");
		initSingleKey(KeyCode.W, "W");
		initSingleKey(KeyCode.X, "X");
		initSingleKey(KeyCode.Y, "Y");
		initSingleKey(KeyCode.Z, "Z");
		initSingleKey(KeyCode.Keypad0, "Num0");
		initSingleKey(KeyCode.Keypad1, "Num1");
		initSingleKey(KeyCode.Keypad2, "Num2");
		initSingleKey(KeyCode.Keypad3, "Num3");
		initSingleKey(KeyCode.Keypad4, "Num4");
		initSingleKey(KeyCode.Keypad5, "Num5");
		initSingleKey(KeyCode.Keypad6, "Num6");
		initSingleKey(KeyCode.Keypad7, "Num7");
		initSingleKey(KeyCode.Keypad8, "Num8");
		initSingleKey(KeyCode.Keypad9, "Num9");
		initSingleKey(KeyCode.KeypadPeriod, "Num.");
		initSingleKey(KeyCode.KeypadDivide, "Num/");
		initSingleKey(KeyCode.KeypadMultiply, "Num*");
		initSingleKey(KeyCode.KeypadMinus, "Num-");
		initSingleKey(KeyCode.KeypadPlus, "Num+");
		initSingleKey(KeyCode.Alpha0, "0");
		initSingleKey(KeyCode.Alpha1, "1");
		initSingleKey(KeyCode.Alpha2, "2");
		initSingleKey(KeyCode.Alpha3, "3");
		initSingleKey(KeyCode.Alpha4, "4");
		initSingleKey(KeyCode.Alpha5, "5");
		initSingleKey(KeyCode.Alpha6, "6");
		initSingleKey(KeyCode.Alpha7, "7");
		initSingleKey(KeyCode.Alpha8, "8");
		initSingleKey(KeyCode.Alpha9, "9");
		initSingleKey(KeyCode.F1, "F1");
		initSingleKey(KeyCode.F2, "F2");
		initSingleKey(KeyCode.F3, "F3");
		initSingleKey(KeyCode.F4, "F4");
		initSingleKey(KeyCode.F5, "F5");
		initSingleKey(KeyCode.F6, "F6");
		initSingleKey(KeyCode.F7, "F7");
		initSingleKey(KeyCode.F8, "F8");
		initSingleKey(KeyCode.F9, "F9");
		initSingleKey(KeyCode.F10, "F10");
		initSingleKey(KeyCode.F11, "F11");
		initSingleKey(KeyCode.F12, "F12");
		initSingleKey(KeyCode.Equals, "=");
		initSingleKey(KeyCode.Minus, "-");
		initSingleKey(KeyCode.LeftBracket, "[");
		initSingleKey(KeyCode.RightBracket, "]");
		initSingleKey(KeyCode.Backslash, "\\");
		initSingleKey(KeyCode.Semicolon, ";");
		initSingleKey(KeyCode.Quote, "'");
		initSingleKey(KeyCode.Comma, ",");
		initSingleKey(KeyCode.Period, ".");
		initSingleKey(KeyCode.Slash, "/");
		initSingleKey(KeyCode.BackQuote, "`");
		initSingleKey(KeyCode.Backspace, "Back");
		initSingleKey(KeyCode.Insert, "Insert");
		initSingleKey(KeyCode.Delete, "Del");
		initSingleKey(KeyCode.Home, "Home");
		initSingleKey(KeyCode.End, "End");
		initSingleKey(KeyCode.PageUp, "PgUp");
		initSingleKey(KeyCode.PageDown, "PgDn");
		initSingleKey(KeyCode.Tab, "Tab");
		initSingleKey(KeyCode.UpArrow, "↑");
		initSingleKey(KeyCode.DownArrow, "↓");
		initSingleKey(KeyCode.LeftArrow, "←");
		initSingleKey(KeyCode.RightArrow, "→");
		initSingleKey(KeyCode.Space, "Space");
		initSingleKey(KeyCode.LeftShift, "LeftShift");
		initSingleKey(KeyCode.RightShift, "RightShift");
		initSingleKey(KeyCode.LeftControl, "LeftControl");
		initSingleKey(KeyCode.RightControl, "RightControl");
	}
	protected void initSingleKey(KeyCode key, string name)
	{
		if (!mInputSystem.isSupportKey(key))
		{
			logError("不支持此按键的映射");
		}
		mKeyNameList.Add(key, name);
	}
}