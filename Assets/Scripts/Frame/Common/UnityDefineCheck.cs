using UnityEngine;
using System;
using System.Collections.Generic;

public class UnityDefineCheck
{
#if UNITY_EDITOR
	static public bool _UNITY_EDITOR = true;
#else
	static public bool _UNITY_EDITOR = false;
#endif

#if UNITY_ANDROID
	static public bool _UNITY_ANDROID = true;
#else
	static public bool _UNITY_ANDROID = false;
#endif

#if UNITY_STANDALONE_WIN
	static public bool _UNITY_STANDALONE_WIN = true;
#else
	static public bool _UNITY_STANDALONE_WIN = false;
#endif

#if UNITY_STANDALONE_LINUX
	static public bool _UNITY_STANDALONE_LINUX = true;
#else
	static public bool _UNITY_STANDALONE_LINUX = false;
#endif

#if UNITY_IOS
	static public bool _UNITY_IOS = true;
#else
	static public bool _UNITY_IOS = false;
#endif

#if UNITY_2018
	static public bool _UNITY_2018 = true;
#else
	static public bool _UNITY_2018 = false;
#endif
}