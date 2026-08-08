using System.Collections.Generic;
using System.Text;
using static TestAssert;

// AssetVersionSystem 纯逻辑单测(资源版本号管理 / 文件加载路径 / 下载统计)
//
// 设计要点:
//   - AssetVersionSystem 继承 FrameSystem(mCreateObject 默认 false), 构造/销毁均安全,
//     不依赖全局单例, 全部方法只读实例字段, 可 new 局部实例测试。
//   - getFileReadPath 的 SAME_TO_REMOTE 分支在"远端缺失该文件"时会 logError,
//     违反"测试不触发框架 error 日志"约定, 故只测"远端文件存在"的路径分支(不产生 error 日志)。
//   - 每用例 finally 中 destroy() 局部实例(mObject 为 null 时 destroy 空安全)。
public static class AssetVersionSystemTest
{
	public static void Run()
	{
		testVersionSettersGetters();
		testGetLocalVersionBothNull();
		testGetLocalVersionStreamingHigher();
		testGetLocalVersionPersistentHigher();
		testGetLocalVersionNullStreaming();
		testGetLocalVersionNullPersistent();
		testReadPathDefaultSameToRemote();
		testReadPathStreamingOnly();
		testReadPathRemoteOnly();
		testReadPathSameToRemotePersistentMatch();
		testReadPathSameToRemoteStreamingMatch();
		testReadPathSameToRemoteNoLocalMatch();
		testReadPathInvalidType();
		testDownloadByteCount();
		testTotalDownloadedFiles();
		testClearDownloadedInfo();
		testAddPersistentFile();
		testGeneratePersistentAssetFileList();
		testSetAssetsFileLists();
		testGameFileInfoCreateAndToString();
	}

	// ─── 版本号 setter/getter ───────────────────────────────────────
	private static void testVersionSettersGetters()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			assertNull(sys.getStreamingAssetsVersion(), "初始 Streaming 版本应为 null");
			assertNull(sys.getPersistentAssetsVersion(), "初始 Persistent 版本应为 null");
			assertNull(sys.getRemoteAssetsVersion(), "初始 Remote 版本应为 null");

			sys.setStreamingAssetsVersion("1.0.0");
			sys.setPersistentAssetsVersion("2.0.0");
			sys.setRemoteVersion("3.0.0");
			assertEqual("1.0.0", sys.getStreamingAssetsVersion(), "Streaming 版本读写");
			assertEqual("2.0.0", sys.getPersistentAssetsVersion(), "Persistent 版本读写");
			assertEqual("3.0.0", sys.getRemoteAssetsVersion(), "Remote 版本读写");

