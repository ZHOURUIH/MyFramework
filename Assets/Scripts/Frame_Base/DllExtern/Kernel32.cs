#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

// 系统枚举
[Flags]
public enum ProcessAccessRights : uint
{
    PROCESS_CREATE_PROCESS = 0x0080,			// Required to create a process.
    PROCESS_CREATE_THREAD = 0x0002,				// Required to create a thread.
    PROCESS_DUP_HANDLE = 0x0040,				// Required to duplicate a handle using DuplicateHandle.
    PROCESS_QUERY_INFORMATION = 0x0400,			// Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken, GetExitCodeProcess, GetPriorityClass, and IsProcessInJob).
    PROCESS_QUERY_LIMITED_INFORMATION = 0x1000, // Required to retrieve certain information about a process (see QueryFullProcessImageName). A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION. Windows Server 2003 and Windows XP/2000:  This access right is not supported.
    PROCESS_SET_INFORMATION = 0x0200,			// Required to set certain information about a process, such as its priority class (see SetPriorityClass).
    PROCESS_SET_QUOTA = 0x0100,					// Required to set memory limits using SetProcessWorkingSetSize.
    PROCESS_SUSPEND_RESUME = 0x0800,			// Required to suspend or resume a process.
    PROCESS_TERMINATE = 0x0001,					// Required to terminate a process using TerminateProcess.
    PROCESS_VM_OPERATION = 0x0008,				// Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
    PROCESS_VM_READ = 0x0010,					// Required to read memory in a process using ReadProcessMemory.
    PROCESS_VM_WRITE = 0x0020,					// Required to write to memory in a process using WriteProcessMemory.
    DELETE = 0x00010000,						// Required to delete the object.
    READ_CONTROL = 0x00020000,					// Required to read information in the security descriptor for the object, not including the information in the SACL. To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
    SYNCHRONIZE = 0x00100000,					// The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
    WRITE_DAC = 0x00040000,						// Required to modify the DACL in the security descriptor for the object.
    WRITE_OWNER = 0x00080000,					// Required to change the owner in the security descriptor for the object.
    STANDARD_RIGHTS_REQUIRED = 0x000f0000,		// i don't know
    PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF //    All possible access rights for a process object.
}

[StructLayout(LayoutKind.Sequential)]
public struct PROCESS_INFORMATION
{
    public IntPtr hProcess;
    public IntPtr hThread;
    public uint dwProcessId;
	public uint dwThreadId;
}

[StructLayout(LayoutKind.Sequential)]
public struct STARTUPINFO
{
    public uint cb;
    public IntPtr lpReserved;
    public IntPtr lpDesktop;
    public IntPtr lpTitle;
    public uint dwX;
    public uint dwY;
    public uint dwXSize;
    public uint dwYSize;
    public uint dwXCountChars;
    public uint dwYCountChars;
    public uint dwFillAttribute;
    public uint dwFlags;
    public ushort wShowWindow;
    public ushort cbReserved2;
    public IntPtr lpReserved2;
    public IntPtr hStdInput;
    public IntPtr hStdOutput;
    public IntPtr hStdError;
}

// 系统枚举
[Flags]
public enum ProcessCreationFlags : uint
{
    NONE = 0,                                   // i don't know
	CREATE_BREAKAWAY_FROM_JOB = 0x01000000,     // i don't know
	CREATE_DEFAULT_ERROR_MODE = 0x04000000,     // i don't know
	CREATE_NEW_CONSOLE = 0x00000010,            // i don't know
	CREATE_NEW_PROCESS_GROUP = 0x00000200,      // i don't know
	CREATE_NO_WINDOW = 0x08000000,              // i don't know
	CREATE_PROTECTED_PROCESS = 0x00040000,      // i don't know
	CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,  // i don't know
	CREATE_SECURE_PROCESS = 0x00400000,         // i don't know
	CREATE_SEPARATE_WOW_VDM = 0x00000800,       // i don't know
	CREATE_SHARED_WOW_VDM = 0x00001000,         // i don't know
	CREATE_SUSPENDED = 0x00000004,              // i don't know
	CREATE_UNICODE_ENVIRONMENT = 0x00000400,    // i don't know
	DEBUG_ONLY_THIS_PROCESS = 0x00000002,       // i don't know
	DEBUG_PROCESS = 0x00000001,                 // i don't know
	DETACHED_PROCESS = 0x00000008,              // i don't know
	EXTENDED_STARTUPINFO_PRESENT = 0x00080000,  // i don't know
	INHERIT_PARENT_AFFINITY = 0x00010000        // i don't know
}

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
	public static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, 
													uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, 
										ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, 
										ref uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

	[DllImport(KERNEL32_DLL)]
	public static extern void CloseHandle(SafeFileHandle hFile);

	[DllImport(KERNEL32_DLL)]
	public static extern bool PurgeComm(SafeFileHandle hFile, uint dwFlags);

	[DllImport(KERNEL32_DLL)]
	public static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CreateProcessW(
            string lpApplicationName,
            [In] string lpCommandLine,
            IntPtr procSecAttrs,
            IntPtr threadSecAttrs,
            bool bInheritHandles,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation
        );

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TerminateProcess(IntPtr processHandle, uint exitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(ProcessAccessRights access, bool inherit, uint processId);
}
#endif