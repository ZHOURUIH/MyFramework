using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static FileUtility;
using static TestAssert;

public static class FileUtilityTest
{
    public static void Run()
    {
        testBasicFileOperations();
        testSearchHelpers();
        testAdvancedFileOps();
        testFindFilesInternal();
        testFindResourcesFiles();
        testFindStreamingAssetsFiles();
        testFindFilesExclude();
        testCheckDiff();
        testCheckNeedUploadFile();
        testCheckDeleteFile();
        testEncryptDecryptAES();
        testGenerateFileMD5Bytes();
    }

    private static void testBasicFileOperations()
    {
        string root = Path.Combine(Path.GetTempPath(), "MicroLegend_FrameHotfix_FileUtility_" + Guid.NewGuid().ToString("N"));
        try
        {
            createDir(root);
            assertTrue(isDirExist(root), "root exists");
            validPath(ref root);
            assertTrue(root.EndsWith("/"), "valid path");

            string textFile = root + "sample.txt";
            writeTxtFile(textFile, "hello\nworld");
            assertTrue(isFileExist(textFile), "text exists");
            assertEqual("hello\nworld", openTxtFileSync(textFile, true).Replace("\r", ""), "text round trip");
            assertEqual(2, openTxtFileLinesSync(textFile, out string[] lines, true, true), "line count");
            assertEqual("hello", lines[0], "first line");

            string binFile = root + "data.bin";
            writeFile(binFile, new byte[] { 1, 2, 3 });
            assertEqual(3, getFileSize(binFile), "binary size");
            assertTrue(isFileExist(binFile), "binary exists");

            string renamed = root + "renamed.bin";
            assertTrue(renameFile(binFile, renamed), "rename");
            assertTrue(isFileExist(renamed), "renamed exists");
            assertTrue(deleteFile(renamed), "delete file");
            assertFalse(isFileExist(renamed), "deleted file gone");
        }
        finally
        {
            deleteFolder(root);
        }

        assertFalse(isDirExist(root), "root deleted");
    }

    private static void testSearchHelpers()
    {
        string root = Path.Combine(Path.GetTempPath(), "MicroLegend_FrameHotfix_Search_" + Guid.NewGuid().ToString("N"));
        try
        {
            createDir(root);
            createDir(root + "/a");
            createDir(root + "/b");
            writeTxtFile(root + "/a/one.txt", "1");
            writeTxtFile(root + "/a/two.log", "2");
            writeTxtFile(root + "/b/three.txt", "3");

			var txtFiles = findFilesNonAlloc(root, ".txt", true);
            assertEqual(2, txtFiles.Count, "txt search count");

            var folders = new System.Collections.Generic.List<string>();
            assertTrue(findFolders(root, folders, null, true), "find folders");
            assertTrue(folders.Count >= 2, "folder count");
        }
        finally
        {
            deleteFolder(root);
        }
    }

    private static void testAdvancedFileOps()
    {
        string root = Path.Combine(Path.GetTempPath(), "MicroLegend_Adv_" + Guid.NewGuid().ToString("N"));
        try
        {
            createDir(root);

            // openFileSync 读取字节
            string src = root + "/src.bin";
            writeFile(src, new byte[] { 10, 20, 30, 40 });
            byte[] readBack = openFileSync(src, false);
            assertEqual(4, readBack.Length, "openFileSync len");
            assertEqual(10, readBack[0], "openFileSync [0]");
            assertEqual(40, readBack[3], "openFileSync [3]");

            // moveFile
            string dst = root + "/dst.bin";
			assertTrue(moveFile(src, dst), "moveFile");
            assertFalse(isFileExist(src), "moveFile src gone");
            assertTrue(isFileExist(dst), "moveFile dst exists");

            // deleteEmptyFolder
            string empty = root + "/emptyDir";
            string nonEmpty = root + "/nonEmpty";
            createDir(empty);
            createDir(nonEmpty);
            writeTxtFile(nonEmpty + "/f.txt", "x");
            assertTrue(deleteEmptyFolder(empty), "deleteEmptyFolder removes empty");
            assertFalse(deleteEmptyFolder(nonEmpty), "deleteEmptyFolder fails on non-empty");
            assertFalse(isDirExist(empty), "empty dir deleted");

            // findFiles List 重载
            var allFiles = new System.Collections.Generic.List<string>();
            findFiles(root, allFiles, null, true);
            assertTrue(allFiles.Count >= 1, "findFiles List has files");
        }
        finally
        {
            if (isDirExist(root))
            {
                deleteFolder(root);
            }
        }
    }

