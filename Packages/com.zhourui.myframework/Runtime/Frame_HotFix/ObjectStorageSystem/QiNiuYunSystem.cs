#if USE_QI_NIU_YUN
using Qiniu.CDN;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using System.Collections.Generic;
using System.Net;
using static FileUtility;
using static HttpUtility;
using static UnityUtility;

// 七牛云的封装,一般在编辑器模式下访问的,用于上传和下载
public class QiNiuYunSystem : IObjectStorageSystem
{
	public string mURL;
	public string mBucket;
	public string mAccessKey;
	public string mSecretKey;
	private static QiNiuYunSystem mInstance;
	public static QiNiuYunSystem get() { return mInstance ??= new QiNiuYunSystem(); }
	public void init(string url, string bucket, string accessKey, string secureKey)
	{
		validPath(ref url);
		mURL = url;
		mBucket = bucket;
		mAccessKey = accessKey;
		mSecretKey = secureKey;
	}
	// 同步下载文件,remotePath是上传到服务器后存储的相对路径,带后缀
	public string downloadTxt(string remotePath)
	{
		if (mURL.isEmpty() || remotePath == null)
		{
			return string.Empty;
		}
		return downloadFile(mURL + remotePath).bytesToString();
	}
	// remotePath是远端的相对路径,是桶内的路径
	// fullPath文件本地绝对路径,noCache为true则表示每次下载此文件都不经过缓存,一般只有版本号文件会设置为true
	public HttpStatusCode upload(string fullPath, string remotePath, bool noCache)
	{
		// 表单上传
		FormUploader target = new(generateConfig());
		PutExtra extra = null;
		if (noCache)
		{
			extra = new PutExtra();
			extra.MimeType = "text/plain";
			extra.CacheControl = "no-cache, no-store, must-revalidate";
		}
		HttpResult result = target.UploadFile(fullPath, remotePath, generateUploadToken(remotePath), extra);
		if (result.Code != (int)HttpCode.OK)
		{
			logWarning("upload error: " + result.ToString() + ", file:" + fullPath);
		}
		else
		{
			log("完成上传文件:" + fullPath + " -> " + remotePath);
		}
		return (HttpStatusCode)result.Code;
	}
	public Dictionary<string, GameFileInfo> getFileList(string remotePath)
	{
		BucketManager bucketManager = new(new(mAccessKey, mSecretKey), generateConfig());
		ListResult result = bucketManager.ListFiles(mBucket, remotePath, "", 1000, "");
		Dictionary<string, GameFileInfo> fileList = new();
		foreach (ListItem item in (result?.Result?.Items).safe())
		{
			if (item.Key == remotePath)
			{
				continue;
			}
			GameFileInfo info = new();
			info.mFileName = item.Key.removeStart(remotePath);
			info.mFileSize = item.Fsize;
			info.mMD5 = item.Md5;
			fileList.Add(info.mFileName, info);
		}
		return fileList;
	}
	public bool delete(string remotePath)
	{
		BucketManager bucketManager = new(new(mAccessKey, mSecretKey), generateConfig());
		HttpResult result = bucketManager.Delete(mBucket, remotePath);
		if (result.Code != (int)HttpCode.OK)
		{
			logWarning("delete error: " + result.ToString() + ", file:" + remotePath);
		}
		else
		{
			log("delete file:" + remotePath);
		}
		return result.Code == (int)HttpCode.OK;
	}
	public bool move(string remoteFilePath, string remoteDestPath)
	{
		BucketManager bucketManager = new(new(mAccessKey, mSecretKey), generateConfig());
		HttpResult result = bucketManager.Move(mBucket, remoteFilePath, mBucket, remoteDestPath, false);
		if (result.Code != (int)HttpCode.OK)
		{
			logWarning("move error: " + result.ToString() + ", source file:" + remoteFilePath + ", dest file:" + remoteDestPath);
		}
		else
		{
			log("move file:" + ", source file:" + remoteFilePath + ", dest file:" + remoteDestPath);
		}
		return result.Code == (int)HttpCode.OK;
	}
	public bool refreshCDN(string remotePath)
	{
		string url = mURL + remotePath.Replace('\\', '/').TrimStart('/');
		CdnManager manager = new(new Mac(mAccessKey, mSecretKey));
		RefreshResult result = manager.RefreshUrls(new[] { url });
		if (result.Code != (int)HttpCode.OK ||
			result.Result == null ||
			result.Result.Code != (int)HttpCode.OK)
		{
			logWarning("刷新CDN缓存失败:" + ",URL:" + url + ",结果:" + result);
			return false;
		}
		log("已提交CDN缓存刷新,URL:" + url + ",RequestID:" + result.Result.RequestId + ",今日剩余额度:" + result.Result.UrlSurplusDay);
		return true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static Config generateConfig()
	{
		Config config = new();
		// 设置上传区域,华东
		config.Zone = Zone.ZONE_CN_East;
		// 设置 http 或者 https 上传
		config.UseHttps = true;
		config.UseCdnDomains = true;
		config.ChunkSize = ChunkUnit.U512K;
		return config;
	}
	// 如果允许覆盖远端同名文件,则需要将文件路径传进来
	protected string generateUploadToken(string filePath)
	{
		PutPolicy putPolicy = new();
		if (filePath.isEmpty())
		{
			putPolicy.Scope = mBucket;
		}
		else
		{
			putPolicy.Scope = mBucket + ":" + filePath;
		}
		putPolicy.InsertOnly = 0;
		return Auth.CreateUploadToken(new(mAccessKey, mSecretKey), putPolicy.ToJsonString());
	}
}
#endif