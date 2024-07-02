// DecaturComm.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "dllmain.h"
#include "DecaturComm.h"
#include "DecaturCableDevice.h"

std::vector<DecaturCableDevice> g_ConnectedDevices;

//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
DECATURCOMM_API void __stdcall DecaturComm_DllVersion(char* buffer, int buffer_length)
{
	strncpy_s(buffer, buffer_length, DLL_VERSION, (buffer_length - 1)); //  7);    -- why 7?
}


//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
DECATURCOMM_API int __stdcall DecaturComm_ScanForDevices(int timeout_multiplier, int* num_devices)
{
	std::vector<std::unique_ptr<TCHAR[]>> devicePaths;
	if (num_devices == NULL) return DCE_INVALID_PARAMETER;
	if (timeout_multiplier<0 || timeout_multiplier>48) return DCE_INVALID_PARAMETER;

	DecaturCableDevice::RetrieveDevicePaths(&devicePaths);
	
	for (auto device : g_ConnectedDevices) {
		device.Close();
	}

	g_ConnectedDevices.clear(); //할당 메모리를 그대로이지만, 내용만 0으로 리셋

	for (auto const& path: devicePaths)	{
		g_ConnectedDevices.push_back(DecaturCableDevice(path.get()));
	}

	*num_devices = g_ConnectedDevices.size();

	return DCE_SUCCESS;
}


//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
DECATURCOMM_API int __stdcall DecaturComm_GetDeviceHandle(int index, DecaturDeviceHandle* handle)
{
	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (index < 0 || (size_t)index >= g_ConnectedDevices.size()) return DCE_INVALID_PARAMETER;

	*handle = &g_ConnectedDevices[index];

	return DCE_SUCCESS;
}


//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
DECATURCOMM_API int __stdcall DecaturComm_OpenConnection(ULONGLONG address, DecaturConnectionHandle* handle)
{
	*handle = NULL;
	return DCE_SUCCESS;
}


//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
DECATURCOMM_API int __stdcall DecaturComm_GetDataLength(DecaturDeviceHandle handle, int* data_length)
{
	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (data_length == NULL) return DCE_INVALID_PARAMETER;

	*data_length = ((DecaturCableDevice*)handle)->GetDataLength();

	return DCE_SUCCESS;
}


DECATURCOMM_API int __stdcall DecaturComm_GetPageCount(DecaturDeviceHandle handle, int* page_count)
{
	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (page_count == NULL) return DCE_INVALID_PARAMETER;

	*page_count = ((DecaturCableDevice*)handle)->GetPageCount();

	return DCE_SUCCESS;
}


DECATURCOMM_API int __stdcall DecaturComm_GetDeviceSerialNumber(DecaturDeviceHandle handle, char* buffer, int buffer_length)
{
	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (buffer == NULL) return DCE_INVALID_PARAMETER;
	if (buffer_length < SERIAL_NUMBER_LENGTH) return DCE_INVALID_PARAMETER;

	auto serial = ((DecaturCableDevice*)handle)->GetSerialNumber();

	strcpy_s(buffer, buffer_length, serial.c_str());

	return DCE_SUCCESS;
}

// dae0 : 20220613 :
// [-------------------------------------------------------
DECATURCOMM_API int __stdcall DecaturComm_GetMonitorFWVersion(DecaturDeviceHandle handle, char* buffer, int buffer_length)
{
	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (buffer == NULL) return DCE_INVALID_PARAMETER;
	if (buffer_length < FW_VERSION_LENGTH) return DCE_INVALID_PARAMETER;

	auto fw_version = ((DecaturCableDevice*)handle)->GetMonitorFWVersion();

	strcpy_s(buffer, buffer_length, fw_version.c_str());

	return DCE_SUCCESS;
}

DECATURCOMM_API int __stdcall DecaturComm_GetMonitorSerialNumber(DecaturDeviceHandle handle, char* buffer, int buffer_length)
{
	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (buffer == NULL) return DCE_INVALID_PARAMETER;
	if (buffer_length < MONITOR_SERIAL_NUMBER_LENGTH) return DCE_INVALID_PARAMETER;

	auto monitor_serial = ((DecaturCableDevice*)handle)->GetMonitorSerialNumber();

	strcpy_s(buffer, buffer_length, monitor_serial.c_str()); //c_str()는 string 객체를 *char로 변환

	return DCE_SUCCESS;
}
// -------------------------------------------------------]

