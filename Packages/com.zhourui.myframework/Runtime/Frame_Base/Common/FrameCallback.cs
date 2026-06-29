using System.Collections.Generic;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;

public delegate void StringCallback(string info);
public delegate void StringListCallback(List<string> info);
public delegate void BytesCallback(byte[] bytes);
public delegate void BytesIntCallback(byte[] bytes, int value);
public delegate void BoolCallback(bool value);
public delegate void DownloadCallback(ulong downloadedBytes, int downloadDelta, double deltaTimeMillis, float percent);
public delegate void StringBytesCallback(string str, byte[] bytes);
public delegate void UnityHttpCallback(string result, UnityWebRequest.Result status, long code);
public delegate void AssetLoadDoneCallback(UObject asset, UObject[] assets, byte[] bytes, string loadPath);