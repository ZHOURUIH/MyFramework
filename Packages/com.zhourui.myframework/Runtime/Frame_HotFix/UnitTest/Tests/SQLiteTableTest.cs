using static TestAssert;

// SQLiteTable 中可脱离数据库实例测试的静态方法
public static class SQLiteTableTest
{
    public static void Run()
    {
        testGetDecryptFilePath();
    }

    // getDecryptFilePath: 返回解密文件所在目录路径(编辑器/Windows 下为临时缓存路径)
    static void testGetDecryptFilePath()
    {
#if USE_SQLITE
        string path = SQLiteTable.getDecryptFilePath();
        assertTrue(path != null, "getDecryptFilePath not null");
        assertTrue(path.Length > 0, "getDecryptFilePath not empty");
        // 路径以斜杠结尾
        assertTrue(path.EndsWith("/") || path.EndsWith("\\"), "getDecryptFilePath ends with separator");
#endif
    }
}
