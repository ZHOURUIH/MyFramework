using UnityEngine;
using UnityEditor;
using static EditorFileUtility;
using static FileUtility;
using static UnityUtility;
using static GameUtility;

public class DecryptDllWindow : GameEditorWindow
{
	public void start()
	{
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
		writeFile(fileName + ".decrypt", decryptAES(fileBytes, getAESKeyBytes(), getAESIVBytes()));
		log("完成解密");
	}
}
