using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine.Networking;
using static HttpUtility;

// 用于执行华为云OBS文件存储服务器的访问逻辑,只用于获取远端文件信息
public class ObsSystem
{
	// fileName是url下的相对路径
	public static void getFileMD5(string url, string fileName, StringCallback callback)
	{
		getFileInfoInternal(url, fileName, (GameFileInfo info) => { callback?.Invoke(info?.mMD5); });
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void getFileInfoInternal(string url, string fileName, Action<GameFileInfo> callback)
	{
		Dictionary<string, string> paramList = new() { { "prefix", fileName } };
		httpGetAsyncWebGL(url, paramList, (string result, UnityWebRequest.Result status, long code) =>
		{
			List<GameFileInfo> fileList = new();
			parseFileList(result, fileList, out _);
			GameFileInfo file = null;
			foreach (GameFileInfo info in fileList)
			{
				if (info.mFileName == fileName)
				{
					file = info;
					break;
				}
			}
			callback?.Invoke(file);
		});
	}
	// 返回值表示是否已经获取了全部的文件信息,如果没有获取全,nextMarker则会返回下一次获取所需的标记
	protected static bool parseFileList(string str, List<GameFileInfo> fileList, out string nextMarker)
	{
		bool fetchFinish = false;
		nextMarker = null;
		using StringReader strReader = new(str);
		using var reader = XmlReader.Create(strReader);
		while (reader.Read())
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			if (reader.Name == "Contents")
			{
				GameFileInfo info = new();
				while (reader.Read())
				{
					if (reader.NodeType != XmlNodeType.Element)
					{
						continue;
					}
					string name = reader.Name;
					reader.Read();
					if (name == "Key")
					{
						info.mFileName = reader.Value;
						// 以/结尾的是目录,不需要放入列表
						if (reader.Value[^1] == '/')
						{
							break;
						}
					}
					else if (name == "ETag")
					{
						info.mMD5 = reader.Value.removeAll('\"');
					}
					else if (name == "Size")
					{
						long.TryParse(reader.Value, out info.mFileSize);
						// 完成一个文件信息的解析
						fileList.Add(info);
						break;
					}
				}
			}
			else if (reader.Name == "IsTruncated")
			{
				reader.Read();
				fetchFinish = reader.Value != "true" && reader.Value != "True" && reader.Value != "TRUE";
			}
			else if (reader.Name == "NextMarker")
			{
				reader.Read();
				nextMarker = reader.Value;
			}
		}
		return fetchFinish;
	}
}