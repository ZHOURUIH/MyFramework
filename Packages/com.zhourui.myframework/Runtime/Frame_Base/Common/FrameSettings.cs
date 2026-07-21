using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 运行时可读取的框架设置资源
public class FrameSettings : ScriptableObject
{
    [Tooltip("桌面端UI标准分辨率宽高,根据此设置来决定UI的适配")]
    public Vector2Int UISizeStandalone = new(1920, 1080);
    [Tooltip("移动端UI标准分辨率宽高,根据此设置来决定UI的适配")]
    public Vector2Int UISizeMobile = new(1920, 1080);
	[Tooltip("允许动态下载的目录列表,GameResources下的相对路径,此列表中的文件不会打包到包体中,也不会在游戏启动时从服务器下载,而是在加载资源时才会进行下载")]
	public List<string> DynamicDownloadList = new();
	[Tooltip("安卓插件的包名,也就是自己的安卓工程代码中定义的包名,用于在C#中访问java代码")]
	public string AndroidPluginBundleName = "com.your.packagename";
	[Tooltip("热更dll加密的Key,实际上会再处理一次计算出最终的Key,是一个16个byte的十六进制形式的字符串,比如FFAE4F表示3个byte:0xFF,0xAE,0x4F,所以这里字符串的长度必须等于16*2=32")]
	public string HotFixDllAesKey = "1234567890ABCDEF1234567890ABCDEF";
	[Tooltip("热更dll加密的IV,实际上会再处理一次计算出最终的IV,是一个16个byte的十六进制形式的字符串,比如FFAE4F表示3个byte:0xFF,0xAE,0x4F,所以这里字符串的长度必须等于16*2=32")]
	public string HotFixDllAesIV = "FEDCBA0987654321FEDCBA0987654321";
	[Tooltip("网络消息的加密密钥,长度必须为2的n次方,长度可以尽量长一些,相对更安全一点,也是一个16个byte的十六进制形式的字符串")]
	public string NetPacketEncryptKey = "FEDCBA0987654321FEDCBA0987654321";
	[Tooltip("辅助加密的整数key,长度固定为4个整数,可以指定任意的整数,只要跟服务器的值对上就行")]
	public int[] NetPacketEncryptKeyHelper = new int[4];

	private byte[] AesKeyBytes;
	private byte[] AesIVBytes;
	private byte[] EncryptKeyBytes;
	private static FrameSettings mFrameSettings;                    // 当前运行时设置
    private static FrameSettings get()
    {
        if (mFrameSettings != null)
        {
            return mFrameSettings;
        }

        string suffix = ".asset";
        mFrameSettings = Resources.Load<FrameSettings>(RUNTIME_SETTINGS_RES_PATH[..^suffix.Length]);
        if (mFrameSettings != null)
        {
            return mFrameSettings;
        }

        Debug.LogError("未找到运行时框架设置:" + P_RESOURCES_PATH + RUNTIME_SETTINGS_RES_PATH);
        mFrameSettings = CreateInstance<FrameSettings>();
        return mFrameSettings;
    }
    public static Vector2Int getUISize()
    {
        if (isMobile())
        {
            return get().UISizeMobile;
        }
        else
        {
            return get().UISizeStandalone;
        }
    }
    public static List<string> getDynamicDownloadList() { return get().DynamicDownloadList; }
    public static string getAndroidPluginBundleName()	{ return get().AndroidPluginBundleName; }
	public static byte[] getAESKey()
	{
		FrameSettings instance = get();
		if (instance.AesKeyBytes == null && !string.IsNullOrEmpty(instance.HotFixDllAesKey))
		{
			// 将密钥再加一次密
			instance.AesKeyBytes = new byte[16];
			Buffer.BlockCopy(generateMD5(hexStringToBytes(instance.HotFixDllAesKey)), 0, instance.AesKeyBytes, 0, instance.AesKeyBytes.Length);
		}
		return instance.AesKeyBytes;
	}
	public static byte[] getAESIV()
	{
		FrameSettings instance = get();
		if (instance.AesIVBytes == null && !string.IsNullOrEmpty(instance.HotFixDllAesIV))
		{
			// 将密钥再加一次密
			instance.AesIVBytes = new byte[16];
			Buffer.BlockCopy(generateMD5(hexStringToBytes(instance.HotFixDllAesIV)), 0, instance.AesIVBytes, 0, instance.AesIVBytes.Length);
		}
		return instance.AesIVBytes;
	}
	public static byte[] getEncryptKey()
	{
		FrameSettings instance = get();
		if (instance.EncryptKeyBytes == null && !string.IsNullOrEmpty(instance.NetPacketEncryptKey))
		{
			instance.EncryptKeyBytes = new byte[instance.NetPacketEncryptKey.Length / 2];
			Buffer.BlockCopy(hexStringToBytes(instance.NetPacketEncryptKey), 0, instance.EncryptKeyBytes, 0, instance.EncryptKeyBytes.Length);
		}
		return instance.EncryptKeyBytes;
	}
	public static int[] getEncryptKeyHelper() 
	{
		FrameSettings instance = get();
		if (instance.NetPacketEncryptKeyHelper.Length != 4)
		{
			logErrorBase("NetPacketEncryptKeyHelper的长度必须为4!");
		}
		return instance.NetPacketEncryptKeyHelper; 
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 计算一个文件的MD5
	public static byte[] generateMD5(byte[] fileContent)
	{
		if (fileContent == null || fileContent.Length == 0)
		{
			return null;
		}
		try
		{
			using MD5CryptoServiceProvider md5Obj = new();
			return md5Obj.ComputeHash(fileContent, 0, fileContent.Length);
		}
		catch (Exception e)
		{
			logErrorBase(e.Message);
		}
		return null;
	}
	protected static byte[] hexStringToBytes(string str)
	{
        if (str.Length != 32)
        {
			logErrorBase("密钥长度错误:" + str);
            return null;
        }
		foreach (char c in str)
		{
			if (!isByteHexChar(c))
			{
				logErrorBase("密钥不是十六进制字符串:" + str);
				return null;
			}
		}
        byte[] bytes = new byte[16];
		for (int i = 0; i < str.Length / 2; ++i)
		{
			bytes[i] = hexStringToByte(str, i * 2);
		}
		return bytes;
	}
	public static byte hexStringToByte(string str, int start = 0)
	{
		byte highBit = 0;
		byte lowBit = 0;
		byte[] strBytes = Encoding.UTF8.GetBytes(str);
		byte highBitChar = strBytes[start];
		byte lowBitChar = strBytes[start + 1];
		if (highBitChar >= 'A' && highBitChar <= 'F')
		{
			highBit = (byte)(10 + highBitChar - 'A');
		}
		else if (highBitChar >= 'a' && highBitChar <= 'f')
		{
			highBit = (byte)(10 + highBitChar - 'a');
		}
		else if (highBitChar >= '0' && highBitChar <= '9')
		{
			highBit = (byte)(highBitChar - '0');
		}
		if (lowBitChar >= 'A' && lowBitChar <= 'F')
		{
			lowBit = (byte)(10 + lowBitChar - 'A');
		}
		else if (lowBitChar >= 'a' && lowBitChar <= 'f')
		{
			lowBit = (byte)(10 + lowBitChar - 'a');
		}
		else if (lowBitChar >= '0' && lowBitChar <= '9')
		{
			lowBit = (byte)(lowBitChar - '0');
		}
		return (byte)(highBit << 4 | lowBit);
	}
	public static bool isByteHexChar(char c)
	{
		return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F');
	}
}