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

public enum ShowCommands : int
{
    SW_HIDE = 0,
    SW_SHOWNORMAL = 1,
    SW_NORMAL = 1,
    SW_SHOWMINIMIZED = 2,
    SW_SHOWMAXIMIZED = 3,
    SW_MAXIMIZE = 3,
    SW_SHOWNOACTIVATE = 4,
    SW_SHOW = 5,
    SW_MINIMIZE = 6,
    SW_SHOWMINNOACTIVE = 7,
    SW_SHOWNA = 8,
    SW_RESTORE = 9,
    SW_SHOWDEFAULT = 10,
    SW_FORCEMINIMIZE = 11,
    SW_MAX = 11
}

[Flags]
public enum ShellExecuteMaskFlags : uint
{
    SEE_MASK_DEFAULT = 0x00000000,
    SEE_MASK_CLASSNAME = 0x00000001,
    SEE_MASK_CLASSKEY = 0x00000003,
    SEE_MASK_IDLIST = 0x00000004,
    SEE_MASK_INVOKEIDLIST = 0x0000000c,   // Note SEE_MASK_INVOKEIDLIST(0xC) implies SEE_MASK_IDLIST(0x04)
    SEE_MASK_HOTKEY = 0x00000020,
    SEE_MASK_NOCLOSEPROCESS = 0x00000040,
    SEE_MASK_CONNECTNETDRV = 0x00000080,
    SEE_MASK_NOASYNC = 0x00000100,
    SEE_MASK_FLAG_DDEWAIT = SEE_MASK_NOASYNC,
    SEE_MASK_DOENVSUBST = 0x00000200,
    SEE_MASK_FLAG_NO_UI = 0x00000400,
    SEE_MASK_UNICODE = 0x00004000,
    SEE_MASK_NO_CONSOLE = 0x00008000,
    SEE_MASK_ASYNCOK = 0x00100000,
    SEE_MASK_HMONITOR = 0x00200000,
    SEE_MASK_NOZONECHECKS = 0x00800000,
    SEE_MASK_NOQUERYCLASSSTORE = 0x01000000,
    SEE_MASK_WAITFORINPUTIDLE = 0x02000000,
    SEE_MASK_FLAG_LOG_USAGE = 0x04000000,
}

public class Shell32
{
    // 字符集设置为unicode,可以避免启动中文路径的文件时报错
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
}