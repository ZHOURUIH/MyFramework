using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

[StructLayout(LayoutKind.Sequential)]
public struct MEMORY_STATUS_EX
{
	public uint dwLength;
	public uint dwMemoryLoad;
	public ulong dwTotalPhys;
	public ulong dwAvailPhys;
	public ulong dwTotalPageFile;
	public ulong dwAvailPageFile;
	public ulong dwTotalVirtual;
	public ulong dwAvailVirtual;
	public ulong dwAvailExtendedVirtual;
}

public class Kernel32
{
	public const short INVALID_HANDLE_VALUE = -1;
	public const short FILE_ATTRIBUTE_NORMAL = 0x80;
	public const uint GENERIC_READ = 0x80000000;
	public const uint GENERIC_WRITE = 0x40000000;
	public const uint FILE_SHARE_READ = 0x00000001;
	public const uint FILE_SHARE_WRITE = 0x00000002;
	public const uint PURGE_TXABORT = 0x0001;   // Kill the pending/current writes to the comm port.
	public const uint PURGE_RXABORT = 0x0002;   // Kill the pending/current reads to the comm port.
	public const uint PURGE_TXCLEAR = 0x0004;   // Kill the transmit queue if there.
	public const uint PURGE_RXCLEAR = 0x0008;   // Kill the typeahead buffer if there.
	public const uint CREATE_NEW = 1;
	public const uint CREATE_ALWAYS = 2;
	public const uint OPEN_EXISTING = 3;
	public const string KERNEL32_DLL = "kernel32.dll";
	[DllImport(KERNEL32_DLL)]
	public extern static IntPtr LoadLibrary(string path);
	[DllImport(KERNEL32_DLL)]
	public extern static IntPtr GetProcAddress(IntPtr lib, string funcName);
	[DllImport(KERNEL32_DLL)]
	public extern static bool FreeLibrary(IntPtr lib);
	[DllImport(KERNEL32_DLL)]
	public static extern void GlobalMemoryStatusEx(ref MEMORY_STATUS_EX meminfo);
	[DllImport(KERNEL32_DLL)]
	public static extern int GetLastError();
	[DllImport(KERNEL32_DLL)]
	public static extern void SetLastError(int err);
	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);
	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, ref uint lpNumberOfBytesWritten, IntPtr lpOverlapped);
	[DllImport(KERNEL32_DLL)]
	public static extern void CloseHandle(SafeFileHandle hFile);
	[DllImport(KERNEL32_DLL)]
	public static extern bool PurgeComm(SafeFileHandle hFile, uint dwFlags);
}