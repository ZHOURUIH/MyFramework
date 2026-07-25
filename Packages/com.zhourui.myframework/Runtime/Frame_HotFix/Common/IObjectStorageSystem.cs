using System.Net;
using System.Collections.Generic;

// 对象存储的接口,也就是类似华为云obs,阿里云oss,七牛云这类的访问接口
public interface IObjectStorageSystem
{
	void init(string url, string bucket, string accessKey, string secureKey);
	string getURL();
	Dictionary<string, GameFileInfo> getFileList(string path);
	string downloadTxt(string remotePath);
	bool delete(string remoteFullPath);
	HttpStatusCode upload(string fullPath, string savePath);
}