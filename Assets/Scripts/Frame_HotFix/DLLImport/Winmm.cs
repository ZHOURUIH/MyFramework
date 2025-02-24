using System;
using System.Runtime.InteropServices;
using static FrameBaseHotFix;
using static CSharpUtility;

[StructLayout(LayoutKind.Sequential)] 
public struct WaveHdr
{
	public IntPtr lpData;			// pointer to locked data buffer
	public int dwBufferLength;		// length of data buffer
	public int dwBytesRecorded;		// used for input only
	public int dwUser;				// for client's use
	public int dwFlags;				// assorted flags (see defines)
	public int dwLoops;				// loop control counter
	public IntPtr lpNext;			// reserved for driver
	public IntPtr reserved;			// reserved for driver
}
[StructLayout(LayoutKind.Sequential)]
public struct WaveFormatEx
{
	public short wFormatTag;		// format type
	public short nChannels;			// number of channels (i.e. mono, stereo...)
	public int nSamplesPerSec;		// sample rate
	public int nAvgBytesPerSec;		// for buffer estimation
	public short nBlockAlign;		// block size of data
	public short wBitsPerSample;	// number of bits per sample of mono data
	public short cbSize;			// the count in bytes of the size of
	// extra information (after cbSize)
}
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct WaveInCaps
{
	public short wMid;						// manufacturer ID
	public short wPid;						// product ID
	public int vDriverVersion;				// version of the driver
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public char[] szPname;					// product name (NULL terminated string)
	public int dwFormats;					// formats supported
	public short wChannels;					// number of channels supported
	public short wReserved1;				// structure packing
}

// 音频回调
public delegate int WaveDelegate(IntPtr hwavein, uint uMsg, int dwInstance, int dwParam1, int dwParam2);
// 音频函数导入委托,委托名需要与库中函数名一致
public delegate int waveInGetNumDevs();
public delegate int waveInAddBuffer(IntPtr hwi, ref WaveHdr pwh, int cbwh);
public delegate int waveInClose(IntPtr hwi);
public delegate int waveInOpen(out IntPtr phwi, uint uDeviceID, ref WaveFormatEx lpFormat, WaveDelegate dwCallback, IntPtr dwInstance, int dwFlags);
public delegate int waveInPrepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);
public delegate int waveInUnprepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);
public delegate int waveInReset(IntPtr hwi);
public delegate int waveInStart(IntPtr hwi);
public delegate int waveInStop(IntPtr hwi);
public delegate int waveInGetDevCapsA(int hwo, ref WaveInCaps lpCaps, int uSize);

// 导入的winmm动态库
public class Winmm
{
	public const string WINMM_DLL = "winmm.dll";	// 动态库文件名
	public static int waveInGetNumDevs()
	{
		return getFunction<waveInGetNumDevs>()();
	}
	public static int waveInAddBuffer(IntPtr hwi, ref WaveHdr pwh, int cbwh)
	{
		return getFunction<waveInAddBuffer>()(hwi, ref pwh, cbwh);
	}
	public static int waveInClose(IntPtr hwi)
	{
		return getFunction<waveInClose>()(hwi);
	}
	public static int waveInOpen(out IntPtr phwi, uint uDeviceID, ref WaveFormatEx lpFormat, WaveDelegate dwCallback, IntPtr dwInstance, int dwFlags)
	{
		return getFunction<waveInOpen>()(out phwi, uDeviceID, ref lpFormat, dwCallback, dwInstance, dwFlags);
	}
	public static int waveInPrepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize)
	{
		return getFunction<waveInPrepareHeader>()(hWaveIn, ref lpWaveInHdr, uSize);
	}
	public static int waveInUnprepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize)
	{
		return getFunction<waveInUnprepareHeader>()(hWaveIn, ref lpWaveInHdr, uSize);
	}
	public static int waveInReset(IntPtr hwi)
	{
		return getFunction<waveInReset>()(hwi);
	}
	public static int waveInStart(IntPtr hwi)
	{
		return getFunction<waveInStart>()(hwi);
	}
	public static int waveInStop(IntPtr hwi)
	{
		return getFunction<waveInStop>()(hwi);
	}
	public static int waveInGetDevCapsA(int hwo, ref WaveInCaps lpCaps, int uSize)
	{
		return getFunction<waveInGetDevCapsA>()(hwo, ref lpCaps, uSize);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static T getFunction<T>() where T : Delegate
	{
		return mDllImportSystem.invoke<T>(WINMM_DLL, typeof(T).Name);
	}
}