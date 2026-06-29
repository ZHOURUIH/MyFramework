using System;
using System.IO;
using static FileUtility;
using static TestAssert;

public static class FileUtilityTest
{
    public static void Run()
    {
        testBasicFileOperations();
        testSearchHelpers();
        testAdvancedFileOps();
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
            writeAppendFile(binFile, new byte[] { 4, 5 });
            assertEqual(5, getFileSize(binFile), "binary size");
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
}