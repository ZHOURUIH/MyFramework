using System;
using UnityEngine;
using UnityEditor;
using static EditorFileUtility;
using static FileUtility;
using static UnityUtility;

public class DecryptDllWindow : GameEditorWindow
{
	protected Func<byte[]> mAESKeyBytesFunc;
	protected Func<byte[]> mAESIVBytesFunc;
    public void start(Func<byte[]> AESKeyBytesFunc, Func<byte[]> AESIVBytesFunc)
	{
        mAESKeyBytesFunc = AESKeyBytesFunc;
        mAESIVBytesFunc = AESIVBytesFunc;
		if (mAESKeyBytesFunc == null || mAESIVBytesFunc == null)
		{
			logError("如果想要正常使用此窗口,需要传入有效的AEesKey和AESIV获取方法,请在应用层中实现对应的方法后,重新写一个方法调用此start");
			return;
		}
        Show();
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected override void onGUI()
	{
		if (button("选择dll文件", 200, 75))
		{
			decryptDll(EditorUtility.OpenFilePanel("", Application.dataPath, "*"));
		}
	}
	protected void decryptDll(string fileName)
	{
		if (fileName.isEmpty())
		{
			return;
		}
		byte[] fileBytes = openFile(fileName, true);
		if (fileBytes.count() == 0)
		{
			logError("文件打开错误");
			return;
		}
		writeFile(fileName + ".decrypt", decryptAES(fileBytes, mAESKeyBytesFunc?.Invoke(), mAESIVBytesFunc?.Invoke()));
		log("完成解密");
	}
}
