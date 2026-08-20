using System.Collections.Generic;
using System.Security.Cryptography;
using static TestAssert;

// Frame_Game 精简层 FileUtility 纯逻辑测试(文件 IO/异步/StreamingAssets 路径不测)
public static class FileUtilityTest
{
	public static void Run()
	{
		testValidPathNoSlash();
		testValidPathHasSlash();
		testValidPathEmpty();
		testGenerateFileMD5Empty();
		testGenerateFileMD5Deterministic();
		testGenerateFileMD5Different();
		testDecryptAESNullKey();
		testDecryptAESEmptyKey();
		testDecryptAESRoundTrip();
		testParseFileListEmpty();
		testParseFileListValid();
		testCheckDeleteFile();
	}

	// validPath: 不以 / 结尾加 /
	static void testValidPathNoSlash()
	{
		string path = "a/b";
		FileUtility.validPath(ref path);
		assertEqual("a/b/", path, "加结尾斜杠");
	}

	// validPath: 已有 / 不变
	static void testValidPathHasSlash()
	{
		string path = "a/b/";
		FileUtility.validPath(ref path);
		assertEqual("a/b/", path, "已有斜杠不变");
	}

	// validPath: 空串不变
	static void testValidPathEmpty()
	{
		string path = "";
		FileUtility.validPath(ref path);
		assertEqual("", path, "空串不变");
	}

	// generateFileMD5: null/空 → 空串
	static void testGenerateFileMD5Empty()
	{
		assertEqual("", FileUtility.generateFileMD5(null), "null 返回空");
		assertEqual("", FileUtility.generateFileMD5(new byte[0]), "空数组返回空");
	}

	// generateFileMD5: 同输入同输出(确定性)
	static void testGenerateFileMD5Deterministic()
	{
		byte[] data = new byte[] { 1, 2, 3, 4, 5 };
		string a = FileUtility.generateFileMD5(data);
		string b = FileUtility.generateFileMD5(data);
		assertEqual(a, b, "MD5 确定性");
		assertFalse(a.isEmpty(), "MD5 非空");
	}

	// generateFileMD5: 不同输入不同输出
	static void testGenerateFileMD5Different()
	{
		string a = FileUtility.generateFileMD5(new byte[] { 1, 2, 3 });
		string b = FileUtility.generateFileMD5(new byte[] { 1, 2, 4 });
		assertFalse(a == b, "不同输入 MD5 不同");
	}

	// decryptAES: key/iv null → 返回原 data
	static void testDecryptAESNullKey()
	{
		byte[] data = new byte[] { 1, 2, 3 };
		assertEqual(data, FileUtility.decryptAES(data, null, new byte[16]), "key null 原样返回");
		assertEqual(data, FileUtility.decryptAES(data, new byte[16], null), "iv null 原样返回");
	}

	// decryptAES: 空 key/iv → 返回原 data
	static void testDecryptAESEmptyKey()
	{
		byte[] data = new byte[] { 1, 2, 3 };
		assertEqual(data, FileUtility.decryptAES(data, new byte[0], new byte[16]), "空 key 原样返回");
		assertEqual(data, FileUtility.decryptAES(data, new byte[16], new byte[0]), "空 iv 原样返回");
	}

	// decryptAES: 加密后解密还原
	static void testDecryptAESRoundTrip()
	{
		byte[] key = new byte[16];
		byte[] iv = new byte[16];
		for (int i = 0; i < 16; ++i)
		{
			key[i] = (byte)i;
			iv[i] = (byte)(i + 1);
		}
		byte[] plain = new byte[] { 10, 20, 30, 40, 50 };
		byte[] encrypted;
		using (Aes aes = Aes.Create())
		{
			aes.Key = key;
			aes.IV = iv;
			using var enc = aes.CreateEncryptor();
			encrypted = enc.TransformFinalBlock(plain, 0, plain.Length);
		}
		byte[] decrypted = FileUtility.decryptAES(encrypted, key, iv);
		assertEqual(plain.Length, decrypted.Length, "解密长度一致");
		for (int i = 0; i < plain.Length; ++i)
		{
			assertEqual(plain[i], decrypted[i], "解密字节一致 index " + i);
		}
	}

	// parseFileList: 空内容不崩
	static void testParseFileListEmpty()
	{
		Dictionary<string, GameFileInfo> list = new();
		FileUtility.parseFileList("", list);
		assertEqual(0, list.Count, "空内容无条目");
	}

	// parseFileList: tab 分隔行解析(tab 3 段: 文件名/大小/MD5)
	static void testParseFileListValid()
	{
		Dictionary<string, GameFileInfo> list = new();
		FileUtility.parseFileList("file1.txt\t123\tmd51", list);
		assertEqual(1, list.Count, "解析 1 条");
		assertTrue(list.ContainsKey("file1.txt"), "key 是文件名");
		assertEqual(123, list["file1.txt"].mFileSize, "大小解析");
		assertEqual("md51", list["file1.txt"].mMD5, "MD5 解析");
	}

	// checkDeleteFile: 标准列表(远端) vs 本地列表 → 需删除的文件
	static void testCheckDeleteFile()
	{
		Dictionary<string, GameFileInfo> standard = new()
		{
			["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 1, mMD5 = "md5a" },
			["b.txt"] = new GameFileInfo { mFileName = "b.txt", mFileSize = 1, mMD5 = "md5b" }
		};
		// 本地: a 一致(不删), c 不在远端(删), d MD5 不同(删)
		Dictionary<string, GameFileInfo> local = new()
		{
			["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 1, mMD5 = "md5a" },
			["c.txt"] = new GameFileInfo { mFileName = "c.txt", mFileSize = 1, mMD5 = "md5c" },
			["d.txt"] = new GameFileInfo { mFileName = "d.txt", mFileSize = 1, mMD5 = "other" }
		};
		List<string> deleteList = FileUtility.checkDeleteFile(standard, local);
		assertEqual(2, deleteList.Count, "删除 2 个文件");
		assertTrue(deleteList.Contains("c.txt"), "不在远端 → 删除");
		assertTrue(deleteList.Contains("d.txt"), "MD5 不同 → 删除");
		assertFalse(deleteList.Contains("a.txt"), "一致 → 不删");
	}
}
