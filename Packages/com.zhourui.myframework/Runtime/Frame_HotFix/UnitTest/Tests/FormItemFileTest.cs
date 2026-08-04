using static TestAssert;

// FormItemFile 表单文件内容测试
public static class FormItemFileTest
{
    public static void Run()
    {
        testConstructor();
        testResetProperty();
    }

    static void testConstructor()
    {
        byte[] content = new byte[] { 1, 2, 3 };
        FormItemFile item = new FormItemFile(content, 3, "test.txt");
        assertEqual(content, item.mFileContent, "fileContent");
        assertEqual(3, item.mFileLength, "fileLength");
        assertEqual("test.txt", item.mFileName, "fileName");
    }

    static void testResetProperty()
    {
        byte[] content = new byte[] { 1, 2, 3 };
        FormItemFile item = new FormItemFile(content, 3, "test.txt");
        item.resetProperty();
        assertNull(item.mFileContent, "reset fileContent=null");
        assertNull(item.mFileName, "reset fileName=null");
        assertEqual(0, item.mFileLength, "reset fileLength=0");
    }
}