DECATURCOMM_API int __stdcall DecaturComm_BeginDownload(DecaturDeviceHandle handle, unsigned int start_page, unsigned int page_count, bool retain_current_page)
{
	HRESULT result;

	if (handle == NULL) return DCE_INVALID_PARAMETER;

	result = ((DecaturCableDevice*)handle)->InitiateDownload(start_page, page_count, retain_current_page);

	if (SUCCEEDED(result)) {
		return DCE_SUCCESS;
	}

	if (result == E_NOT_VALID_STATE) {
		return DCE_MONITOR_NOT_CONNECTED;
	}

	return DCE_GENERAL_COMM_ERROR;
}


DECATURCOMM_API int __stdcall DecaturComm_DownloadData(DecaturDeviceHandle handle, uint8_t* buffer, unsigned int buffer_length, unsigned long* length_received)
{
	HRESULT result;

	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (buffer == NULL) return DCE_INVALID_PARAMETER;

	result = ((DecaturCableDevice*)handle)->DownloadData(buffer, buffer_length, length_received);

	if (SUCCEEDED(result)) {
		return DCE_SUCCESS;
	}
	else if (result == HRESULT_TIMEOUT) {
		return DCE_TIMEOUT;
	}

	return DCE_GENERAL_COMM_ERROR;
}


DECATURCOMM_API int __stdcall DecaturComm_EraseFlash(DecaturDeviceHandle handle)
{
	HRESULT result;

	if (handle == NULL) return DCE_INVALID_PARAMETER;

	result = ((DecaturCableDevice*)handle)->EraseFlash();

	if (SUCCEEDED(result)) {
		return DCE_SUCCESS;
	}

	if (result == E_NOT_VALID_STATE) {
		return DCE_MONITOR_NOT_CONNECTED;
	}

	return DCE_GENERAL_COMM_ERROR;
}


DECATURCOMM_API int __stdcall DecaturComm_MonitorPresent(DecaturDeviceHandle handle, bool* present)
{
	HRESULT result;

	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (present == NULL) return DCE_INVALID_PARAMETER;

	result = ((DecaturCableDevice*)handle)->MonitorPresent(present);

	if (SUCCEEDED(result)) {
		return DCE_SUCCESS;
	}
	else if (result == HRESULT_TIMEOUT) {
		return DCE_TIMEOUT;
	}

	return DCE_GENERAL_COMM_ERROR;
}


DECATURCOMM_API int __stdcall DecaturComm_MonitorType(DecaturDeviceHandle handle, unsigned char* mtype)
{
	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (mtype == NULL) return DCE_INVALID_PARAMETER;

	*mtype = ((DecaturCableDevice*)handle)->GetMonitorType();

	return DCE_SUCCESS;
}

//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
DECATURCOMM_API int __stdcall DecaturComm_GetRevision(DecaturDeviceHandle handle, int* revision_number)
{
	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (revision_number == NULL) return DCE_INVALID_PARAMETER;

	*revision_number = ((DecaturCableDevice*)handle)->GetRevisionNumber();

	return DCE_SUCCESS;
}

// dae0 : 20220615 :
DECATURCOMM_API int __stdcall DecaturComm_FW_Upgrade(DecaturDeviceHandle handle)
{
	HRESULT result;

	if (handle == NULL) return DCE_INVALID_PARAMETER;

	result = ((DecaturCableDevice*)handle)->FW_Upgrade();

	if (SUCCEEDED(result)) {
		return DCE_SUCCESS;
	}

	if (result == E_NOT_VALID_STATE) {
		return DCE_MONITOR_NOT_CONNECTED;
	}

	return DCE_GENERAL_COMM_ERROR;
}

// dae0 : 20220617 :
DECATURCOMM_API int __stdcall DecaturComm_BeginUpload (DecaturDeviceHandle handle, unsigned int start_page, unsigned int page_count, bool retain_current_page)
{
	HRESULT result;
	
	if (handle == NULL) return DCE_INVALID_PARAMETER;

	result = ((DecaturCableDevice*)handle)->InitiateUpload(start_page, page_count, retain_current_page);

	if (SUCCEEDED(result)) {
		return DCE_SUCCESS;
	}

	if (result == E_NOT_VALID_STATE) {
		return DCE_MONITOR_NOT_CONNECTED;
	}

	return DCE_GENERAL_COMM_ERROR;
}


DECATURCOMM_API int __stdcall DecaturComm_UploadData(DecaturDeviceHandle handle, uint8_t* buffer, unsigned int buffer_length, unsigned long* length_received)
{
	HRESULT result;

	if (handle == NULL) return DCE_INVALID_PARAMETER;
	if (buffer == NULL) return DCE_INVALID_PARAMETER;

	result = ((DecaturCableDevice*)handle)->UploadData(buffer, buffer_length, length_received);

	if (SUCCEEDED(result)) {
		return DCE_SUCCESS;
	}
	else if (result == HRESULT_TIMEOUT) {
		return DCE_TIMEOUT;
	}

	return DCE_GENERAL_COMM_ERROR;
}
