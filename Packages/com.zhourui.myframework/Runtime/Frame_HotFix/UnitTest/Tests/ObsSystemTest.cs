using System.Collections.Generic;
using static TestAssert;

// ObsSystem 单元测试
// 覆盖可脱离网络的纯逻辑:
//   parseFileList (OBS 响应XML解析)
//   generatePolicySignature / generateURLSignature / generateHeaderSignature (签名生成)
//   hmacSha1 (空key处理)
//   init / downloadTxt / downloadBytes / delete / getFileList 空URL保护
//   GameFileInfo.createInfo (Frame_Base 通用文件信息解析)
public static class ObsSystemTest
{
	public static void Run()
	{
		// ─── GameFileInfo.createInfo (通用解析) ───
		testCreateInfoValid();
		testCreateInfoInvalidCount();
		testCreateInfoEmpty();
		// ─── ObsSystem 空URL保护 ───
		testEmptyURL();
		testNullRemotePath();
		// ─── parseFileList: OBS XML 解析 ───
		testParseFileListEmpty();
		testParseFileListSingleFile();
		testParseFileListMultiFile();
		testParseFileListDirectorySkipped();
		testParseFileListTruncated();
		testParseFileListNextMarker();
		testParseFileListETagQuotesRemoved();
		testParseFileListInvalidXml();
		// ─── 签名生成 ───
		testGeneratePolicySignature();
		testGenerateURLSignature();
		testGenerateHeaderSignature();
		testHmacSha1EmptyKey();
		testHmacSha1NonEmptyKey();
	}

