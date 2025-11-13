#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct SHELLEXECUTEINFO
{
    public int cbSize;
    public uint fMask;
    public IntPtr hwnd;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string lpVerb;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string lpFile;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string lpParameters;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string lpDirectory;
    public int nShow;
    public IntPtr hInstApp;
    public IntPtr lpIDList;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string lpClass;
    public IntPtr hkeyClass;
    public uint dwHotKey;
    public IntPtr hIcon;
    public IntPtr hProcess;
}

// 窗口显示枚举
public enum ShowCommands : int
{
    SW_HIDE = 0,            // i don't know
	SW_SHOWNORMAL = 1,      // i don't know
	SW_NORMAL = 1,          // i don't know
	SW_SHOWMINIMIZED = 2,   // i don't know
	SW_SHOWMAXIMIZED = 3,   // i don't know
	SW_MAXIMIZE = 3,        // i don't know
	SW_SHOWNOACTIVATE = 4,  // i don't know
	SW_SHOW = 5,            // i don't know
	SW_MINIMIZE = 6,        // i don't know
	SW_SHOWMINNOACTIVE = 7, // i don't know
	SW_SHOWNA = 8,          // i don't know
	SW_RESTORE = 9,         // i don't know
	SW_SHOWDEFAULT = 10,    // i don't know
	SW_FORCEMINIMIZE = 11,  // i don't know
	SW_MAX = 11             // i don't know
}

// 系统枚举
[Flags]
public enum ShellExecuteMaskFlags : uint
{
    SEE_MASK_DEFAULT = 0x00000000,				// i don't know
	SEE_MASK_CLASSNAME = 0x00000001,			// i don't know
	SEE_MASK_CLASSKEY = 0x00000003,				// i don't know
	SEE_MASK_IDLIST = 0x00000004,				// i don't know
	SEE_MASK_INVOKEIDLIST = 0x0000000c,			// Note SEE_MASK_INVOKEIDLIST(0xC) implies SEE_MASK_IDLIST(0x04)
    SEE_MASK_HOTKEY = 0x00000020,				// i don't know
	SEE_MASK_NOCLOSEPROCESS = 0x00000040,		// i don't know
	SEE_MASK_CONNECTNETDRV = 0x00000080,		// i don't know
	SEE_MASK_NOASYNC = 0x00000100,				// i don't know
	SEE_MASK_FLAG_DDEWAIT = SEE_MASK_NOASYNC,   // i don't know
	SEE_MASK_DOENVSUBST = 0x00000200,			// i don't know
	SEE_MASK_FLAG_NO_UI = 0x00000400,			// i don't know
	SEE_MASK_UNICODE = 0x00004000,				// i don't know
	SEE_MASK_NO_CONSOLE = 0x00008000,			// i don't know
	SEE_MASK_ASYNCOK = 0x00100000,				// i don't know
	SEE_MASK_HMONITOR = 0x00200000,				// i don't know
	SEE_MASK_NOZONECHECKS = 0x00800000,			// i don't know
	SEE_MASK_NOQUERYCLASSSTORE = 0x01000000,    // i don't know
	SEE_MASK_WAITFORINPUTIDLE = 0x02000000,     // i don't know
	SEE_MASK_FLAG_LOG_USAGE = 0x04000000,       // i don't know
}

public class Shell32
{
    // 字符集设置为unicode,可以避免启动中文路径的文件时报错
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
}
#endif