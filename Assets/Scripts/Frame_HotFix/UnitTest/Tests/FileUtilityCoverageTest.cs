#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using static FileUtility;
using static TestAssert;

public static class FileUtilityCoverageTest
{
    public static void Run()
    {
        testBasicFileOperations();
        testSearchHelpers();
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
}
#endif
