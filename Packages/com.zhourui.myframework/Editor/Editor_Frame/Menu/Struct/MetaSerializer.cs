using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// 用于序列化和反序列化Dictionary<string, string>缓存信息
public static class MetaSerializer
{
	private static readonly string CACHE_FILE_PATH = FrameDefine.F_PROJECT_PATH + "Library/MetaCache.json";
	// 将Dictionary<string, string>序列化到JSON文件
	public static void serialize(Dictionary<string, string> dictionary)
	{
		string directory = Path.GetDirectoryName(CACHE_FILE_PATH);
		if (!directory.isEmpty() && !Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		CacheData cacheData = new ();
		foreach (var item in dictionary.safe())
		{
			cacheData.mItemList.Add(new()
			{
				mKey = item.Key ?? string.Empty,
				mValue = item.Value ?? string.Empty,
			});
		}
		File.WriteAllText(CACHE_FILE_PATH, JsonUtility.ToJson(cacheData, true), new UTF8Encoding(false));
	}
	// 从JSON文件反序列化Dictionary<string, string>
	public static Dictionary<string, string> deserialize()
	{
		if (!File.Exists(CACHE_FILE_PATH))
		{
			return null;
		}
		string json = File.ReadAllText(CACHE_FILE_PATH, Encoding.UTF8);
		if (json.isEmpty())
		{
			throw new InvalidDataException("Dictionary<string, string>缓存文件内容为空:" + CACHE_FILE_PATH);
		}

		CacheData cacheData = JsonUtility.FromJson<CacheData>(json) ?? throw new InvalidDataException("Dictionary<string, string>缓存文件格式错误:" + CACHE_FILE_PATH);
		Dictionary<string, string> dictionary = new(cacheData.mItemList.Count);
		foreach (DictionaryItemData item in cacheData.mItemList.safe())
		{
			dictionary.Add(item.mKey ?? string.Empty, item.mValue ?? string.Empty);
		}
		return dictionary;
	}
	[Serializable]
	private class CacheData
	{
		public List<DictionaryItemData> mItemList = new();
	}
	[Serializable]
	private class DictionaryItemData
	{
		public string mKey;
		public string mValue;
	}
}