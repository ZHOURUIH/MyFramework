using System.Collections.Generic;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;

// 游戏委托定义
public delegate void StringCallback(string info);
public delegate void StringListCallback(List<string> info);
public delegate void BytesCallback(byte[] bytes);
public delegate void BytesIntCallback(byte[] bytes, int value);
public delegate void BoolCallback(bool value);
public delegate void AssetLoadDoneCallback(UObject asset, UObject[] assets, byte[] bytes, string loadPath);
public delegate void DownloadCallback(ulong downloadedBytes, int downloadDelta, double deltaTimeMillis, float percent);
public delegate void GameLayoutCallback(GameLayout layout);
public delegate void StringBytesCallback(string str, byte[] bytes);
public delegate void DownloadFileListCallback(StringCallback callback);
public delegate void GameDownloadCallback(float progress, PROGRESS_TYPE type, string info, int bytesPerSecond, int downloadRemainSeconds);
public delegate void GameDownloadTipCallback(DOWNLOAD_TIP type);
public delegate void UnityHttpCallback(string result, UnityWebRequest.Result status, long code);