using System.Runtime.InteropServices;

public class iOSDllImportFrameBase
{
#if UNITY_IOS
    [DllImport("__Internal")]
	static public extern void reportException(string name, string reason, string stack);
    [DllImport("__Internal")]
    static public extern void iOSLog(string str);
#else
	static public void reportException(string name, string reason, string stack) { }
	static public void iOSLog(string str) { UnityEngine.Debug.Log(str); }
#endif
}