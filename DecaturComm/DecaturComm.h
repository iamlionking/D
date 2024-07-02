#pragma once
/*! @file DecaturComm.h
Describes the "C" level interface to the communication and device identification
system for Decatur ECG devices.
*/
// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DECATURCOMM_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DECATURCOMM_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DECATURCOMM_EXPORTS
//! Defined when compiling the DLL
#define DECATURCOMM_API __declspec(dllexport)
#else
//! Defined when including this file in client code
#define DECATURCOMM_API __declspec(dllimport)
#endif

//! No error
#define DCE_SUCCESS							0

//! Index out of range
#define DCE_INDEX_OUT_OF_RANGE				1002

//! Invalid device handle
#define DCE_INVALID_DEVICE_HANDLE			1003

//! A parameter was invalid
#define DCE_INVALID_PARAMETER				1004

//! Invalid commication handle
#define DCE_INVALID_CONNECTION_HANDLE		1005

//! Timeout reading ECG
#define DCE_TIMEOUT							1006

//! The device is not answering. Probably off or out of range.
#define DCE_DEVICE_NOT_FOUND				1010

//! The device is busy.
#define DCE_DEVICE_BUSY						1011

//! General communication problem
#define DCE_GENERAL_COMM_ERROR				1012

//! Command not available at this time
#define DCE_CMD_NOT_AVAILABLE				1013

//! Monitor is not connected
#define DCE_MONITOR_NOT_CONNECTED			1014

/*! An internal error occured

In general, an error of this type means that the state of the communication
system is not known and following results are undefined.
*/
#define DCE_INTERNAL_ERROR					1999

#define HRESULT_TIMEOUT						0x80070079

#define DLL_VERSION							"0.11.0.0"

//! Type used for a Decatur ECG device handle
typedef void* DecaturDeviceHandle;

//! Type used for a connection to a Decatur ECG device
typedef void* DecaturConnectionHandle;

