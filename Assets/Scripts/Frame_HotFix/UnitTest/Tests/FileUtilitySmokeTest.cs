#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using static FileUtility;
using static TestAssert;

// FileUtility smoke tests for basic local filesystem operations.
public static class FileUtilitySmokeTest
{
	public static void Run()
	{
		testWriteReadAndDelete();
	}

	private static void testWriteReadAndDelete()
	{
		string tempRoot = Path.GetTempPath().Replace('\\', '/').TrimEnd('/') + "/MicroLegend_FrameHotFix_FileUtilityTest_" + Guid.NewGuid().ToString("N");
		try
		{
			createDir(tempRoot);
			assertTrue(isDirExist(tempRoot), "createDir should create the root directory");

			string textFile = tempRoot + "/sample.txt";
			writeTxtFile(textFile, "hello world");
			assertTrue(isFileExist(textFile), "writeTxtFile should create a file");
			assertEqual("hello world", openTxtFileSync(textFile, true), "openTxtFileSync should read back the text");

			int lineCount = openTxtFileLinesSync(textFile, out string[] lines, true, true);
			assertEqual(1, lineCount, "openTxtFileLinesSync should return one line");
			assertEqual(1, lines.Length, "openTxtFileLinesSync should split into one line");
			assertEqual("hello world", lines[0], "openTxtFileLinesSync should preserve content");
			assertEqual(11, getFileSize(textFile), "getFileSize should match the written bytes");

			string nestedDir = tempRoot + "/nested";
			createDir(nestedDir);
			assertTrue(isDirExist(nestedDir), "createDir should create nested directory");

			string nestedFile = nestedDir + "/child.txt";
			writeFile(nestedFile, new byte[] { 1, 2, 3 });
			assertTrue(isFileExist(nestedFile), "writeFile should create nested file");
			assertEqual(3, getFileSize(nestedFile), "getFileSize should report binary file size");

			deleteFile(textFile);
			assertFalse(isFileExist(textFile), "deleteFile should remove the text file");
		}
		finally
		{
			deleteFolder(tempRoot);
		}

		assertFalse(isDirExist(tempRoot), "deleteFolder should remove the temp root");
	}
}
#endif
