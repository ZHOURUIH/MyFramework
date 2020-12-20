using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public struct InterfaceDetails
{
	public string manufacturer;
	public string product;
	public string serialNumber;
	public ushort VID;
	public ushort PID;
	public string devicePath;
	public int IN_reportByteLength;
	public int OUT_reportByteLength;
	public ushort versionNumber;
}
public class HIDDevice : FrameBase
{
    private HIDP_CAPS capabilities;
    public SafeFileHandle mHandle;
    public InterfaceDetails mProductInfo;
    public bool mDeviceConnected;
    /// <summary>
    /// Creates an object to handle read/write functionality for a USB HID device
    /// Uses one filestream for each of read/write to allow for a write to occur during a blocking
    /// asnychronous read
    /// </summary>
    /// <param name="VID">The vendor ID of the USB device to connect to</param>
    /// <param name="PID">The product ID of the USB device to connect to</param>
    /// <param name="serialNumber">The serial number of the USB device to connect to</param>
    /// <param name="useAsyncReads">True - Read the device and generate events on data being available</param>
    public HIDDevice(ushort VID, ushort PID)
    {
        InterfaceDetails[] devices = getConnectedDevices();
        //loop through all connected devices to find one with the correct details
        for (int i = 0; i < devices.Length; i++)
        {
            if ((devices[i].VID == VID) && (devices[i].PID == PID))
            {
                initDevice(devices[i].devicePath);
                break;
            }
        }
    }
    /// <summary>
    /// Creates an object to handle read/write functionality for a USB HID device
    /// Uses one filestream for each of read/write to allow for a write to occur during a blocking
    /// asnychronous read
    /// </summary>
    /// <param name="devicePath">The USB device path - from getConnectedDevices</param>
    /// <param name="useAsyncReads">True - Read the device and generate events on data being available</param>
    public HIDDevice(string devicePath)
    {
        initDevice(devicePath);
    }
    private void initDevice(string devicePath)
    {
        mDeviceConnected = false;

        //create file handles using CT_CreateFile
        mHandle = Kernel32.CreateFile(devicePath, Kernel32.GENERIC_READ | Kernel32.GENERIC_WRITE, Kernel32.FILE_SHARE_READ | Kernel32.FILE_SHARE_WRITE, IntPtr.Zero, Kernel32.OPEN_EXISTING, 0, IntPtr.Zero);

        //get capabilites - use getPreParsedData, and getCaps
        //store the reportlengths
        IntPtr ptrToPreParsedData = new IntPtr();
        HID.HidD_GetPreparsedData(mHandle, ref ptrToPreParsedData);

        capabilities = new HIDP_CAPS();
        HID.HidP_GetCaps(ptrToPreParsedData, ref capabilities);

        HIDD_ATTRIBUTES attributes = new HIDD_ATTRIBUTES();
        HID.HidD_GetAttributes(mHandle, ref attributes);

        string productName = EMPTY;
        string SN = EMPTY;
        string manfString = EMPTY;
        IntPtr buffer = Marshal.AllocHGlobal(126);//max alloc for string; 
        if (HID.HidD_GetProductString(mHandle, buffer, 126))
        {
            productName = Marshal.PtrToStringAuto(buffer);
        }
        if (HID.HidD_GetSerialNumberString(mHandle, buffer, 126))
        {
            SN = Marshal.PtrToStringAuto(buffer);
        }
        if (HID.HidD_GetManufacturerString(mHandle, buffer, 126))
        {
            manfString = Marshal.PtrToStringAuto(buffer);
        }
        Marshal.FreeHGlobal(buffer);

        //Call freePreParsedData to release some stuff
        HID.HidD_FreePreparsedData(ref ptrToPreParsedData);

        if (mHandle.IsInvalid)
        {
            return;
        }

        mDeviceConnected = true;

        //If connection was sucsessful, record the values in a global struct
        mProductInfo = new InterfaceDetails();
        mProductInfo.devicePath = devicePath;
        mProductInfo.manufacturer = manfString;
        mProductInfo.product = productName;
        mProductInfo.serialNumber = SN;
        mProductInfo.PID = (ushort)attributes.ProductID;
        mProductInfo.VID = (ushort)attributes.VendorID;
        mProductInfo.versionNumber = (ushort)attributes.VersionNumber;
        mProductInfo.IN_reportByteLength = capabilities.InputReportByteLength;
        mProductInfo.OUT_reportByteLength = capabilities.OutputReportByteLength;
    }
    public static InterfaceDetails[] getConnectedDevices()
    {
		InterfaceDetails[] devices = new InterfaceDetails[0];

        //Create structs to hold interface information
        SP_DEVINFO_DATA devInfo = new SP_DEVINFO_DATA();
		devInfo.cbSize = (uint)Marshal.SizeOf(devInfo);
		SP_DEVICE_INTERFACE_DATA devIface = new SP_DEVICE_INTERFACE_DATA();
        devIface.cbSize = (uint)(Marshal.SizeOf(devIface));

        Guid G = Guid.Empty;
        HID.HidD_GetHidGuid(ref G); //Get the guid of the HID device class

        IntPtr deviceInfo = SetupAPI.SetupDiGetClassDevs(ref G, IntPtr.Zero, IntPtr.Zero, SetupAPI.DIGCF_DEVICEINTERFACE | SetupAPI.DIGCF_PRESENT);
        //Loop through all available entries in the device list, until false
        int j = 0;
        while (true)
        {
            if (!SetupAPI.SetupDiEnumDeviceInterfaces(deviceInfo, IntPtr.Zero, ref G, (uint)j, ref devIface))
			{
				break;
			}
            uint requiredSize = 0;
			IntPtr detailMemory = Marshal.AllocHGlobal((int)requiredSize);
			SP_DEVICE_INTERFACE_DETAIL_DATA functionClassDeviceData = (SP_DEVICE_INTERFACE_DETAIL_DATA)Marshal.PtrToStructure(detailMemory, Typeof<SP_DEVICE_INTERFACE_DETAIL_DATA>());
			functionClassDeviceData.cbSize = Marshal.SizeOf(functionClassDeviceData);
			if (!SetupAPI.SetupDiGetDeviceInterfaceDetail(deviceInfo, ref devIface, ref functionClassDeviceData, requiredSize, out requiredSize, ref devInfo))
			{
				Marshal.FreeHGlobal(detailMemory);
				break;
			}
			string devicePath = functionClassDeviceData.DevicePath;
			Marshal.FreeHGlobal(detailMemory);

            //create file handles using CT_CreateFile
            uint desiredAccess = Kernel32.GENERIC_READ | Kernel32.GENERIC_WRITE;
            uint  shareMode = Kernel32.FILE_SHARE_READ | Kernel32.FILE_SHARE_WRITE;
            SafeFileHandle tempHandle = Kernel32.CreateFile(devicePath, desiredAccess, shareMode, 
                                                            IntPtr.Zero, Kernel32.OPEN_EXISTING, 0, IntPtr.Zero);
            //get capabilites - use getPreParsedData, and getCaps
            //store the reportlengths
            IntPtr ptrToPreParsedData = new IntPtr();
            bool ppdSucsess = HID.HidD_GetPreparsedData(tempHandle, ref ptrToPreParsedData);
            if (!ppdSucsess)
			{
				continue;
			}

            HIDP_CAPS capability = new HIDP_CAPS();
            HID.HidP_GetCaps(ptrToPreParsedData, ref capability);

            HIDD_ATTRIBUTES attributes = new HIDD_ATTRIBUTES();
            HID.HidD_GetAttributes(tempHandle, ref attributes);

            string productName = EMPTY;
            string SN = EMPTY;
            string manfString = EMPTY;
			const int bufferLen = 128;
            IntPtr buffer = Marshal.AllocHGlobal(bufferLen);
            if (HID.HidD_GetProductString(tempHandle, buffer, bufferLen))
			{
				productName = Marshal.PtrToStringAuto(buffer);
			}
			if (HID.HidD_GetSerialNumberString(tempHandle, buffer, bufferLen))
			{
				SN = Marshal.PtrToStringAuto(buffer);
			}
			if (HID.HidD_GetManufacturerString(tempHandle, buffer, bufferLen))
			{
				manfString = Marshal.PtrToStringAuto(buffer);
			}
            Marshal.FreeHGlobal(buffer);

			//Call freePreParsedData to release some stuff
			HID.HidD_FreePreparsedData(ref ptrToPreParsedData);

			//If connection was sucsessful, record the values in a global struct
			InterfaceDetails productInfo = new InterfaceDetails();
            productInfo.devicePath = devicePath;
            productInfo.manufacturer = manfString;
            productInfo.product = productName;
            productInfo.PID = (ushort)attributes.ProductID;
            productInfo.VID = (ushort)attributes.VendorID;
            productInfo.versionNumber = (ushort)attributes.VersionNumber;
            productInfo.IN_reportByteLength = capability.InputReportByteLength;
            productInfo.OUT_reportByteLength = capability.OutputReportByteLength;
            productInfo.serialNumber = SN;     //Check that serial number is actually a number
                
            int newSize = devices.Length + 1;
            Array.Resize(ref devices, newSize);
            devices[newSize - 1] = productInfo;
			++j;
		}
        SetupAPI.SetupDiDestroyDeviceInfoList(deviceInfo);
        return devices;
    }
    public void close()
	{ 
		try
		{
			if (mHandle != null && !mHandle.IsInvalid)
			{
                //Kernel32.PurgeComm(handle, Kernel32.PURGE_TXABORT | Kernel32.PURGE_RXABORT);
                //Kernel32.CloseHandle(handle);
                mHandle.Close();
                mHandle = null;
				logInfo("设备已关闭");
			}
            mDeviceConnected = false;
		}
		catch(Exception e)
		{
			logInfo("exception : " + e.Message);
		}
    }
    public bool write(byte[] data)  
    {
        if (data.Length >= capabilities.OutputReportByteLength)
		{
			return false;
		}
		uint writeCount = 0;
		return Kernel32.WriteFile(mHandle, data, (uint)data.Length, ref writeCount, IntPtr.Zero);
	}
    public int read(ref byte[] data, int expectCount = 0)
    {
		if (data.Length < capabilities.InputReportByteLength)
		{
			return 0;
		}
		uint readCount = 0;
		Kernel32.ReadFile(mHandle, data, (uint)data.Length, ref readCount, IntPtr.Zero);
		// 如果读取的数量和期望的数量不一致,则丢弃数据
		if(expectCount > 0 && expectCount != readCount)
		{
			return 0;
		}
		return (int)readCount;
	}
}