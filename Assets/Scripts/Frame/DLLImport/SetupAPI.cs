using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct SP_DEVICE_INTERFACE_DATA
{
	public uint cbSize;
	public Guid InterfaceClassGuid;
	public uint Flags;
	public IntPtr Reserved;
}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct SP_DEVICE_INTERFACE_DETAIL_DATA
{
	public int cbSize;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string DevicePath;
}
public class SetupAPI
{
	public const int DIGCF_DEFAULT = 0x1;
	public const int DIGCF_PRESENT = 0x2;
	public const int DIGCF_ALLCLASSES = 0x4;
	public const int DIGCF_PROFILE = 0x8;
	public const int DIGCF_DEVICEINTERFACE = 0x10;
	public const string SETUP_API_DLL = "setupapi.dll";
	[DllImport(SETUP_API_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);
	[DllImport(@SETUP_API_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);
	[DllImport(@SETUP_API_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, ref SP_DEVINFO_DATA deviceInfoData);
	[DllImport(SETUP_API_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
}