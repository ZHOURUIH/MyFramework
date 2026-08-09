using static TestAssert;

// WavSound.generateMixPCMData 纯逻辑测试
// ═══════════════════════════════════════════════════════════════════════════════
// WavRecorder 模块(0% 覆盖)中唯一可脱离真实音频设备/文件测试的纯逻辑部分:
//   WavSound 的两个 public static generateMixPCMData 重载(PCM 混合算法):
//     - byte[]   版本: 从原始字节流(小端)逐采样解析并混合声道
//     - short[]  版本: 直接对 short 采样数据做声道混合
//   channelCount==1 直接拷贝/取字节, channelCount==2 取左右声道平均值。
//
// 说明:
//   - 方法为 static, 不依赖真实 wav 文件/麦克风/平台设备, 完全可脱离实例测试。
//   - WavSound 继承 ClassObject, 但本测试只调静态方法, 无需实例化。
//   - 不测 readFile/startWaveStream/endWaveStream/pushWaveStream(依赖真实文件/
//     WaveFormatEx 头/SerializerWrite), 遵守"不触发 error 日志"约定。
//   - 字节序依据: bytesToShort(byte0, byte1) = (byte1<<8)|byte0 (小端, 低字节在前)。
public static class WavSoundTest
{
    public static void Run()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
		testGenerateMixPCMData_Mono_ByteArray();
        testGenerateMixPCMData_Stereo_ByteArray();
        testGenerateMixPCMData_ByteArray_OddShort();
        testGenerateMixPCMData_Mono_ShortArray();
        testGenerateMixPCMData_Stereo_ShortArray();
        testGenerateMixPCMData_ShortArray_BufferSizeClamp();
        testGenerateMixPCMData_Stereo_ShortArray_NoBufferSizeUse();
        testGenerateMixPCMData_Empty();
		testWaveStreamMono();
		testWaveStreamStereo();
		testResetPropertyAfterStream();
#endif
    }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
	// ═══════════════════════════════════════════════════════════════════
	// byte[] 版本
	// ═══════════════════════════════════════════════════════════════════

	// 单声道: mix[i] = bytesToShort(data[2i], data[2i+1]) (每 2 字节一个短整型)
	private static void testGenerateMixPCMData_Mono_ByteArray()
    {
        byte[] data = { 0x34, 0x12, 0x78, 0x56 }; // 2 个采样
        short[] mix = new short[2];
        WavSound.generateMixPCMData(mix, 2, 1, data);
        assertEqual((short)0x1234, mix[0], "单声道 mix[0] = bytesToShort(0x34,0x12) = 0x1234");
        assertEqual((short)0x5678, mix[1], "单声道 mix[1] = bytesToShort(0x78,0x56) = 0x5678");
    }

    // 双声道: mix[i] = (short)((left + right) * 0.5f)
    //   left  = bytesToShort(data[4i],   data[4i+1])
    //   right = bytesToShort(data[4i+2], data[4i+3])
    private static void testGenerateMixPCMData_Stereo_ByteArray()
    {
        byte[] data =
        {
            0x34, 0x12, 0x78, 0x56,  // 采样0: left=0x1234=4660, right=0x5678=22136
            0x01, 0x00, 0x02, 0x00   // 采样1: left=1,           right=2
        };
        short[] mix = new short[2];
        WavSound.generateMixPCMData(mix, 2, 2, data);
        // (4660 + 22136) * 0.5f = 13398
        assertEqual((short)13398, mix[0], "双声道 mix[0] = (4660+22136)*0.5 = 13398");
        // (1 + 2) * 0.5f = 1.5f → (short) 截断为 1
        assertEqual((short)1, mix[1], "双声道 mix[1] = (1+2)*0.5 = 1.5 截断为 1");
    }

    // 双声道且奇数采样值: (short)((负数+正数)*0.5f) 截断语义
    private static void testGenerateMixPCMData_ByteArray_OddShort()
    {
        byte[] data =
        {
            0x00, 0x80, 0x00, 0x00,  // left = 0x8000 = -32768, right = 0
            0x02, 0x00, 0x04, 0x00   // left = 2,              right = 4
        };
        short[] mix = new short[2];
        WavSound.generateMixPCMData(mix, 2, 2, data);
        // (-32768 + 0) * 0.5f = -16384.0
        assertEqual((short)(-16384), mix[0], "双声道负数混合 = -32768*0.5 = -16384");
        // (2 + 4) * 0.5f = 3.0
        assertEqual((short)3, mix[1], "双声道 (2+4)*0.5 = 3");
    }

    // ═══════════════════════════════════════════════════════════════════
    // short[] 版本
    // ═══════════════════════════════════════════════════════════════════

    // 单声道: memcpy(mix, dataBuffer, 0, 0, min(bufferSize, mixDataCount) * 2) 字节
    //   即拷贝前 min(bufferSize, mixDataCount) 个 short 到 mix
    private static void testGenerateMixPCMData_Mono_ShortArray()
    {
        short[] data = { 100, 200, -50, 300 };
        short[] mix = new short[2];
        WavSound.generateMixPCMData(mix, 2, 1, data, 4);
        // min(4, 2) * 2 字节 = 4 字节 = 前 2 个 short
        assertEqual((short)100, mix[0], "单声道 short[] 拷贝 data[0] = 100");
        assertEqual((short)200, mix[1], "单声道 short[] 拷贝 data[1] = 200");
    }

    // bufferSize < mixDataCount 时, 只拷贝前 bufferSize 个, 其余保持默认 0
    private static void testGenerateMixPCMData_ShortArray_BufferSizeClamp()
    {
        short[] data = { 100, 200, 300, 400 };
        short[] mix = new short[4];
        WavSound.generateMixPCMData(mix, 4, 1, data, 1);
        // min(1, 4) * 2 字节 = 2 字节 = 仅 1 个 short
        assertEqual((short)100, mix[0], "仅拷贝 bufferSize=1 个 short");
        assertEqual((short)0, mix[1], "超出 bufferSize 部分保持 0");
        assertEqual((short)0, mix[2], "超出 bufferSize 部分保持 0");
        assertEqual((short)0, mix[3], "超出 bufferSize 部分保持 0");
    }

    // 双声道: mix[i] = (short)((dataBuffer[2i] + dataBuffer[2i+1]) * 0.5f)
    //   (此重载双声道不使用 bufferSize, 直接按 mixDataCount 遍历)
    private static void testGenerateMixPCMData_Stereo_ShortArray()
    {
        short[] data = { 100, 300, -40, 60, 1000, 2000 };
        short[] mix = new short[2];
        WavSound.generateMixPCMData(mix, 2, 2, data, 6);
        assertEqual((short)200, mix[0], "双声道 (100+300)*0.5 = 200");
        assertEqual((short)10, mix[1], "双声道 (-40+60)*0.5 = 10");
    }

    // 双声道 short[] 中 bufferSize 参数不参与计算(仅单声道 memcpy 使用), 传不同值结果一致
    private static void testGenerateMixPCMData_Stereo_ShortArray_NoBufferSizeUse()
    {
        short[] data = { 100, 300, -40, 60 };
        short[] mixA = new short[2];
        short[] mixB = new short[2];
        WavSound.generateMixPCMData(mixA, 2, 2, data, 4);
        WavSound.generateMixPCMData(mixB, 2, 2, data, 1); // bufferSize 不影响双声道
        assertEqual(mixA[0], mixB[0], "双声道结果不依赖 bufferSize");
        assertEqual(mixA[1], mixB[1], "双声道结果不依赖 bufferSize");
    }

    // 空数据 / 0 计数: 不抛异常, mix 保持默认
    private static void testGenerateMixPCMData_Empty()
    {
        byte[] data = { 0x34, 0x12 };
        short[] mix = new short[1];
        WavSound.generateMixPCMData(mix, 0, 1, data);
        assertEqual((short)0, mix[0], "mixDataCount=0 时无写入");
    }

    // ═══════════════════════════════════════════════════════════════════
    // 实例方法: startWaveStream → pushWaveStream → endWaveStream 纯内存链路
    // 不依赖真实 wav 文件/麦克风, 用 WaveFormatEx 头 + 内存字节构造可控 PCM 数据。
    // ═══════════════════════════════════════════════════════════════════
    // 单声道 4 字节(2 采样): startWaveStream 后 pushWaveStream({0x34,0x12,0x78,0x56},4)
    //   endWaveStream: mDataBuffer=[0x34,0x12,0x78,0x56], getPCMBufferSize=4,
    //   getPCMShortDataCount=4/2=2, getMixPCMDataCount=4/(2*1)=2,
    //   mix[0]=0x1234, mix[1]=0x5678
    private static void testWaveStreamMono()
    {
        WavSound sound = new();
        try
        {
            WaveFormatEx header = new()
            {
                wFormatTag = 1,
                nChannels = 1,
                nSamplesPerSec = 8000,
                nAvgBytesPerSec = 16000,
                nBlockAlign = 2,
                wBitsPerSample = 16,
                cbSize = 0
            };
            sound.startWaveStream(header);
            byte[] pcm = { 0x34, 0x12, 0x78, 0x56 };
            sound.pushWaveStream(pcm, pcm.Length);
            sound.endWaveStream();
            assertEqual((short)1, sound.getSoundChannels(), "单声道 nChannels=1");
            assertEqual(4, sound.getPCMBufferSize(), "单声道 2 采样 buffer 长度 4");
            assertEqual(2, sound.getPCMShortDataCount(), "单声道 short 采样数 2");
            assertEqual(2, sound.getMixPCMDataCount(), "单声道 mix 数 = 2");
            short[] mix = sound.getMixPCMData();
            assertEqual((short)0x1234, mix[0], "单声道 mix[0] = 0x1234");
            assertEqual((short)0x5678, mix[1], "单声道 mix[1] = 0x5678");
        }
        finally
        {
            sound.resetProperty();
        }
    }
    // 双声道 4 字节(1 采样): getMixPCMDataCount=4/(2*2)=1, mix[0]=(4660+22136)*0.5=13398
    private static void testWaveStreamStereo()
    {
        WavSound sound = new();
        try
        {
            WaveFormatEx header = new()
            {
                wFormatTag = 1,
                nChannels = 2,
                nSamplesPerSec = 44100,
                nAvgBytesPerSec = 176400,
                nBlockAlign = 4,
                wBitsPerSample = 16,
                cbSize = 0
            };
            sound.startWaveStream(header);
            byte[] pcm = { 0x34, 0x12, 0x78, 0x56 }; // left=0x1234=4660, right=0x5678=22136
            sound.pushWaveStream(pcm, pcm.Length);
            sound.endWaveStream();
            assertEqual((short)2, sound.getSoundChannels(), "双声道 nChannels=2");
            assertEqual(1, sound.getMixPCMDataCount(), "双声道 4 字节 mix 数 = 4/(2*2) = 1");
            short[] mix = sound.getMixPCMData();
            assertEqual((short)13398, mix[0], "双声道 mix[0] = (4660+22136)*0.5 = 13398");
        }
        finally
        {
            sound.resetProperty();
        }
    }
    // resetProperty 清空 buffer: endWaveStream 后 resetProperty → getPCMBuffer null, getMixPCMData null
    private static void testResetPropertyAfterStream()
    {
        WavSound sound = new();
        WaveFormatEx header = new()
        {
            wFormatTag = 1,
            nChannels = 1,
            nSamplesPerSec = 8000,
            nAvgBytesPerSec = 16000,
            nBlockAlign = 2,
            wBitsPerSample = 16,
            cbSize = 0
        };
        sound.startWaveStream(header);
        byte[] pcm = { 0x34, 0x12, 0x78, 0x56 };
        sound.pushWaveStream(pcm, pcm.Length);
        sound.endWaveStream();
        assertEqual(4, sound.getPCMBufferSize(), "endWaveStream 后 buffer 长度 4");
        sound.resetProperty();
        assertNull(sound.getPCMBuffer(), "resetProperty 后 getPCMBuffer 为 null");
        assertNull(sound.getMixPCMData(), "resetProperty 后 getMixPCMData 为 null");
        assertEqual((short)0, sound.getSoundChannels(), "resetProperty 后声道数归 0");
    }
#endif
}