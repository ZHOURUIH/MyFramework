using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static FrameBaseDefine;
public class MiniGamePlayerPrefs
{
	private static Dictionary<string, string> mValues = new();
	private static bool mLoaded;
	public static int GetInt(string key, int defaultValue = 0)
	{
		return int.TryParse(GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : defaultValue;
	}
	public static void SetInt(string key, int value)
	{
		SetString(key, value.ToString(CultureInfo.InvariantCulture));
	}
	public static float GetFloat(string key, float defaultValue = 0.0f)
	{
		return float.TryParse(GetString(key), NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : defaultValue;
	}
	public static void SetFloat(string key, float value)
	{
		SetString(key, value.ToString("R", CultureInfo.InvariantCulture));
	}
	public static string GetString(string key)
	{
		ensureLoaded();
		return mValues.TryGetValue(key, out string value) ? value : "";
	}
	public static void SetString(string key, string value)
	{
		ensureLoaded();
		mValues[key] = value ?? "";
	}
	public static bool HasKey(string key)
	{
		ensureLoaded();
		return mValues.ContainsKey(key);
	}
	public static void DeleteKey(string key)
	{
		ensureLoaded();
		mValues.Remove(key);
	}
	public static void DeleteAll()
	{
		ensureLoaded();
		mValues.Clear();
	}
	public static void Save()
	{
		ensureLoaded();
		using MemoryStream stream = new();
		using BinaryWriter writer = new(stream);
		writer.Write(mValues.Count);
		foreach (var item in mValues)
		{
			writer.Write(item.Key);
			writer.Write(item.Value);
		}
		writer.Flush();
		string folder = F_PERSISTENT_DATA_PATH + "MyFrameworkPrefs/";
#if OPPO_MINI_GAME || VIVO_MINI_GAME
		QuickGameFileSystem.createDirectory(folder);
		QuickGameFileSystem.writeBytes(folder + "PlayerPrefs.bytes", stream.ToArray());
#elif UNITY_WEIXINMINIGAME
		WeChatFileSystem.createDirectory(folder);
		WeChatFileSystem.writeBytes(folder + "PlayerPrefs.bytes", stream.ToArray());
#endif
	}
	//--------------------------------------------------------------------------------------------------------------
	private static void ensureLoaded()
	{
		if (mLoaded)
		{
			return;
		}
		string file = F_PERSISTENT_DATA_PATH + "MyFrameworkPrefs/PlayerPrefs.bytes";
		byte[] bytes = null;
#if OPPO_MINI_GAME || VIVO_MINI_GAME
		if (QuickGameFileSystem.isFileExist(file))
		{
			bytes = QuickGameFileSystem.readBytes(file);
			if (bytes == null)
			{
				throw new IOException("Cannot read player preferences: " + file);
			}
		}
#elif UNITY_WEIXINMINIGAME
		if (WeChatFileSystem.isFileExist(file))
		{
			bytes = WeChatFileSystem.readBytes(file);
			if (bytes == null)
			{
				throw new IOException("Cannot read player preferences: " + file);
			}
		}
#endif
		if (bytes != null)
		{
			using MemoryStream stream = new(bytes);
			using BinaryReader reader = new(stream);
			int count = reader.ReadInt32();
			Dictionary<string, string> values = new();
			for (int i = 0; i < count; ++i)
			{
				values.Add(reader.ReadString(), reader.ReadString());
			}
			mValues = values;
		}
		mLoaded = true;
	}
}