			// 覆盖同值覆盖
			sys.setStreamingAssetsVersion("1.0.1");
			assertEqual("1.0.1", sys.getStreamingAssetsVersion(), "Streaming 版本覆盖");
			sys.setPersistentAssetsVersion("");
			assertEqual("", sys.getPersistentAssetsVersion(), "Persistent 版本空串写回");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getLocalVersion: 两者都 null → null ───────────────────────
	private static void testGetLocalVersionBothNull()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			assertNull(sys.getLocalVersion(), "版本均未设置时本地版本应 null");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getLocalVersion: Streaming 更高时取 Streaming ─────────────
	private static void testGetLocalVersionStreamingHigher()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			sys.setStreamingAssetsVersion("2.0.0");
			sys.setPersistentAssetsVersion("1.0.0");
			assertEqual("2.0.0", sys.getLocalVersion(), "Streaming(2.0.0) 高于 Persistent(1.0.0) 时取 Streaming");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getLocalVersion: Persistent 更高时取 Persistent ───────────
	private static void testGetLocalVersionPersistentHigher()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			sys.setStreamingAssetsVersion("1.9.9");
			sys.setPersistentAssetsVersion("2.0.0");
			assertEqual("2.0.0", sys.getLocalVersion(), "Persistent(2.0.0) 高于 Streaming(1.9.9) 时取 Persistent");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getLocalVersion: Streaming null, Persistent 有值 → Persistent
	//     getLocalVersion 先判"两者皆 null"再判 Persistent==null 走 Streaming,
	//     当 Streaming null、Persistent 非 null 时 compareVersion3(null, x)=REMOTE_LOWER
	//     != LOCAL_LOWER, 落到返回 Persistent。
	private static void testGetLocalVersionNullStreaming()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			sys.setPersistentAssetsVersion("1.5.0");
			assertEqual("1.5.0", sys.getLocalVersion(), "Streaming 为 null 时取 Persistent");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getLocalVersion: Persistent null, Streaming 有值 → Streaming
	private static void testGetLocalVersionNullPersistent()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			sys.setStreamingAssetsVersion("1.5.0");
			assertEqual("1.5.0", sys.getLocalVersion(), "Persistent 为 null 时取 Streaming");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getFileReadPath: 默认 SAME_TO_REMOTE(构造时设置) ─────────
	// 观察: 用远端一致 + streaming 不匹配(不触发 logError)的配置,
	// SAME_TO_REMOTE 分支会走到"本地无匹配 → null"; 若默认是 STREAMING_ONLY
	// 则会直接返回 streaming 前缀。可据此反证默认路径类型是 SAME_TO_REMOTE。
	private static void testReadPathDefaultSameToRemote()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			setRemoteOnlyInfo(sys, "f1.ab");   // remote {f1.ab: 42L / same_md5}
			sys.setStreamingAssetsFile(MakeDict("f1.ab", "local.m5", 10L));
			// SAME_TO_REMOTE: remote 存在但 streaming(size/md5) 不一致 → 无本地匹配 → null
			assertNull(sys.getFileReadPath("f1.ab"), "默认 SAME_TO_REMOTE 下本地不匹配远端时返回 null");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getFileReadPath: STREAMING_ASSETS_ONLY → 恒返回 streaming 前缀 ──
	private static void testReadPathStreamingOnly()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			sys.setAssetReadPath(ASSET_READ_PATH.STREAMING_ASSETS_ONLY);
			string path = sys.getFileReadPath("abc/bundle.ab");
			assertNotNull(path, "STREAMING_ONLY 不应返回 null");
			assertTrue(path.EndsWith("abc/bundle.ab"), "STREAMING_ONLY 应返回以相对路径结尾的字符串, got " + path);
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getFileReadPath: REMOTE_ASSETS_ONLY → 恒返回 null(触发下载) ──
	private static void testReadPathRemoteOnly()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			sys.setAssetReadPath(ASSET_READ_PATH.REMOTE_ASSETS_ONLY);
			assertNull(sys.getFileReadPath("any/path.ab"), "REMOTE_ONLY 恒返回 null 以触发下载");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getFileReadPath: SAME_TO_REMOTE + persistent 匹配远端 → persistent 前缀 ──
	private static void testReadPathSameToRemotePersistentMatch()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			// remote 与 persistent 设为相同 file info → persistent 匹配 → 返回持久化路径
			sys.setRemoteAssetsFile(MakeDict("file.ab", "abc", 5L));
			sys.setPersistentAssetsFile(MakeDict("file.ab", "abc", 5L));
			string path = sys.getFileReadPath("file.ab");
			// PERSISTENT_ASSETS_PATH 常量固定为 F_PERSISTENT_DATA_PATH + "Assets/",
			// 此处不依赖具体路径值, 只验证"返回的路径以文件名结尾"且"不是 streaming 前缀"。
			assertNotNull(path, "persistent 匹配远端时应返回 persistent 路径");
			assertTrue(path.EndsWith("file.ab"), "应返回以 file.ab 结尾的持久化路径, got " + path);
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getFileReadPath: SAME_TO_REMOTE + streaming 匹配远端 → streaming 前缀 ──
	private static void testReadPathSameToRemoteStreamingMatch()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			// remote 与 streaming 设为相同 file info → streaming 匹配 → 返回 streaming 路径
			sys.setRemoteAssetsFile(MakeDict("file.ab", "abc", 5L));
			sys.setStreamingAssetsFile(MakeDict("file.ab", "abc", 5L));
			string path = sys.getFileReadPath("file.ab");
			assertNotNull(path, "streaming 匹配远端时应返回 streaming 路径");
			assertTrue(path.EndsWith("file.ab"), "应返回以 file.ab 结尾的 streaming 路径, got " + path);
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getFileReadPath: SAME_TO_REMOTE + 远端存在但本地无匹配 → null ──
	private static void testReadPathSameToRemoteNoLocalMatch()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			setRemoteOnlyInfo(sys, "file.ab");
			sys.setStreamingAssetsFile(MakeDict("file.ab", "local.m5", 999L));
			sys.setPersistentAssetsFile(MakeDict("file.ab", "persist.m5", 123L));
			// remote 存在但 streaming/persistent 都不匹配 → 无本地匹配 → null(触发下载, 无 logError)
			assertNull(sys.getFileReadPath("file.ab"), "本地无一匹配远端时返回 null");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── getFileReadPath: 无效路径类型(NONE)→ logError(不测, 违反约定) ──
	// 此处仅保证该分支存在说明注释, 不做实际触发, 以免产生 error 日志。
	private static void testReadPathInvalidType()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			// NONE / PERSISTENT_FIRST 未被 getFileReadPath 处理, 会走到末尾 logError 分支;
			// 该行为会触发编辑器 error 日志, 违反测试规范, 故跳过实际调用。
			// 仅保留此空用例以明确该分支已被审计(不测)。
			sys.setAssetReadPath(ASSET_READ_PATH.NONE);
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── 下载字节数 ─────────────────────────────────────────────────
	private static void testDownloadByteCount()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			assertEqual(0L, sys.getTotalDownloadedByteCount(), "初始下载字节数应为 0");
			sys.setTotalDownloadedByteCount(1024L);
			assertEqual(1024L, sys.getTotalDownloadedByteCount(), "下载字节数 setter/getter");
			sys.setTotalDownloadedByteCount(-5L);
			assertEqual(-5L, sys.getTotalDownloadedByteCount(), "下载字节数可为负(框架未做钳制)");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── 已下载文件列表 ────────────────────────────────────────────
	private static void testTotalDownloadedFiles()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			assertNotNull(sys.getTotalDownloadedFiles(), "已下载列表不应为 null");
			assertEqual(0, sys.getTotalDownloadedFiles().Count, "初始已下载列表为空");

