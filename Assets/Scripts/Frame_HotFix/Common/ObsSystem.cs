using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Net;
using static FileUtility;
using static StringUtility;
using static BinaryUtility;
using static TimeUtility;
using static HttpUtility;

// 用于执行华为云OBS文件存储服务器的访问逻辑,使用前需要确保以下4个参数已经设置正确
// 如果只是下载,则只需要设置URL,上传或者删除则需要配置其余三个参数
public class ObsSystem
{
	protected static string mURL;
	protected static string mBucketName;
	protected static string mAccessKey;
	protected static string mSecureKey;
	public static void setURL(string url) { mURL = url; }
	public static void setBucketName(string name) { mBucketName = name; }
	public static void setAccessKey(string key) { mAccessKey = key; }
	public static void settSecureKey(string key) { mSecureKey = key; }
	// 同步下载文件,remotePath是上传到服务器后存储的相对路径,带后缀
	public static byte[] downloadBytes(string remotePath)
	{
		if (mURL.isEmpty() || remotePath == null)
		{
			return null;
		}
		return downloadFile(mURL + "/" + remotePath);
	}
	// 同步下载文件,remotePath是上传到服务器后存储的相对路径,带后缀
	public static string downloadTxt(string remotePath)
	{
		if (mURL.isEmpty() || remotePath == null)
		{
			return EMPTY;
		}
		return bytesToString(downloadFile(mURL + "/" + remotePath)) ?? EMPTY;
	}
	// 异步下载文件,remotePath是上传到服务器后存储的相对路径,带后缀
	public static void downloadBytes(string remotePath, BytesIntCallback callback)
	{
		if (mURL.isEmpty() || remotePath == null)
		{
			callback?.Invoke(null, 0);
			return;
		}
		ResourceManager.loadAssetsFromUrl(mURL + "/" + remotePath, (byte[] bytes) => { callback?.Invoke(bytes, bytes.count()); });
	}
	// 异步下载文件,字符串格式,remotePath是上传到服务器后存储的相对路径,带后缀
	public static IEnumerator downloadTxtWaiting(string remotePath, StringIntCallback callback)
	{
		if (mURL.isEmpty() || remotePath == null)
		{
			callback?.Invoke(null, 0);
			return null;
		}
		return ResourceManager.loadAssetsFromUrlWaiting(mURL + "/" + remotePath, (byte[] bytes) =>
		{
			callback?.Invoke(bytesToString(bytes) ?? EMPTY, bytes.count());
		});
	}
	// 异步下载文件,byte[],remotePath是上传到服务器后存储的相对路径,带后缀
	public static IEnumerator downloadBytesWaiting(string remotePath, BytesIntCallback callback)
	{
		if (mURL.isEmpty() || remotePath == null)
		{
			callback?.Invoke(null, 0);
			return null;
		}
		return ResourceManager.loadAssetsFromUrlWaiting(mURL + "/" + remotePath, (byte[] bytes) =>
		{
			callback?.Invoke(bytes, bytes.count());
		});
	}
	// 异步下载文件,remotePath是上传到服务器后存储的相对路径,带后缀
	public static void downloadTxt(string remotePath, StringIntCallback callback)
	{
		if (mURL.isEmpty() || remotePath == null)
		{
			callback?.Invoke(null, 0);
			return;
		}
		ResourceManager.loadAssetsFromUrl(mURL + "/" + remotePath, (byte[] bytes) =>
		{
			callback?.Invoke(bytesToString(bytes) ?? EMPTY, bytes.count());
		});
	}
	// fullPath是要上传文件的本地绝对路径,savePath是上传到服务器后存储的相对路径,带后缀
	public static bool upload(string fullPath, byte[] fileBuffer, string savePath, out WebExceptionStatus status, out HttpStatusCode code)
	{
		status = WebExceptionStatus.Success;
		code = HttpStatusCode.OK;
		if (mURL.isEmpty() || fullPath == null)
		{
			return false;
		}
		return !savePath.isEmpty() && httpPostFile(mURL, out status, out code, generateUploadFormList(fullPath, fileBuffer, savePath)) != null;
	}
	public static void uploadAsync(string fullPath, string savePath, HttpCallback callback)
	{
		if (mURL.isEmpty() || fullPath == null)
		{
			return;
		}
		openFileAsync(fullPath, false, (byte[] fileBuffer) =>
		{
			httpPostFileAsync(mURL, generateUploadFormList(fullPath, fileBuffer, savePath), callback);
		});
	}
	public static bool delete(string path)
	{
		if (mURL.isEmpty() || path == null)
		{
			return false;
		}
		string contentType = "application/x-www-form-urlencoded";
		string signature = generateURLSignature(mSecureKey, "DELETE", null, contentType, out string expires, mBucketName, path);
		Dictionary<string, string> paramList = new()
		{
			{ "AccessKeyId", mAccessKey },
			{ "Expires", expires },
			{ "Signature", signature }
		};
		return httpDelete(mURL + "/" + path, out _, out _, paramList, null, contentType) != null;
	}
	public static Dictionary<string, GameFileInfo> getFileList(string path)
	{
		Dictionary<string, GameFileInfo> fileMap = new();
		getFileList(path, fileMap);
		return fileMap;
	}
	public static void getFileList(string path, Dictionary<string, GameFileInfo> fileMap)
	{
		if (mURL.isEmpty() || path == null)
		{
			return;
		}
		fileMap.Clear();
		using var a = new ListScope<GameFileInfo>(out var fileList);
		using var b = new DicScope<string, string>(out var paramList);
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
			str = httpGet(mURL, out _, out _, paramList);
			if (str == null)
			{
				return;
			}
		} while (!parseFileList(str, fileList, out marker));
		int count = fileList.Count;
		for (int i = 0; i < count; ++i)
		{
			GameFileInfo info = fileList[i];
			info.mFileName = info.mFileName.removeStartString(path);
			fileMap.Add(info.mFileName, info);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static List<FormItem> generateUploadFormList(string fullPath, byte[] fileBuffer, string savePath)
	{
		if (fileBuffer == null)
		{
			return null;
		}
		string signature = generatePolicySignature(mBucketName, mSecureKey, savePath, "public-read", out string policyBase64);
		List<FormItem> formList = new()
		{
			new FormItemParam("key", savePath),
			new FormItemParam("x-obs-acl", "public-read"),
			new FormItemParam("AccessKeyId", mAccessKey),
			new FormItemParam("policy", policyBase64),
			new FormItemParam("signature", signature),
			new FormItemFile(fileBuffer, fileBuffer.Length, fullPath)
		};
		return formList;
	}
	protected static string generateHeaderSignature(string secureKey, string verb, string contentMD5_16, string contentType, DateTime date, string bucket, string file)
	{
		string canonicalizedResource = "/" + bucket + "/" + file;
		string contentMD5Base64 = null;
		if (!contentMD5_16.isEmpty())
		{
			contentMD5Base64 = Convert.ToBase64String(stringToBytes(contentMD5_16));
		}
		string stringToSign = verb + "\n" + contentMD5Base64 + "\n" + contentType + "\n" + date.ToString("r") + "\n" + canonicalizedResource;
		byte[] bytes = hmacSha1(secureKey, stringToSign);
		return bytes != null ? Convert.ToBase64String(bytes) : EMPTY;
	}
	protected static string generateURLSignature(string secureKey, string verb, string contentMD5_16, string contentType, out string expires, string bucket, string file)
	{
		string canonicalizedResource = "/" + bucket + "/" + file;
		string contentMD5Base64 = null;
		if (!contentMD5_16.isEmpty())
		{
			contentMD5Base64 = Convert.ToBase64String(stringToBytes(contentMD5_16));
		}
		expires = LToS(dateTimeToTimeStamp(DateTime.Now.AddMinutes(10)));
		string stringToSign = verb + "\n" + contentMD5Base64 + "\n" + contentType + "\n" + expires + "\n" + canonicalizedResource;
		byte[] bytes = hmacSha1(secureKey, stringToSign);
		return bytes != null ? Convert.ToBase64String(bytes) : EMPTY;
	}
	protected static string generatePolicySignature(string bucket, string secureKey, string savePath, string acl, out string policyBase64)
	{
		StringBuilder policy = new();
		// 10分钟后失效
		policy.AppendLine("{\"expiration\": \"" + DateTime.UtcNow.AddMinutes(10).ToString("O") + "\",");
		policy.AppendLine("\"conditions\":[");
		policy.AppendLine("{\"x-obs-acl\": \"" + acl + "\"},");
		policy.AppendLine("{\"bucket\":\"" + bucket + "\"},");
		policy.AppendLine("{\"key\":\"" + savePath + "\"}");
		policy.AppendLine("]");
		policy.Append("}");
		policyBase64 = Convert.ToBase64String(stringToBytes(policy.ToString()));
		byte[] bytes = hmacSha1(secureKey, policyBase64);
		return bytes != null ? Convert.ToBase64String(bytes) : EMPTY;
	}
	// 返回值表示是否已经获取了全部的文件信息,如果没有获取全,nextMarker则会返回下一次获取所需的标记
	protected static bool parseFileList(string str, List<GameFileInfo> fileList, out string nextMarker)
	{
		bool fetchFinish = false;
		nextMarker = null;
		StringReader strReader = new(str);
		var reader = XmlReader.Create(strReader);
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
						info.mFileSize = SToL(reader.Value);
						// 完成一个文件信息的解析
						fileList.Add(info);
						break;
					}
				}
			}
			else if (reader.Name == "IsTruncated")
			{
				reader.Read();
				fetchFinish = !stringToBool(reader.Value);
			}
			else if (reader.Name == "NextMarker")
			{
				reader.Read();
				nextMarker = reader.Value;
			}
		}
		reader.Close();
		strReader.Close();
		return fetchFinish;
	}
	protected static byte[] hmacSha1(string key, string toSign)
	{
		if (key.isEmpty())
		{
			return null;
		}
		return new HMACSHA1(stringToBytes(key)).ComputeHash(stringToBytes(toSign));
	}
}