using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)] 
public struct WaveHdr
{
	public IntPtr lpData;			/* pointer to locked data buffer */
	public int dwBufferLength;		/* length of data buffer */
	public int dwBytesRecorded;	/* used for input only */
	public int dwUser;			/* for client's use */
	public int dwFlags;			/* assorted flags (see defines) */
	public int dwLoops;			/* loop control counter */
	public IntPtr lpNext;		/* reserved for driver */
	public IntPtr reserved;		/* reserved for driver */
}
[StructLayout(LayoutKind.Sequential)]
public struct WaveFormatEx
{
	public short wFormatTag;		/* format type */
	public short nChannels;		/* number of channels (i.e. mono, stereo...) */
	public int nSamplesPerSec;		/* sample rate */
	public int nAvgBytesPerSec;	/* for buffer estimation */
	public short nBlockAlign;		/* block size of data */
	public short wBitsPerSample;	/* number of bits per sample of mono data */
	public short cbSize;			/* the count in bytes of the size of */
	/* extra information (after cbSize) */
}
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct WaveInCaps
{
	public short wMid;						/* manufacturer ID */
	public short wPid;						/* product ID */
	public int vDriverVersion;				/* version of the driver */
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public char[] szPname;					/* product name (NULL terminated string) */
	public int dwFormats;					/* formats supported */
	public short wChannels;					/* number of channels supported */
	public short wReserved1;				/* structure packing */
}

// 音频回调
public delegate int WaveDelegate(IntPtr hwavein, uint uMsg, int dwInstance, int dwParam1, int dwParam2);
// 音频函数导入委托
public delegate int waveInGetNumDevsDelegate();
public delegate int waveInAddBufferDelegate(IntPtr hwi, ref WaveHdr pwh, int cbwh);
public delegate int waveInCloseDelegate(IntPtr hwi);
public delegate int waveInOpenDelegate(out IntPtr phwi, uint uDeviceID, ref WaveFormatEx lpFormat, WaveDelegate dwCallback, IntPtr dwInstance, int dwFlags);
public delegate int waveInPrepareHeaderDelegate(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);
public delegate int waveInUnprepareHeaderDelegate(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);
public delegate int waveInResetDelegate(IntPtr hwi);
public delegate int waveInStartDelegate(IntPtr hwi);
public delegate int waveInStopDelegate(IntPtr hwi);
public delegate int waveInGetDevCapsADelegate(int hwo, ref WaveInCaps lpCaps, int uSize);

public class Winmm : UnityUtility
{
	public const string WINMM_DLL = "winmm.dll";
	public static int waveInGetNumDevs()
	{
		waveInGetNumDevsDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInGetNumDevs", Typeof<waveInGetNumDevsDelegate>()) as waveInGetNumDevsDelegate;
		return d();
	}
	public static int waveInAddBuffer(IntPtr hwi, ref WaveHdr pwh, int cbwh)
	{
		waveInAddBufferDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInAddBuffer", Typeof<waveInAddBufferDelegate>()) as waveInAddBufferDelegate;
		return d(hwi, ref pwh, cbwh);
	}
	public static int waveInClose(IntPtr hwi)
	{
		waveInCloseDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInClose", Typeof<waveInCloseDelegate>()) as waveInCloseDelegate;
		return d(hwi);
	}
	public static int waveInOpen(out IntPtr phwi, uint uDeviceID, ref WaveFormatEx lpFormat, WaveDelegate dwCallback, IntPtr dwInstance, int dwFlags)
	{
		waveInOpenDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInOpen", Typeof<waveInOpenDelegate>()) as waveInOpenDelegate;
		return d(out phwi, uDeviceID, ref lpFormat, dwCallback, dwInstance, dwFlags);
	}
	public static int waveInPrepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize)
	{
		waveInPrepareHeaderDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInPrepareHeader", Typeof<waveInPrepareHeaderDelegate>()) as waveInPrepareHeaderDelegate;
		return d(hWaveIn, ref lpWaveInHdr, uSize);
	}
	public static int waveInUnprepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize)
	{
		waveInUnprepareHeaderDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInUnprepareHeader", Typeof<waveInUnprepareHeaderDelegate>()) as waveInUnprepareHeaderDelegate;
		return d(hWaveIn, ref lpWaveInHdr, uSize);
	}
	public static int waveInReset(IntPtr hwi)
	{
		waveInResetDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInReset", Typeof<waveInResetDelegate>()) as waveInResetDelegate;
		return d(hwi);
	}
	public static int waveInStart(IntPtr hwi)
	{
		waveInStartDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInStart", Typeof<waveInStartDelegate>()) as waveInStartDelegate;
		return d(hwi);
	}
	public static int waveInStop(IntPtr hwi)
	{
		waveInStopDelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInStop", Typeof<waveInStopDelegate>()) as waveInStopDelegate;
		return d(hwi);
	}
	public static int waveInGetDevCapsA(int hwo, ref WaveInCaps lpCaps, int uSize)
	{
		waveInGetDevCapsADelegate d = DllImportExtern.Invoke(WINMM_DLL, "waveInGetDevCapsA", Typeof<waveInGetDevCapsADelegate>()) as waveInGetDevCapsADelegate;
		return d(hwo, ref lpCaps, uSize);
	}
}