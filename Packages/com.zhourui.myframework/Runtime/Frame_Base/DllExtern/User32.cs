#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;

public class User32
{
	public const string USER32_DLL = "user32.dll";
	[DllImport(USER32_DLL, SetLastError = true)]
	public extern static long SetWindowLong(IntPtr hwnd, int _nIndex, long dwNewLong);
	[DllImport(USER32_DLL, SetLastError = true)]
	public extern static long GetWindowLong(IntPtr hwnd, int _nIndex);
	[DllImport(USER32_DLL, SetLastError = true)]
	public extern static bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	[DllImport(USER32_DLL, SetLastError = true)]
	public extern static IntPtr GetForegroundWindow();
}
#endif