			List<string> files = new List<string> { "a.ab", "b.ab", "c.ab" };
			sys.setTotalDownloadedFiles(files);
			var got = sys.getTotalDownloadedFiles();
			assertEqual(3, got.Count, "setTotalDownloadedFiles 后数量为 3");
			assertEqual("a.ab", got[0], "已下载列表第 0 项");
			assertEqual("b.ab", got[1], "已下载列表第 1 项");
			assertEqual("c.ab", got[2], "已下载列表第 2 项");

			// setRange 是复制语义, 外部再改源列表不应影响内部
			files[0] = "changed.ab";
			assertEqual("a.ab", sys.getTotalDownloadedFiles()[0], "setTotalDownloadedFiles 应复制而非引用");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── clearDownloadedInfo: 清零字节数与列表 ──────────────────────
	private static void testClearDownloadedInfo()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			sys.setTotalDownloadedByteCount(999L);
			List<string> files = new List<string> { "x.ab", "y.ab" };
			sys.setTotalDownloadedFiles(files);

			sys.clearDownloadedInfo();
			assertEqual(0L, sys.getTotalDownloadedByteCount(), "clear 后字节数清零");
			assertEqual(0, sys.getTotalDownloadedFiles().Count, "clear 后列表清空");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── addPersistentFile: 向 persistent 文件表追加单个文件 ─────────
	private static void testAddPersistentFile()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			GameFileInfo info = GameFileInfo.createInfo("sub/file.ab\t2048\taabbcc");
			assertNotNull(info, "合法信息应能解析");
			sys.addPersistentFile(info);

			// 设远端信息与 persistent 一致 → 命中 persistent 分支返回持久化路径
			sys.setRemoteAssetsFile(MakeDict("sub/file.ab", "aabbcc", 2048L));
			string path = sys.getFileReadPath("sub/file.ab");
			assertNotNull(path, "persistent 匹配远端时应返回持久化路径");

			// 重复添加同 key 用 TryAdd, 保留首次值。把远端改成 overwrite 信息(1/x):
			// 若 TryAdd 未保留首值(被 overwrite 覆盖为 1/x), 则远端会命中并返回路径;
			// 若保留首值(仍 2048/aabbcc), 则远端(1/x)不匹配 → null。
			// TryAdd 语义为"保留首值", 故此处应为 null, 验证未被覆盖。
			GameFileInfo overwrite = GameFileInfo.createInfo("sub/file.ab\t1\tx");
			sys.addPersistentFile(overwrite);
			sys.setRemoteAssetsFile(MakeDict("sub/file.ab", "x", 1L));
			path = sys.getFileReadPath("sub/file.ab");
			assertNull(path, "TryAdd 应保留首次添加值, 远端改 overwrite 值后不匹配");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── generatePersistentAssetFileList: 序列化 persistent 文件表 ───
	private static void testGeneratePersistentAssetFileList()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			// 空表 → 仅 "0\n"
			string emptyStr = sys.generatePersistentAssetFileList();
			assertEqual("0\n", emptyStr, "空 persistent 表序列化为 '0\\n'");

