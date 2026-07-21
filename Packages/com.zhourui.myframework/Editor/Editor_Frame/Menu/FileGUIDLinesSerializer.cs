using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

// 用于序列化和反序列化查找文件引用的缓存信息
public static class FileGUIDLinesSerializer
{
	private static readonly string CACHE_FILE_PATH = FrameDefine.F_PROJECT_PATH + "Library/FileGUIDLinesCache.json";
	// 将Dictionary<string, List<FileRefGUIDs>>序列化到本地缓存
	public static void serialize(Dictionary<string, List<FileRefGUIDs>> dictionary)
	{
		string directory = Path.GetDirectoryName(CACHE_FILE_PATH);
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		CacheData cacheData = new();
		Dictionary<FileRefGUIDs, int> fileInfoIndexMap = new(ReferenceComparer<FileRefGUIDs>.Instance);
		foreach (var dictionaryItem in dictionary.safe())
		{
			foreach (FileRefGUIDs fileInfo in dictionaryItem.Value.safe())
			{
				if (fileInfo == null || fileInfoIndexMap.ContainsKey(fileInfo))
				{
					continue;
				}
				fileInfoIndexMap.Add(fileInfo, cacheData.mFileInfoList.Count);
				FileInfoData fileInfoData = new()
				{
					mProjectFileName = fileInfo.mProjectFileName ?? string.Empty,
					mProjectFileGUID = fileInfo.mProjectFileGUID ?? string.Empty,
					mGUIDs = fileInfo.mGUIDs != null ? new(fileInfo.mGUIDs) : new()
				};
				cacheData.mFileInfoList.Add(fileInfoData);
			}
		}

		foreach (var dictionaryItem in dictionary.safe())
		{
			DictionaryItemData dictionaryItemData = new()
			{
				mGUID = dictionaryItem.Key ?? string.Empty
			};
			foreach (FileRefGUIDs fileInfo in dictionaryItem.Value.safe())
			{
				dictionaryItemData.mFileInfoIndexes.Add(fileInfo != null ? fileInfoIndexMap[fileInfo] : -1);
			}
			cacheData.mDictionaryItemList.Add(dictionaryItemData);
		}
		File.WriteAllText(CACHE_FILE_PATH, JsonUtility.ToJson(cacheData, true), new UTF8Encoding(false));
	}
	// 从本地缓存反序列化Dictionary<string, List<FileRefGUIDs>>
	public static Dictionary<string, List<FileRefGUIDs>> deserialize()
	{
		if (!File.Exists(CACHE_FILE_PATH))
		{
			return null;
		}
		string json = File.ReadAllText(CACHE_FILE_PATH, Encoding.UTF8);
		if (json.isEmpty())
		{
			throw new InvalidDataException("FileGUIDLines缓存文件内容为空:" + CACHE_FILE_PATH);
		}
		CacheData cacheData = JsonUtility.FromJson<CacheData>(json) ?? throw new InvalidDataException("FileGUIDLines缓存文件格式错误:" + CACHE_FILE_PATH);
		List<FileRefGUIDs> fileInfoList = new();
		foreach (FileInfoData fileInfoData in cacheData.mFileInfoList)
		{
			FileRefGUIDs fileInfo = new()
			{
				mProjectFileName = fileInfoData.mProjectFileName,
				mProjectFileGUID = fileInfoData.mProjectFileGUID,
				mGUIDs = fileInfoData.mGUIDs != null ? new(fileInfoData.mGUIDs) : new()
			};
			// 如果guid为空,可能这个文件就没有meta,不认为是项目资源
			fileInfoList.Add(fileInfo.mProjectFileGUID != "" ? fileInfo : null);
		}

		Dictionary<string, List<FileRefGUIDs>> dictionary = new(cacheData.mDictionaryItemList.Count);
		foreach (DictionaryItemData dictionaryItemData in cacheData.mDictionaryItemList)
		{
			List<FileRefGUIDs> valueList = new(dictionaryItemData.mFileInfoIndexes.Count);
			foreach (int fileInfoIndex in dictionaryItemData.mFileInfoIndexes)
			{
				valueList.addNotNull(fileInfoList.get(fileInfoIndex));
			}
			dictionary.Add(dictionaryItemData.mGUID, valueList);
		}
		return dictionary;
	}
	[Serializable]
	private class CacheData
	{
		public List<FileInfoData> mFileInfoList = new();
		public List<DictionaryItemData> mDictionaryItemList = new();
	}
	[Serializable]
	private class FileInfoData
	{
		public string mProjectFileName;
		public string mProjectFileGUID;
		public List<string> mGUIDs = new();
	}
	[Serializable]
	private class DictionaryItemData
	{
		public string mGUID;
		public List<int> mFileInfoIndexes = new();
	}
}

public sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
{
	public static readonly ReferenceComparer<T> Instance = new ();
	public bool Equals(T left, T right)
	{
		return ReferenceEquals(left, right);
	}
	public int GetHashCode(T value)
	{
		return RuntimeHelpers.GetHashCode(value);
	}
}