	// ═════════════════════════════════════════════════════════════════
	// GameFileInfo.createInfo
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateInfoValid()
	{
		GameFileInfo info = GameFileInfo.createInfo("path/to/file.png\t12345\tabcdef0123456789");
		assertNotNull(info, "合法3段应解析成功");
		assertEqual("path/to/file.png", info.mFileName);
		assertEqual(12345L, info.mFileSize);
		assertEqual("abcdef0123456789", info.mMD5);
	}
	private static void testCreateInfoInvalidCount()
	{
		GameFileInfo info = GameFileInfo.createInfo("only-name");
		assertNull(info, "非3段应返回 null");
		info = GameFileInfo.createInfo("a\tb");
		assertNull(info, "2段也应返回 null");
	}
	private static void testCreateInfoEmpty()
	{
		// 空字符串 Split 后为1段, 返回 null (不传 null, 因为 Split(null) 会抛 NPE)
		GameFileInfo info = GameFileInfo.createInfo("");
		assertNull(info, "空字符串应返回 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// ObsSystem 空 URL / 空路径保护
	// ═════════════════════════════════════════════════════════════════
	private static void testEmptyURL()
	{
		ObsSystem obs = new();
		obs.init("", "bucket", "ak", "sk");
		assertEqual("", obs.downloadTxt("file.txt"), "URL为空时 downloadTxt 返回空串");
		obs.init(null, "bucket", "ak", "sk");
		assertEqual("", obs.downloadTxt("file.txt"), "URL为null时返回空串");
	}
	private static void testNullRemotePath()
	{
		ObsSystem obs = new();
		obs.init("http://host/", "bucket", "ak", "sk");
		assertEqual("", obs.downloadTxt(null), "remotePath为null返回空串");
		// delete 空路径
		assertFalse(obs.delete(null), "delete null 返回 false");
		// getFileList 空路径返回空字典
		Dictionary<string, GameFileInfo> map = obs.getFileList(null);
		assertNotNull(map, "getFileList(null) 返回非空字典对象");
		assertTrue(map.Count == 0, "getFileList(null) 字典为空");
	}

	// ═════════════════════════════════════════════════════════════════
	// parseFileList: OBS ListBucketResult XML 解析
	// ═════════════════════════════════════════════════════════════════
	private static void testParseFileListEmpty()
	{
		string xml = "<?xml version=\"1.0\"?><ListBucketResult><IsTruncated>false</IsTruncated></ListBucketResult>";
		var list = new List<GameFileInfo>();
		bool finished = ObsSystemTestHelper.callParseFileList(xml, list, out string nextMarker);
		assertTrue(finished, "无 Contents 且 IsTruncated=false 应 finished");
		assertNull(nextMarker, "无 NextMarker 应为 null");
		assertTrue(list.Count == 0, "无文件");
	}
	private static void testParseFileListSingleFile()
	{
		string xml = "<?xml version=\"1.0\"?><ListBucketResult>" +
			"<Contents><Key>res/a.png</Key><ETag>\"abc123\"</ETag><Size>1024</Size></Contents>" +
			"<IsTruncated>false</IsTruncated></ListBucketResult>";
		var list = new List<GameFileInfo>();
		bool finished = ObsSystemTestHelper.callParseFileList(xml, list, out string nextMarker);
		assertTrue(finished, "单个文件应 finished");
		assertTrue(list.Count == 1, "解析出1个文件");
		assertEqual("res/a.png", list[0].mFileName);
		assertEqual("abc123", list[0].mMD5, "ETag 引号应被移除");
		assertEqual(1024L, list[0].mFileSize);
	}
	private static void testParseFileListMultiFile()
	{
		string xml = "<?xml version=\"1.0\"?><ListBucketResult>" +
			"<Contents><Key>a.txt</Key><ETag>\"e1\"</ETag><Size>10</Size></Contents>" +
			"<Contents><Key>b.txt</Key><ETag>\"e2\"</ETag><Size>20</Size></Contents>" +
			"<IsTruncated>false</IsTruncated></ListBucketResult>";
		var list = new List<GameFileInfo>();
		ObsSystemTestHelper.callParseFileList(xml, list, out _);
		assertTrue(list.Count == 2, "解析出2个文件");
		assertEqual("a.txt", list[0].mFileName);
		assertEqual("b.txt", list[1].mFileName);
		assertEqual(20L, list[1].mFileSize);
	}
	private static void testParseFileListDirectorySkipped()
	{
		// Key 以 / 结尾的是目录, 不入列表
		string xml = "<?xml version=\"1.0\"?><ListBucketResult>" +
			"<Contents><Key>subdir/</Key><ETag>\"e1\"</ETag><Size>0</Size></Contents>" +
			"<IsTruncated>false</IsTruncated></ListBucketResult>";
		var list = new List<GameFileInfo>();
		ObsSystemTestHelper.callParseFileList(xml, list, out _);
		assertTrue(list.Count == 0, "目录项应被跳过");
	}
	private static void testParseFileListTruncated()
	{
		string xml = "<?xml version=\"1.0\"?><ListBucketResult>" +
			"<Contents><Key>a.txt</Key><ETag>\"e\"</ETag><Size>1</Size></Contents>" +
			"<IsTruncated>true</IsTruncated><NextMarker>next-token</NextMarker></ListBucketResult>";
		var list = new List<GameFileInfo>();
		bool finished = ObsSystemTestHelper.callParseFileList(xml, list, out string nextMarker);
		assertFalse(finished, "IsTruncated=true 不应 finished");
		assertEqual("next-token", nextMarker, "应解析出 NextMarker");
		assertTrue(list.Count == 1, "截断时也有1个文件");
	}
	private static void testParseFileListNextMarker()
	{
		string xml = "<?xml version=\"1.0\"?><ListBucketResult><IsTruncated>true</IsTruncated>" +
			"<NextMarker>marker-xyz</NextMarker></ListBucketResult>";
		var list = new List<GameFileInfo>();
		ObsSystemTestHelper.callParseFileList(xml, list, out string nextMarker);
		assertEqual("marker-xyz", nextMarker, "纯 NextMarker 解析");
	}
	private static void testParseFileListETagQuotesRemoved()
	{
		string xml = "<?xml version=\"1.0\"?><ListBucketResult>" +
			"<Contents><Key>f.bin</Key><ETag>\"\"quoted\"\"</ETag><Size>5</Size></Contents>" +
			"<IsTruncated>false</IsTruncated></ListBucketResult>";
		var list = new List<GameFileInfo>();
		ObsSystemTestHelper.callParseFileList(xml, list, out _);
		assertTrue(list.Count == 1, "应解析1个");
		assertEqual("quoted", list[0].mMD5, "所有引号都应被移除");
	}
	private static void testParseFileListInvalidXml()
	{
		// 最小合法XML, 无Contents/IsTruncated, 不抛异常且无文件
		var list = new List<GameFileInfo>();
		bool finished = ObsSystemTestHelper.callParseFileList("<ListBucketResult/>", list, out string marker);
		assertTrue(list.Count == 0, "无Contents则无文件");
		// IsTruncated 缺失时 fetchFinish 保持默认 false, 返回 false
		assertFalse(finished, "无IsTruncated视为未取完(fetchFinish=false)");
		assertNull(marker, "无NextMarker为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// 签名生成 (确定性, 可脱离网络验证)
	// ═════════════════════════════════════════════════════════════════
	private static void testGeneratePolicySignature()
	{
		// 固定过期时间保证确定性: 相同输入产生相同签名, 且返回 base64 非空
		System.DateTime exp = new(2026, 1, 1, 12, 0, 0, System.DateTimeKind.Utc);
		string p0 = ObsSystemTestHelper.callGeneratePolicySignature("mybucket", "mysecret", "dir/file.txt", "public-read", out string b0, exp);
		string p1 = ObsSystemTestHelper.callGeneratePolicySignature("mybucket", "mysecret", "dir/file.txt", "public-read", out string b1, exp);
		assertTrue(!string.IsNullOrEmpty(p0), "签名非空");
		assertTrue(!string.IsNullOrEmpty(b0), "policyBase64 非空");
		assertEqual(p0, p1, "相同输入签名应一致");
		// 不同 key 签名不同
		string p2 = ObsSystemTestHelper.callGeneratePolicySignature("mybucket", "othersecret", "dir/file.txt", "public-read", out _, exp);
		assertTrue(p0 != p2, "不同 secureKey 签名不同");
		// 不同过期时间签名不同
		string p3 = ObsSystemTestHelper.callGeneratePolicySignature("mybucket", "mysecret", "dir/file.txt", "public-read", out _, new System.DateTime(2026, 2, 1, 12, 0, 0, System.DateTimeKind.Utc));
		assertTrue(p0 != p3, "不同过期时间签名不同");
	}
	private static void testGenerateURLSignature()
	{
		// 固定过期时间保证确定性: 相同输入产生相同签名, 且含 expires 输出
		System.DateTime exp = new(2026, 1, 1, 12, 0, 0, System.DateTimeKind.Utc);
		string sig0 = ObsSystemTestHelper.callGenerateURLSignature("sk", "DELETE", null, "application/x-www-form-urlencoded", out string exp0, "bucket", "path/f", exp);
		string sig1 = ObsSystemTestHelper.callGenerateURLSignature("sk", "DELETE", null, "application/x-www-form-urlencoded", out string exp1, "bucket", "path/f", exp);
		assertTrue(!string.IsNullOrEmpty(sig0), "URL 签名非空");
		assertTrue(!string.IsNullOrEmpty(exp0), "expires 非空");
		assertEqual(sig0, sig1, "相同输入 URL 签名一致");
		// 不同 verb 不同签名
		string sigGet = ObsSystemTestHelper.callGenerateURLSignature("sk", "GET", null, "application/x-www-form-urlencoded", out _, "bucket", "path/f", exp);
		assertTrue(sig0 != sigGet, "不同 verb 签名不同");
		// 不同过期时间签名不同
		string sig2 = ObsSystemTestHelper.callGenerateURLSignature("sk", "DELETE", null, "application/x-www-form-urlencoded", out _, "bucket", "path/f", new System.DateTime(2026, 2, 1, 12, 0, 0, System.DateTimeKind.Utc));
		assertTrue(sig0 != sig2, "不同过期时间签名不同");
	}
	private static void testGenerateHeaderSignature()
	{
		System.DateTime dt = new(2026, 1, 1, 12, 0, 0, System.DateTimeKind.Utc);
		string sig0 = ObsSystemTestHelper.callGenerateHeaderSignature("sk", "PUT", "", "text/plain", dt, "bucket", "file");
		string sig1 = ObsSystemTestHelper.callGenerateHeaderSignature("sk", "PUT", "", "text/plain", dt, "bucket", "file");
		assertTrue(!string.IsNullOrEmpty(sig0), "Header 签名非空");
		assertEqual(sig0, sig1, "相同输入 Header 签名一致");
		// 带 contentMD5 时签名不同
		string sigMd5 = ObsSystemTestHelper.callGenerateHeaderSignature("sk", "PUT", "deadbeef", "text/plain", dt, "bucket", "file");
		assertTrue(sig0 != sigMd5, "contentMD5 参与签名");
	}
	private static void testHmacSha1EmptyKey()
	{
		byte[] result = ObsSystemTestHelper.callHmacSha1("", "data");
		assertNull(result, "空 key 返回 null");
	}
	private static void testHmacSha1NonEmptyKey()
	{
		byte[] r0 = ObsSystemTestHelper.callHmacSha1("key", "message");
		byte[] r1 = ObsSystemTestHelper.callHmacSha1("key", "message");
		assertNotNull(r0, "非空 key 返回字节数组");
		assertEqual(20, r0.Length, "HMAC-SHA1 输出20字节");
		for (int i = 0; i < r0.Length; ++i)
		{
			assertEqual(r0[i], r1[i], "相同输入 HMAC 结果一致, 索引" + i);
		}
	}
}

// 通过子类暴露 ObsSystem 的 protected 方法, 以便单测
public class ObsSystemTestHelper : ObsSystem
{
	public static bool callParseFileList(string str, List<GameFileInfo> fileList, out string nextMarker)
	{
		return parseFileList(str, fileList, out nextMarker);
	}
	public static string callGeneratePolicySignature(string bucket, string secureKey, string savePath, string acl, out string policyBase64, System.DateTime? expiration = null)
	{
		return generatePolicySignature(bucket, secureKey, savePath, acl, out policyBase64, expiration);
	}
	public static string callGenerateURLSignature(string secureKey, string verb, string contentMD5_16, string contentType, out string expires, string bucket, string file, System.DateTime? expiration = null)
	{
		return generateURLSignature(secureKey, verb, contentMD5_16, contentType, out expires, bucket, file, expiration);
	}
	public static string callGenerateHeaderSignature(string secureKey, string verb, string contentMD5_16, string contentType, System.DateTime date, string bucket, string file)
	{
		return generateHeaderSignature(secureKey, verb, contentMD5_16, contentType, date, bucket, file);
	}
	public static byte[] callHmacSha1(string key, string toSign)
	{
		return hmacSha1(key, toSign);
	}
}