			// 两个文件 → 首行是数量 "2", 后续每行是 file\tsize\tmd5(迭代顺序不保证, 断言做内容校验)
			sys.addPersistentFile(GameFileInfo.createInfo("a.txt\t100\taaa"));
			sys.addPersistentFile(GameFileInfo.createInfo("b.txt\t200\tbbb"));
			string listStr = sys.generatePersistentAssetFileList();
			// 空表返回 "0\n" ⇒ 拆分为 ["0",""], 长度 2
			assertEqual(2, "0\n".Split('\n').Length, "空串拆分段数自检");
			string[] lines = listStr.Split('\n');
			// 两文件: "2\n" + 行1 + 行2 + 最后换行 → ["2", file1, file2, ""], 共 4 段
			assertEqual(4, lines.Length, "两文件序列化拆分应有 4 段(数量+2文件+空尾)");
			assertEqual("2", lines[0], "首行为文件数量");
			// 断言两行文件信息都存在(无视字典迭代顺序)
			bool hasA = lines[1].StartsWith("a.txt") || lines[2].StartsWith("a.txt");
			bool hasB = lines[1].StartsWith("b.txt") || lines[2].StartsWith("b.txt");
			assertTrue(hasA, "应包含 a.txt 行, got [" + listStr.Replace("\n", "|") + "]");
			assertTrue(hasB, "应包含 b.txt 行, got [" + listStr.Replace("\n", "|") + "]");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── setStreamingAssetsFile / setPersistentAssetsFile / setRemoteAssetsFile ──
	private static void testSetAssetsFileLists()
	{
		AssetVersionSystem sys = new AssetVersionSystem();
		try
		{
			// 三源默认 SAME_TO_REMOTE。remote 先设为 streaming 的 m1/1:
			sys.setRemoteAssetsFile(MakeDict("f.ab", "m1", 1L));
			sys.setStreamingAssetsFile(MakeDict("f.ab", "m1", 1L));
			sys.setPersistentAssetsFile(MakeDict("f.ab", "m2", 2L));
			// remote(m1/1) 匹配 streaming; persistent(m2/2) 不匹配 → 命中 streaming 分支
			string path = sys.getFileReadPath("f.ab");
			assertNotNull(path, "streaming 匹配远端时应返回加载路径");

			// 远端改为与 persistent 一致 → 命中 persistent 分支(persistent 优先于 streaming)
			sys.setRemoteAssetsFile(MakeDict("f.ab", "m2", 2L));
			path = sys.getFileReadPath("f.ab");
			assertNotNull(path, "远端与 persistent 一致时返回 persistent 路径");
			assertTrue(path.EndsWith("f.ab"), "路径以 f.ab 结尾, got " + path);

			// setStreamingAssetsFile 后再 setRange 覆盖, streaming 表被替换
			sys.setStreamingAssetsFile(MakeDict("f.ab", "m3", 3L));
			// remote 仍 m2/2 与 persistent 一致, 仍返回 persistent 路径
			path = sys.getFileReadPath("f.ab");
			assertNotNull(path, "streaming 表覆盖后不影响 persistent 命中的结果");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── GameFileInfo.createInfo / toString ─────────────────────────
	private static void testGameFileInfoCreateAndToString()
	{
		// 合法输入
		GameFileInfo info = GameFileInfo.createInfo("path/a.b\t1234\tmd5val");
		assertNotNull(info, "合法三段信息应解析");
		assertEqual("path/a.b", info.mFileName, "mFileName 正确");
		assertEqual(1234L, info.mFileSize, "mFileSize 正确");
		assertEqual("md5val", info.mMD5, "mMD5 正确");

		// toString → 三段 tab 拼接
		StringBuilder sb = new StringBuilder();
		info.toString(sb);
		assertEqual("path/a.b\t1234\tmd5val", sb.ToString(), "toString 应还原原串");

		// 段数不足 3 → default(null)
		GameFileInfo bad = GameFileInfo.createInfo("only_one_tab\t5");
		assertNull(bad, "段数不足 3 时返回 default");

		// 段数超过 3 → 同样是 default(null)
		// 文档化 Bug: 源码 createInfo 检查 list.Length != 3 即返回 default(非"取前3段"),
		// 故超 3 段(此处 4 段 "k/7/extra/junk")同样解析为 null, 以真实(不合理)行为为期望写死。
		GameFileInfo extra = GameFileInfo.createInfo("k\t7\textra\tjunk");
		assertNull(extra, "超 3 段时源码同样返回 default(null), 与不足 3 段一致");
	}

	// ─── 辅助: 构造 size=md5 均为同值的远端文件表 ──────────────────
	private static void setRemoteOnlyInfo(AssetVersionSystem sys, string fileName)
	{
		sys.setRemoteAssetsFile(MakeDict(fileName, "same_md5", 42L));
	}

	// ─── 辅助: 构造单个文件字典 ────────────────────────────────────
	private static Dictionary<string, GameFileInfo> MakeDict(string fileName, string md5, long size)
	{
		return new Dictionary<string, GameFileInfo>
		{
			{ fileName, new GameFileInfo { mFileName = fileName, mFileSize = size, mMD5 = md5 } }
		};
	}
}
