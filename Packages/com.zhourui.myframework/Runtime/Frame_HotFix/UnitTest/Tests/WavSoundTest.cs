using static WavSound;
using static TestAssert;

// WavSound.generateMixPCMData 纯逻辑函数测试
// generateMixPCMData 是 public static 函数, 不依赖 Unity 对象, 可独立测试
public static class WavSoundTest
{
	public static void Run()
	{
		testGenerateMixPCMDataByteSingleChannel();
		testGenerateMixPCMDataByteDualChannel();
		testGenerateMixPCMDataShortSingleChannel();
		testGenerateMixPCMDataShortDualChannel();
	}

	// byte[] 单声道: 直接拷贝 byte -> short
	private static void testGenerateMixPCMDataByteSingleChannel()
	{
		// PCM 16-bit 单声道: [0x01,0x00, 0x02,0x00, 0x03,0x00] = 3 samples: 1,2,3
		byte[] dataBuffer = { 0x01, 0x00, 0x02, 0x00, 0x03, 0x00 };
		short[] mixPCMData = new short[3];
		generateMixPCMData(mixPCMData, 3, 1, dataBuffer);
		assertEqual((short)1, mixPCMData[0], "byte 单声道 sample0=1");
		assertEqual((short)2, mixPCMData[1], "byte 单声道 sample1=2");
		assertEqual((short)3, mixPCMData[2], "byte 单声道 sample2=3");
	}

	// byte[] 双声道: 左右声道取平均
	private static void testGenerateMixPCMDataByteDualChannel()
	{
		// PCM 16-bit 双声道: [L0_lo,L0_hi, R0_lo,R0_hi, L1_lo,L1_hi, R1_lo,R1_hi]
		// L0=10, R0=20 → avg=15; L1=100, R1=200 → avg=150
		byte[] dataBuffer = {
			0x0A, 0x00, 0x14, 0x00,   // sample0: L=10, R=20
			0x64, 0x00, 0xC8, 0x00    // sample1: L=100, R=200
		};
		short[] mixPCMData = new short[2];
		generateMixPCMData(mixPCMData, 2, 2, dataBuffer);
		assertEqual((short)15, mixPCMData[0], "byte 双声道 sample0 avg=15");
		assertEqual((short)150, mixPCMData[1], "byte 双声道 sample1 avg=150");
	}

	// short[] 单声道: 直接拷贝
	private static void testGenerateMixPCMDataShortSingleChannel()
	{
		short[] dataBuffer = { 100, 200, 300 };
		short[] mixPCMData = new short[3];
		generateMixPCMData(mixPCMData, 3, 1, dataBuffer, 3);
		assertEqual((short)100, mixPCMData[0], "short 单声道 sample0=100");
		assertEqual((short)200, mixPCMData[1], "short 单声道 sample1=200");
		assertEqual((short)300, mixPCMData[2], "short 单声道 sample2=300");
	}

	// short[] 双声道: 左右声道取平均
	private static void testGenerateMixPCMDataShortDualChannel()
	{
		// [L0,R0, L1,R1] → avg0, avg1
		short[] dataBuffer = { 10, 20, 100, 200 };
		short[] mixPCMData = new short[2];
		generateMixPCMData(mixPCMData, 2, 2, dataBuffer, 4);
		assertEqual((short)15, mixPCMData[0], "short 双声道 sample0 avg=15");
		assertEqual((short)150, mixPCMData[1], "short 双声道 sample1 avg=150");
	}
}
