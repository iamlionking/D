using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace Decatur
{
    #region Enumerations
    public enum DecaturOperation
    {
        Idle,
        DownloadToFile,
        DownloadRange,
        UploadFromFile, 	// dae0 : 20220510 :
        DownloadRangeAsync,
        RangeAsync
    }

    public enum DecaturResult
	{
        Success,
        IndexOutOfRange,
        InvalidDeviceHandle,
        InvalidParameter,
        Timeout,
        DeviceNotFound,
        DeviceBusy,
        GeneralCommError,
        CommandNotAvailable,
        MonitorNotConnected,
        InternalError,
        UnknownError
	}

    public enum DecaturMonitorType
    {
        None,
        SOLO,
        DUET_4GB,
        DUET_8GB,
        ADX_4GB,
        DM500
    }

    #endregion

    #region DLL
    class DLL
    {
        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void DecaturComm_DllVersion(
            [Out] StringBuilder version,
            [In] int version_capacity
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_ScanForDevices(
            [In] int timeout_multiplier,
            [Out] out int num_devices
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_GetDeviceHandle(
            [In] int index,
            [Out] out IntPtr device
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_GetDataLength(
            [In] IntPtr handle,
            [Out] out int data_length
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_GetPageCount(
            [In] IntPtr handle,
            [Out] out int page_count
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_BeginDownload(
            [In] IntPtr handle,
            [In] uint start_page,
            [In] uint page_count,
            [In] bool retain_current_page
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_DownloadData(
            [In] IntPtr handle,
            [Out] byte[] buffer,
            [In] uint buffer_length,
            [Out] out ulong length_received
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_EraseFlash(
            [In] IntPtr handle
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_MonitorPresent(
            [In] IntPtr handle,
            [Out] out bool present
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_MonitorType(
            [In] IntPtr handle,
            [Out] out byte mtype
            );

        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_GetRevision(
            [In] IntPtr handle,
            [Out] out int revision_number
            );

        /*
        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int DecaturComm_GetRevision(
            [In] IntPtr handle,
            [Out] StringBuilder buffer,
            [In] int buffer_length
            );
        */
        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int DecaturComm_GetDeviceSerialNumber(
            [In] IntPtr handle,
            [Out] StringBuilder buffer,
            [In] int buffer_length
            );
        // dae0 : 20220613 :
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int DecaturComm_GetMonitorFWVersion(
            [In] IntPtr handle,
            [Out] StringBuilder buffer,
            [In] int buffer_length
            );
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int DecaturComm_GetMonitorSerialNumber(
            [In] IntPtr handle,
            [Out] StringBuilder buffer,
            [In] int buffer_length
            );

        // dae0 : 20220615 :
        //-------------------------------------------------------------------------------------
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_FW_Upgrade(
            [In] IntPtr handle
            );

        // dae0 : 20220617 :
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_BeginUpload(
            [In] IntPtr handle,
            [In] uint start_page,
            [In] uint page_count,
            [In] bool retain_current_page
            );
        [DllImport("DecaturComm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DecaturComm_UploadData(
            [In] IntPtr handle,
            [In] byte[] buffer,
            [In] uint buffer_length,
            [Out] out ulong length_received
            );

    }
    #endregion

    #region EventArgs
    public class DecaturOperationCompleteEventArgs : EventArgs
    {
        public DecaturOperation Operation { get; private set; }
        public DecaturResult Result { get; private set; }

        public DecaturOperationCompleteEventArgs(DecaturOperation operation, DecaturResult result)
        {
            this.Operation = operation;
            this.Result = result;
        }
    }

    public class DecaturOperationProgressEventArgs : EventArgs
    {
        public DecaturOperation Operation { get; private set; }
        public int PercentComplete { get; private set; }
        public int BytesReceived { get; private set; }
        public int TotalBytes { get; private set; }

        public DecaturOperationProgressEventArgs(DecaturOperation operation, int percentComplete, int bytesReceived, int totalBytes)
        {
            this.Operation = operation;
            this.PercentComplete = percentComplete;
            this.BytesReceived = bytesReceived;
            this.TotalBytes = totalBytes;
        }
    }
    #endregion

    #region Decatur
    public static class Decatur
    {
        [Obsolete]
        public const int PAGE_SIZE = 2048;  // TODO: Remove - call DecaturDevice::MonitorPageSize(DecaturMonitorType)

        public const int STATUS_PAGE = 131071;  // Applies to Micron 4gb flash

        public static string Version
        {
            get
            {
                StringBuilder version = new StringBuilder(32);
                DLL.DecaturComm_DllVersion(version, 32);

                return version.ToString();
            }
        }

        public static List<DecaturDevice> GetConnectedDevices()
        {
            IntPtr handle;
            var devices = new List<DecaturDevice>();
            int count;
            int result = DLL.DecaturComm_ScanForDevices(1, out count);

            if (result == 0)
            {
                for(int index = 0;index < count;index++) {
                    result = DLL.DecaturComm_GetDeviceHandle(index, out handle);
                    devices.Add(new DecaturDevice(handle));
                }
            }

            return devices;
        }

        public static DecaturResult GetResult(int resultCode)
        {
            DecaturResult result = DecaturResult.UnknownError;

            switch (resultCode)
            {
                case 0:
                    result = DecaturResult.Success;
                    break;

                case 1002:
                    result = DecaturResult.IndexOutOfRange;
                    break;

                case 1003:
                    result = DecaturResult.InvalidDeviceHandle;
                    break;

                case 1004:
                    result = DecaturResult.InvalidParameter;
                    break;

                case 1006:
                    result = DecaturResult.Timeout;
                    break;

                case 1010:
                    result = DecaturResult.DeviceNotFound;
                    break;

                case 1011:
                    result = DecaturResult.DeviceBusy;
                    break;

                case 1012:
                    result = DecaturResult.GeneralCommError;
                    break;

                case 1013:
                    result = DecaturResult.CommandNotAvailable;
                    break;

                case 1014:
                    result = DecaturResult.MonitorNotConnected;
                    break;

                case 1999:
                    result = DecaturResult.InternalError;
                    break;

                default:
                    result = DecaturResult.UnknownError;
                    break;
            }

            return result;
        }
    }
    #endregion

    #region DecaturDevice
#endregion
}
