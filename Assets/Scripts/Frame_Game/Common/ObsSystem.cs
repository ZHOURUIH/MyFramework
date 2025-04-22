using System.Collections.Generic;
using System.IO;
using System.Xml;
using static HttpUtility;
using static FileUtility;

// 用于执行华为云OBS文件存储服务器的访问逻辑,使用前需要确保以下5个参数已经设置正确
// 如果只是下载,则只需要设置URL和RemoteFolder,上传或者删除则需要配置其余三个参数
public class ObsSystem
{
	protected static string mURL;
	protected static string mBucketName;
	protected static string mAccessKey;
	protected static string mSecureKey;
	public static void setURLAndKeys(string url, string bucketName, string accessKey, string secureKey)
	{
		mURL = url;
		mBucketName = bucketName;
		mAccessKey = accessKey;
		mSecureKey = secureKey;
	}
	// 异步下载文件,remotePath是上传到服务器后存储的相对路径,带后缀
	public static void downloadBytes(string remotePath, BytesIntCallback callback)
	{
		ResourceManager.loadAssetsFromUrl(mURL + remotePath, (byte[] bytes) => { callback?.Invoke(bytes, bytes?.Length ?? 0); });
	}
	// 异步下载文件,remotePath是上传到服务器后存储的相对路径,带后缀
	public static void downloadTxt(string remotePath, StringCallback callback)
	{
		ResourceManager.loadAssetsFromUrl(mURL + remotePath, (byte[] bytes) => { callback?.Invoke(bytesToString(bytes) ?? ""); });
	}
	// fileName是URL下的相对路径
	public static string getFileMD5(string fileName)
	{
		foreach (GameFileInfo file in getFileListInternal(fileName))
		{
			if (file.mFileName == fileName)
			{
				return file.mMD5;
			}
		}
		return null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static List<GameFileInfo> getFileListInternal(string path)
	{
		List<GameFileInfo> fileList = new();
		Dictionary<string, string> paramList = new();
		string marker = null;
		string str;
		do
		{
			paramList.Clear();
			if (!marker.isEmpty())
			{
				paramList.Add("marker", marker);
			}
			paramList.Add("prefix", path);
			str = httpGet(mURL, out _, out _, paramList, null);
			if (str == null)
			{
				return null;
			}
		} while (!parseFileList(str, fileList, out marker));
		return fileList;
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