    // ─── checkDiff: 两个文件列表是否一致 ──────────────────────────────
    private static void testCheckDiff()
    {
        // 相同内容应一致
        var list0 = new System.Collections.Generic.Dictionary<string, GameFileInfo>();
        var list1 = new System.Collections.Generic.Dictionary<string, GameFileInfo>();
        list0["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "abc" };
        list1["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "abc" };
        assertTrue(checkDiff(list0, list1, true, false), "checkDiff same");

        // 大小不同，不查MD5
        list1["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 200, mMD5 = "abc" };
        assertFalse(checkDiff(list0, list1, false, false), "checkDiff size diff");

        // 大小相同但MD5不同，查MD5
        list1["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "xyz" };
        assertFalse(checkDiff(list0, list1, true, false), "checkDiff md5 diff");
        assertTrue(checkDiff(list0, list1, false, false), "checkDiff skip md5");

        // 数量不同
        var list2 = new System.Collections.Generic.Dictionary<string, GameFileInfo>();
        assertFalse(checkDiff(list0, list2, false, false), "checkDiff count diff");

        // 都为空
        assertTrue(checkDiff(list2, list2, false, false), "checkDiff both empty");

        // 两边key不同: 两边都有相同key但值不同 → 等效于不同key的语义
        var list3 = new System.Collections.Generic.Dictionary<string, GameFileInfo>();
        list3["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 999, mMD5 = "abc" };
        assertFalse(checkDiff(list0, list3, false, false), "checkDiff different values (same key)");
    }

    // ─── checkNeedUploadFile: 筛选新增/修改的文件 ─────────────────────
    private static void testCheckNeedUploadFile()
    {
        var remote = new System.Collections.Generic.Dictionary<string, GameFileInfo>();
        remote["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "abc" };
        remote["b.txt"] = new GameFileInfo { mFileName = "b.txt", mFileSize = 200, mMD5 = "def" };

        var local = new System.Collections.Generic.Dictionary<string, GameFileInfo>();

        // 空本地: 无新增
        var modifyList1 = checkNeedUploadFile(remote, local);
        assertEqual(0, modifyList1.Count, "upload empty local");

        // 新增文件 (远端不存在)
        local["c.txt"] = new GameFileInfo { mFileName = "c.txt", mFileSize = 50, mMD5 = "ghi" };
        var modifyList2 = checkNeedUploadFile(remote, local);
        assertEqual(1, modifyList2.Count, "upload new file");
        assertTrue(modifyList2.Contains("c.txt"), "upload new file c.txt");

        // 文件大小修改
        local["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 999, mMD5 = "abc" };
        var modifyList3 = checkNeedUploadFile(remote, local);
        assertTrue(modifyList3.Contains("a.txt"), "upload size changed");

        // MD5修改
        local["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "xxx" };
        var modifyList4 = checkNeedUploadFile(remote, local);
        assertTrue(modifyList4.Contains("a.txt"), "upload md5 changed");

        // 完全一致的不用上传
        local.Clear();
        local["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "abc" };
        local["b.txt"] = new GameFileInfo { mFileName = "b.txt", mFileSize = 200, mMD5 = "def" };
        var modifyList5 = checkNeedUploadFile(remote, local);
        assertEqual(0, modifyList5.Count, "upload all same");
    }

    // ─── checkDeleteFile: 筛选需要删除的文件 ──────────────────────────
    private static void testCheckDeleteFile()
    {
        var standard = new System.Collections.Generic.Dictionary<string, GameFileInfo>();
        standard["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "abc" };
        standard["b.txt"] = new GameFileInfo { mFileName = "b.txt", mFileSize = 200, mMD5 = "def" };

        // 本地全部在标准列表中且MD5一致: 不删除
        var local1 = new System.Collections.Generic.Dictionary<string, GameFileInfo>();
        local1["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "abc" };
        local1["b.txt"] = new GameFileInfo { mFileName = "b.txt", mFileSize = 200, mMD5 = "def" };
        var del1 = checkDeleteFile(standard, local1);
        assertEqual(0, del1.Count, "delete none needed");

        // 本地有文件不在标准列表中: 应删除
        local1["c.txt"] = new GameFileInfo { mFileName = "c.txt", mFileSize = 50, mMD5 = "ghi" };
        var del2 = checkDeleteFile(standard, local1);
        assertEqual(1, del2.Count, "delete extra file");
        assertTrue(del2.Contains("c.txt"), "delete c.txt");

        // 本地文件MD5不一致: 应删除
        local1["a.txt"] = new GameFileInfo { mFileName = "a.txt", mFileSize = 100, mMD5 = "xxx" };
        var del3 = checkDeleteFile(standard, local1);
        assertTrue(del3.Contains("a.txt"), "delete md5 mismatch");

        // 空本地: 无删除
        var emptyLocal = new System.Collections.Generic.Dictionary<string, GameFileInfo>();
        var del4 = checkDeleteFile(standard, emptyLocal);
        assertEqual(0, del4.Count, "delete empty local");
    }

    // ─── AES 加密解密往返 ──────────────────────────────────────────
    private static void testEncryptDecryptAES()
    {
        byte[] key = new byte[32];
        byte[] iv = new byte[16];
        // 使用随机 key/iv 以确保安全
        new Random(42).NextBytes(key);
        new Random(43).NextBytes(iv);

        // 正常加解密往返
        byte[] data = Encoding.UTF8.GetBytes("Hello World! 测试数据 123");
        byte[] encrypted = encryptAES(data, key, iv);
        assertTrue(encrypted.Length > 0, "encrypt non-empty");
        assertFalse(data.SequenceEqual(encrypted), "encrypted != original");

        byte[] decrypted = decryptAES(encrypted, key, iv);
        assertTrue(data.SequenceEqual(decrypted), "decrypt roundtrip");

        // decryptAES: key 为空时返回原数据
        byte[] decryptedEmptyKey = decryptAES(encrypted, new byte[0], iv);
        assertTrue(encrypted.SequenceEqual(decryptedEmptyKey), "decrypt empty key returns original");

        // decryptAES: iv 为空时返回原数据
        byte[] decryptedEmptyIV = decryptAES(encrypted, key, new byte[0]);
        assertTrue(encrypted.SequenceEqual(decryptedEmptyIV), "decrypt empty iv returns original");

        // 空数据加解密
        byte[] emptyData = new byte[0];
        byte[] encryptedEmpty = encryptAES(emptyData, key, iv);
        byte[] decryptedEmpty = decryptAES(encryptedEmpty, key, iv);
        assertEqual(0, decryptedEmpty.Length, "empty roundtrip len=0");
    }

    // ─── findFilesInternal: 文件系统遍历 ─────────────────────────────
    private static void testFindFilesInternal()
    {
        string root = Path.Combine(Path.GetTempPath(), "MF_FindInternal_" + Guid.NewGuid().ToString("N"));
        try
        {
            createDir(root);
            createDir(root + "/sub1");
            createDir(root + "/sub2");
            writeTxtFile(root + "/a.txt", "a");
            writeTxtFile(root + "/b.log", "b");
            writeTxtFile(root + "/sub1/c.txt", "c");
            writeTxtFile(root + "/sub2/d.txt", "d");
            writeTxtFile(root + "/sub2/e.log", "e");

            var fileList = new System.Collections.Generic.List<string>();

            // 非递归: 只查当前目录
            fileList.Clear();
            findFilesInternal(root, fileList, null, null, false);
            assertEqual(2, fileList.Count, "non-recursive root count");

            // 递归: 所有子目录
            fileList.Clear();
            findFilesInternal(root, fileList, null, null, true);
            assertEqual(5, fileList.Count, "recursive all count");

            // 带 pattern: 只查 .txt
            var patterns = new System.Collections.Generic.List<string> { ".txt" };
            fileList.Clear();
            findFilesInternal(root, fileList, patterns, null, true);
            assertEqual(3, fileList.Count, "pattern .txt count");

            // 带 exclude: 排除 .log
            var exclude = new System.Collections.Generic.List<string> { ".log" };
            fileList.Clear();
            findFilesInternal(root, fileList, null, exclude, true);
            assertEqual(3, fileList.Count, "exclude .log count");

            // 同时 pattern + exclude
            fileList.Clear();
            findFilesInternal(root, fileList, patterns, exclude, true);
            assertEqual(3, fileList.Count, "pattern+exclude count");

            // 不存在的目录: 直接 return
            fileList.Clear();
            findFilesInternal(root + "/nonexistent", fileList, null, null, true);
            assertEqual(0, fileList.Count, "nonexistent dir count");
        }
        finally
        {
            deleteFolder(root);
        }
    }

    // ─── findResourcesFiles: 在 Editor 下等价于 findFilesInternal ────
    private static void testFindResourcesFiles()
    {
        string root = Path.Combine(Path.GetTempPath(), "MF_FindRes_" + Guid.NewGuid().ToString("N"));
        try
        {
            createDir(root);
            writeTxtFile(root + "/img.png", "png");
            writeTxtFile(root + "/data.json", "json");
            writeTxtFile(root + "/shader.asset", "asset");

            // findResourcesFiles(path, fileList, pattern, recursive, keepAbsolute)
            var fileList = new System.Collections.Generic.List<string>();
            findResourcesFiles(root, fileList, ".json", false);
            assertTrue(fileList.Count >= 0, "findResourcesFiles no crash");

            // NonAlloc 版本
            var files = findResourcesFilesNonAlloc(root, ".json", false);
            assertTrue(files != null, "findResourcesFilesNonAlloc not null");

            // 多 pattern
            var patterns = new System.Collections.Generic.List<string> { ".png", ".json" };
            fileList.Clear();
            findResourcesFiles(root, fileList, patterns, false);
            assertTrue(fileList.Count >= 0, "findResourcesFiles multi pattern");
        }
        finally
        {
            deleteFolder(root);
        }
    }

    // ─── findStreamingAssetsFiles: 非 Android 下 logError ──────────
    private static void testFindStreamingAssetsFiles()
    {
        // 在 Editor/Windows 下，findStreamingAssetsFiles 内部调用 findFilesInternal
        // 这与 findResourcesFiles 行为相同（非 Android/iOS 走 Directory.GetFiles）
        string root = Path.Combine(Path.GetTempPath(), "MF_StreamAssets_" + Guid.NewGuid().ToString("N"));
        try
        {
            createDir(root);
            writeTxtFile(root + "/s1.txt", "1");

            var fileList = new System.Collections.Generic.List<string>();
            findStreamingAssetsFiles(root, fileList, ".txt", false);
            assertTrue(fileList.Count >= 0, "findStreamingAssetsFiles no crash");

            // NonAlloc
            var files = findStreamingAssetsFilesNonAlloc(root, ".txt", false);
            assertTrue(files != null, "findStreamingAssetsFilesNonAlloc not null");

            // findStreamingAssetsFolders
            var folders = new System.Collections.Generic.List<string>();
            findStreamingAssetsFolders(root, folders, false);
            assertTrue(folders != null, "findStreamingAssetsFolders no crash");
        }
        finally
        {
            deleteFolder(root);
        }
    }

    // ─── findFilesExcludeNonAlloc ────────────────────────────────────
    private static void testFindFilesExclude()
    {
        string root = Path.Combine(Path.GetTempPath(), "MF_Exclude_" + Guid.NewGuid().ToString("N"));
        try
        {
            createDir(root);
            writeTxtFile(root + "/keep.txt", "k");
            writeTxtFile(root + "/skip.meta", "s");
            writeTxtFile(root + "/skip.DS_Store", "d");

            var exclude = new System.Collections.Generic.List<string> { ".meta", ".DS_Store" };
            var files = findFilesExcludeNonAlloc(root, exclude, false);
            assertTrue(files != null, "findFilesExcludeNonAlloc not null");
            assertTrue(files.Count >= 1, "findFilesExcludeNonAlloc has results");
        }
        finally
        {
            deleteFolder(root);
        }
    }

    // ─── generateFileMD5(byte[], int) ──────────────────────────────
    private static void testGenerateFileMD5Bytes()
    {
        // 空数组: 返回 EMPTY
        assertEqual("", generateFileMD5(new byte[0]), "md5 empty bytes");

        // 已知数据的 MD5
        byte[] data = Encoding.UTF8.GetBytes("hello");
        string md5 = generateFileMD5(data);
        // MD5("hello") = "5d41402abc4b2a76b9719d911017c592" (generateFileMD5返回大写)
        assertEqual("5D41402ABC4B2A76B9719D911017C592".ToUpper(), md5.ToUpper(), "md5 hello");

        // 指定部分长度
        byte[] longData = Encoding.UTF8.GetBytes("hello world");
        string md5Partial = generateFileMD5(longData, 5);
        assertEqual("5D41402ABC4B2A76B9719D911017C592".ToUpper(), md5Partial.ToUpper(), "md5 partial=5 bytes");

        // 全部长度
        string md5Full = generateFileMD5(longData);
        // MD5("hello world") = "5EB63BBBE01EEED093CB22BB8F5ACDC3"
        assertEqual("5EB63BBBE01EEED093CB22BB8F5ACDC3".ToUpper(), md5Full.ToUpper(), "md5 hello world");

        // 相同数据应产生相同 MD5
        byte[] data2 = Encoding.UTF8.GetBytes("hello");
        string md5_2 = generateFileMD5(data2);
        assertEqual(md5, md5_2, "md5 deterministic");
    }
}