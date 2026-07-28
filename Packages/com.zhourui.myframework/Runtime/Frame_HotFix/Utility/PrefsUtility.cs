using UPlayerPrefs = UnityEngine.PlayerPrefs;
#if BYTE_DANCE
using TTPlayerPrefs = TTSDK.TT.PlayerPrefs;
#endif

// 与PlayerPrefs相关的工具函数
public class PrefsUtility
{
	public static bool prefsGetBool(string key, bool defaultValue = false)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		return TTPlayerPrefs.GetInt(key, defaultValue ? 1 : 0) > 0;
#else
		return UPlayerPrefs.GetInt(key, defaultValue ? 1 : 0) > 0;
#endif
	}
	public static void prefsSetBool(string key, bool value, bool save = true)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		TTPlayerPrefs.SetInt(key, value ? 1 : 0);
		if (save)
		{
			TTPlayerPrefs.Save();
		}
#else
		UPlayerPrefs.SetInt(key, value ? 1 : 0);
		if (save)
		{
			UPlayerPrefs.Save();
		}
#endif
	}
	public static int prefsGetInt(string key, int defaultValue = 0)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		return TTPlayerPrefs.GetInt(key, defaultValue);
#else
		return UPlayerPrefs.GetInt(key, defaultValue);
#endif
	}
	public static void prefsSetInt(string key, int value, bool save = true)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		TTPlayerPrefs.SetInt(key, value);
		if (save)
		{
			TTPlayerPrefs.Save();
		}
#else
		UPlayerPrefs.SetInt(key, value);
		if (save)
		{
			UPlayerPrefs.Save();
		}
#endif
	}
	public static float prefsGetFloat(string key, float defaultValue = 0.0f)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		return TTPlayerPrefs.GetFloat(key, defaultValue);
#else
		return UPlayerPrefs.GetFloat(key, defaultValue);
#endif
	}
	public static void prefsSetFloat(string key, float value, bool save = true)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		TTPlayerPrefs.SetFloat(key, value);
		if (save)
		{
			TTPlayerPrefs.Save();
		}
#else
		UPlayerPrefs.SetFloat(key, value);
		if (save)
		{
			UPlayerPrefs.Save();
		}
#endif
	}
	public static string prefsGetString(string key)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		return TTPlayerPrefs.GetString(key);
#else
		return UPlayerPrefs.GetString(key);
#endif
	}
	public static void prefsSetString(string key, string value, bool save = true)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		TTPlayerPrefs.SetString(key, value);
		if (save)
		{
			TTPlayerPrefs.Save();
		}
#else
		UPlayerPrefs.SetString(key, value);
		if (save)
		{
			UPlayerPrefs.Save();
		}
#endif
	}
	public static bool prefsHasKey(string key)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		return TTPlayerPrefs.HasKey(key);
#else
		return UPlayerPrefs.HasKey(key);
#endif
	}
	public static void prefsDeleteKey(string key)
	{
#if !UNITY_EDITOR && BYTE_DANCE
		TTPlayerPrefs.DeleteKey(key);
#else
		UPlayerPrefs.DeleteKey(key);
#endif
	}
}