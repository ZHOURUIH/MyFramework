#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static TestAssert;

public static class StreamBufferTest
{
    public static void Run()
    {
        testInit();
        testAddData();
        testAddDataWithOffset();
        testRemoveData();
        testClear();
        testMerge();
        testEmptyBuffer();
        testEdgeCases();
        testDataIntegrity();
        testMultipleOperations();
    }

    static void testInit()
    {
        StreamBuffer buffer = new StreamBuffer(64);
        assertNotNull(buffer.getData(), "getData should return non-null byte array");
        assertEqual(0, buffer.getDataLength(), "New buffer should have zero data length");
        assertEqual(64, buffer.getBufferSize(), "Buffer size should match init parameter");
        buffer.destroy();
    }

    static void testAddData()
    {
        StreamBuffer buffer = new StreamBuffer(32);
        byte[] data = { 0x01, 0x02, 0x03, 0x04, 0x05 };

        bool result = buffer.addData(data, data.Length);
        assertTrue(result, "addData should succeed");
        assertEqual(5, buffer.getDataLength(), "Data length should be 5 after adding 5 bytes");

        byte[] stored = buffer.getData();
        assertEqual(0x01, stored[0], "First byte should be 0x01");
        assertEqual(0x05, stored[4], "Fifth byte should be 0x05");
        buffer.destroy();
    }

    static void testAddDataWithOffset()
    {
        StreamBuffer buffer = new StreamBuffer(32);
        byte[] data = { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

        bool result = buffer.addData(data, 2, 3);
        assertTrue(result, "addData with offset should succeed");
        assertEqual(3, buffer.getDataLength(), "Should have added 3 bytes");

        byte[] stored = buffer.getData();
        assertEqual(0xCC, stored[0], "First stored byte should be data[2] = 0xCC");
        assertEqual(0xEE, stored[2], "Third stored byte should be data[4] = 0xEE");
        buffer.destroy();
    }

    static void testRemoveData()
    {
        StreamBuffer buffer = new StreamBuffer(32);
        byte[] data = { 0x10, 0x20, 0x30, 0x40, 0x50, 0x60 };
        buffer.addData(data, data.Length);

        bool result = buffer.removeData(2, 2);
        assertTrue(result, "removeData should succeed");
        assertEqual(4, buffer.getDataLength(), "Should have 4 bytes after removing 2");

        byte[] stored = buffer.getData();
        assertEqual(0x10, stored[0], "First byte should remain 0x10");
        assertEqual(0x50, stored[2], "After removal, stored[2] should be 0x50");
        assertEqual(0x60, stored[3], "After removal, stored[3] should be 0x60");
        buffer.destroy();
    }

    static void testClear()
    {
        StreamBuffer buffer = new StreamBuffer(16);
        byte[] data = { 1, 2, 3, 4, 5 };
        buffer.addData(data, data.Length);
        assertEqual(5, buffer.getDataLength(), "Should have 5 bytes before clear");

        buffer.clear();
        assertEqual(0, buffer.getDataLength(), "Should have 0 bytes after clear");
        buffer.destroy();
    }

    static void testMerge()
    {
        StreamBuffer buffer1 = new StreamBuffer(32);
        StreamBuffer buffer2 = new StreamBuffer(32);

        byte[] data1 = { 0xA, 0xB };
        byte[] data2 = { 0xC, 0xD, 0xE };

        buffer1.addData(data1, data1.Length);
        buffer2.addData(data2, data2.Length);

        bool result = buffer1.merge(buffer2);
        assertTrue(result, "merge should succeed");
        assertEqual(5, buffer1.getDataLength(), "Merged buffer should have 5 bytes total");

        byte[] merged = buffer1.getData();
        assertEqual(0xA, merged[0], "First byte should be from first buffer");
        assertEqual(0xB, merged[1], "Second byte should be from first buffer");
        assertEqual(0xC, merged[2], "Third byte should be from second buffer");
        assertEqual(0xE, merged[4], "Fifth byte should be last of second buffer");

        buffer1.destroy();
        buffer2.destroy();
    }

    static void testEmptyBuffer()
    {
        StreamBuffer empty = new StreamBuffer(8);

        assertEqual(0, empty.getDataLength(), "Empty buffer should have length 0");
        assertEqual(8, empty.getBufferSize(), "Empty buffer should retain buffer size");

        // Removing from empty buffer should fail
        bool result = empty.removeData(0, 1);
        assertFalse(result, "removeData on empty buffer should fail");

        empty.destroy();
    }

    static void testEdgeCases()
    {
        // Add more data than buffer size
        StreamBuffer small = new StreamBuffer(4);
        byte[] largeData = { 1, 2, 3, 4, 5 };
        bool result = small.addData(largeData, largeData.Length);
        // Behavior depends on implementation - may auto-expand or fail
        // Just verify no crash
        small.clear();
        small.destroy();

        // Remove beyond bounds
        StreamBuffer buf = new StreamBuffer(16);
        buf.addData(new byte[] { 1, 2, 3 }, 3);
        result = buf.removeData(0, 10);
        // Removing more than available should be handled gracefully
        buf.destroy();
    }

    static void testDataIntegrity()
    {
        StreamBuffer buffer = new StreamBuffer(256);
        byte[] testData = new byte[100];
        for (int i = 0; i < 100; i++)
        {
            testData[i] = (byte)(i & 0xFF);
        }

        buffer.addData(testData, testData.Length);
        byte[] stored = buffer.getData();

        for (int i = 0; i < 100; i++)
        {
            assertEqual((byte)(i & 0xFF), stored[i], "Data integrity check at index " + i);
        }

        buffer.destroy();
    }

    static void testMultipleOperations()
    {
        StreamBuffer buffer = new StreamBuffer(64);
        byte[] chunk1 = { 0x11, 0x22 };
        byte[] chunk2 = { 0x33, 0x44, 0x55 };

        buffer.addData(chunk1, chunk1.Length);
        buffer.addData(chunk2, chunk2.Length);
        assertEqual(5, buffer.getDataLength(), "Should have 5 bytes after two adds");

        buffer.removeData(1, 2);
        assertEqual(3, buffer.getDataLength(), "Should have 3 bytes after removal");

        byte[] finalData = { 0x66 };
        buffer.addData(finalData, finalData.Length);
        assertEqual(4, buffer.getDataLength(), "Should have 4 bytes after final add");

        byte[] stored = buffer.getData();
        assertEqual(0x11, stored[0], "Complex sequence should preserve data at [0]");
        assertEqual(0x55, stored[2], "Complex sequence should preserve data at [2]");
        assertEqual(0x66, stored[3], "Complex sequence should have final byte at [3]");

        buffer.clear();
        assertEqual(0, buffer.getDataLength(), "After clear, length should be 0");

        buffer.destroy();
    }
}
#endif