extern "C"
{
	/*! Returns the DLL version

	@ingroup MgmtFunc

	@param[out]	buffer			The buffer where the text will be placed
	@param[in]	buffer_length	The length in bytes of buffer
	*/
	DECATURCOMM_API void __stdcall DecaturComm_DllVersion(char* buffer, int buffer_length);

	/*! Scan for Decatur ECG devices

	@ingroup MgmtFunc

	This function will block for timeout_multiplier*1.28 seconds.  The timeout_multiplier
	value must be >=0 and <49.  If the timeout multiplier is 0, no inquiry is performed and
	only previously known devices will be returned.

	@param[in]	timeout_multiplier	Number of 1.28 second intervals to scan.
	@param[out]	num_devices			Pointer where the number of devices will be stored.

	@retval DCE_SUCCESS						The function worked
	@retval DCE_SOCKET_INITIALIZATION_ERROR	The operating system services required are not working.
	@retval DCE_NO_LOCAL_RADIOS				No local Bluetooth radio was found.
	@retval DCE_INVALID_PARAMETER			num_devices was NULL or timeout_multiplier is out of bounds.
	*/
	DECATURCOMM_API int __stdcall DecaturComm_ScanForDevices(int timeout_multiplier, int* num_devices);

	/*! Get a handle to the Decatur ECG device

	The handle is valid until another call to DecaturComm_ScanForDevices().

	@ingroup MgmtFunc

	@param[in] index	Index to the requested device.  Must be in 0 <= index < num_devices where
	num_devices was returned by the DecaturComm_ScanForDevices() function.
	@param[out] handle	A pointer to a DecaturDeviceHandle where the result will be stored.

	@retval DCE_SUCCESS						The function worked
	@retval DCE_SOCKET_INITIALIZATION_ERROR	The operating system services required are not working.
	@retval DCE_INDEX_OUT_OF_RANGE			The index specified was outside the bounds of legality.
	@retval DCE_INVALID_PARAMETER			handle was NULL
	*/
	DECATURCOMM_API int __stdcall DecaturComm_GetDeviceHandle(int index, DecaturDeviceHandle* handle);

	DECATURCOMM_API int __stdcall DecaturComm_GetDataLength(DecaturDeviceHandle handle, int* data_length);

	DECATURCOMM_API int __stdcall DecaturComm_GetPageCount(DecaturDeviceHandle handle, int* page_count);

	//DECATURCOMM_API int __stdcall DecaturComm_BeginDownload(DecaturDeviceHandle handle);
	DECATURCOMM_API int __stdcall DecaturComm_BeginDownload(DecaturDeviceHandle handle, unsigned int start_page, unsigned int page_count, bool retain_current_page);

	DECATURCOMM_API int __stdcall DecaturComm_DownloadData(DecaturDeviceHandle handle, uint8_t* buffer, unsigned int buffer_length, unsigned long* length_received);

	DECATURCOMM_API int __stdcall DecaturComm_EraseFlash(DecaturDeviceHandle handle);

	DECATURCOMM_API int __stdcall DecaturComm_MonitorPresent(DecaturDeviceHandle handle, bool* present);
	
	DECATURCOMM_API int __stdcall DecaturComm_MonitorType(DecaturDeviceHandle handle, unsigned char* mtype);

	/*! Gets the device's revision information

	The revision string comes in the following form:
	@code
	app:<app revision>,boot:<boot revision>,bt:<bluetooth revision>,hw:<hardware revision>
	@endcode

	Each revision piece is maximally 27 characters so the total string is a maximum of 126 characters.

	@ingroup ConnFunc

	@param[in] handle		The connection handle
	@param[out] buffer		A pointer to where to put the revision.
	@param[in] buffer_size	Size of buffer.

	@retval DCE_SUCCESS						Everything is working
	@retval DCE_SOCKET_INITIALIZATION_ERROR	The operating system services required are not working.
	@retval DCE_INVALID_CONNECTION_HANDLE	The handle was not a valid connection handle.
	@retval DCE_INVALID_PARAMETER			\p buffer was NULL or buffer_size<=0
	@retval DCE_CMD_NOT_AVAILABLE			The device is busy with another command

	@see DecaturComm_OpenConnection()
	*/
	//DECATURCOMM_API int __stdcall DecaturComm_GetRevision(DecaturConnectionHandle handle, char* buffer, int buffer_size);
	DECATURCOMM_API int __stdcall DecaturComm_GetRevision(DecaturDeviceHandle handle, int* revision_number);

	/*! Gets the device serial number

	@ingroup DevFunc

	@param[in]	handle			The device handle
	@param[out] buffer			Pointer to buffer to store the name.
	@param[in]	buffer_length	Length of the name buffer

	@retval DCE_SUCCESS						The function worked
	@retval DCE_INVALID_DEVICE_HANDLE		handle is not a valid handle
	@retval DCE_INVALID_PARAMETER			name was NULL or buffer_len <= 0
	*/
	DECATURCOMM_API int __stdcall DecaturComm_GetDeviceSerialNumber(DecaturDeviceHandle handle, char* buffer, int buffer_length);
	// dae0 : 20220613 :
	DECATURCOMM_API int __stdcall DecaturComm_GetMonitorFWVersion(DecaturDeviceHandle handle, char* buffer, int buffer_length);
	DECATURCOMM_API int __stdcall DecaturComm_GetMonitorSerialNumber(DecaturDeviceHandle handle, char* buffer, int buffer_length);
	// dae0 : 20220615 :
	DECATURCOMM_API int __stdcall DecaturComm_FW_Upgrade(DecaturDeviceHandle handle);
	// dae0 : 20220617 :
	DECATURCOMM_API int __stdcall DecaturComm_BeginUpload(DecaturDeviceHandle handle, unsigned int start_page, unsigned int page_count, bool retain_current_page);
	DECATURCOMM_API int __stdcall DecaturComm_UploadData(DecaturDeviceHandle handle, uint8_t* buffer, unsigned int buffer_length, unsigned long* length_received);
}
