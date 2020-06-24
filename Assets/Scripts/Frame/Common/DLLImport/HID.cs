using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

[StructLayout(LayoutKind.Sequential)]
public struct HIDD_ATTRIBUTES
{
	public int Size;
	public short VendorID;
	public short ProductID;
	public short VersionNumber;
}
[StructLayout(LayoutKind.Sequential)]
public struct SP_DEVINFO_DATA
{
	public uint cbSize;
	public Guid ClassGuid;
	public uint DevInst;
	public IntPtr Reserved;
}
[StructLayout(LayoutKind.Sequential)]
public struct HIDP_CAPS
{
	public ushort Usage;                 // USHORT
	public ushort UsagePage;             // USHORT
	public ushort InputReportByteLength;
	public ushort OutputReportByteLength;
	public ushort FeatureReportByteLength;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
	public ushort[] Reserved;                // USHORT  Reserved[17];			
	public ushort NumberLinkCollectionNodes;
	public ushort NumberInputButtonCaps;
	public ushort NumberInputValueCaps;
	public ushort NumberInputDataIndices;
	public ushort NumberOutputButtonCaps;
	public ushort NumberOutputValueCaps;
	public ushort NumberOutputDataIndices;
	public ushort NumberFeatureButtonCaps;
	public ushort NumberFeatureValueCaps;
	public ushort NumberFeatureDataIndices;
}

public class HID
{
	public const string HID_DLL = "hid.dll";
	[DllImport(HID_DLL, SetLastError = true)]
	public static extern void HidD_GetHidGuid(ref Guid hidGuid);
	[DllImport(HID_DLL, SetLastError = true)]
	public static extern bool HidD_GetPreparsedData(SafeFileHandle hObject, ref IntPtr PreparsedData);
	[DllImport(HID_DLL, SetLastError = true)]
	public static extern Boolean HidD_FreePreparsedData(ref IntPtr PreparsedData);
	[DllImport(HID_DLL, SetLastError = true)]
	public static extern int HidP_GetCaps(IntPtr pPHIDP_PREPARSED_DATA, ref HIDP_CAPS myPHIDP_CAPS);
	[DllImport(HID_DLL, SetLastError = true)]
	public static extern Boolean HidD_GetAttributes(SafeFileHandle hObject, ref HIDD_ATTRIBUTES Attributes);
	[DllImport(HID_DLL, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	public static extern bool HidD_GetFeature(IntPtr hDevice, IntPtr hReportBuffer, uint ReportBufferLength);
	[DllImport(HID_DLL, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	public static extern bool HidD_SetFeature(IntPtr hDevice, IntPtr ReportBuffer, uint ReportBufferLength);
	[DllImport(HID_DLL, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	public static extern bool HidD_GetProductString(SafeFileHandle hDevice, IntPtr Buffer, uint BufferLength);
	[DllImport(HID_DLL, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	public static extern bool HidD_GetSerialNumberString(SafeFileHandle hDevice, IntPtr Buffer, uint BufferLength);
	[DllImport(HID_DLL, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
	public static extern Boolean HidD_GetManufacturerString(SafeFileHandle hDevice, IntPtr Buffer, uint BufferLength);
}