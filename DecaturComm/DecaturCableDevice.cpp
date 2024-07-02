#include "stdafx.h"
#include "DecaturCableDevice.h"

#include <SetupAPI.h>

DecaturCableDevice::DecaturCableDevice(LPTSTR DevicePath) : m_DataLength(0), m_PageCount(0), m_RevisionNumber(0), m_MonitorWasPresent(false),
m_monitorType(0)
{
	Open(DevicePath);
}


DecaturCableDevice::~DecaturCableDevice()
{
}


HRESULT
DecaturCableDevice::RetrieveDevicePath(
_Out_bytecap_(BufLen) LPTSTR DevicePath,
_In_                  ULONG  BufLen,
_In_				  uint8_t DeviceIndex,
_Out_opt_             PBOOL  FailureDeviceNotFound
)
{
	BOOL                             bResult = FALSE;
	HDEVINFO                         deviceInfo;
	SP_DEVICE_INTERFACE_DATA         interfaceData;
	PSP_DEVICE_INTERFACE_DETAIL_DATA detailData = NULL;
	ULONG                            length;
	ULONG                            requiredLength = 0;
	HRESULT                          hr;

	if (NULL != FailureDeviceNotFound) {

		*FailureDeviceNotFound = FALSE;
	}

	//
	// Enumerate all devices exposing the interface
	//
	deviceInfo = SetupDiGetClassDevs(&g_DecaturDeviceGUID, //현재 시스템에 장착되어있는 모든 Device 검색
		NULL,
		NULL,
		DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

	if (deviceInfo == INVALID_HANDLE_VALUE) { //디바이스 반환 실패시

		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}

	interfaceData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA); //구조체의 크기(바이트 단위)를 입력받음

	//
	// Get the device indicated by DeviceIndex from the result set
	//
	bResult = SetupDiEnumDeviceInterfaces(deviceInfo, //디바이스 정보 집합에 포함된 디바이스 인터페이스를 열거
		NULL,
		&g_DecaturDeviceGUID,
		DeviceIndex,
		&interfaceData);

	if (FALSE == bResult) {

		//
		// We would see this error if there are less devices than DeviceIndex
		//
		if (ERROR_NO_MORE_ITEMS == GetLastError() &&
			NULL != FailureDeviceNotFound) {

			*FailureDeviceNotFound = TRUE;
		}

		hr = HRESULT_FROM_WIN32(GetLastError());
		SetupDiDestroyDeviceInfoList(deviceInfo); //디바이스 정보 집합을 삭제하고 연결된 모든 메모리를 해제
		return hr;
	}

	//
	// Get the size of the path string
	// We expect to get a failure with insufficient buffer
	//
	bResult = SetupDiGetDeviceInterfaceDetail(deviceInfo, //디바이스 인터페이스에 대한 세부 정보의 크기를 반환
		&interfaceData,
		NULL,
		0,
		&requiredLength,
		NULL);
	if (FALSE == bResult && ERROR_INSUFFICIENT_BUFFER != GetLastError()) {

		hr = HRESULT_FROM_WIN32(GetLastError());
		SetupDiDestroyDeviceInfoList(deviceInfo);
		return hr;
	}

	//
	// Allocate temporary space for SetupDi structure
	//
	detailData = (PSP_DEVICE_INTERFACE_DETAIL_DATA)
		LocalAlloc(LMEM_FIXED, requiredLength);

	if (NULL == detailData)
	{
		hr = E_OUTOFMEMORY;
		SetupDiDestroyDeviceInfoList(deviceInfo);
		return hr;
	}

	detailData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
	length = requiredLength;

	//
	// Get the interface's path string
	//
	bResult = SetupDiGetDeviceInterfaceDetail(deviceInfo, //디바이스 인터페이스에 대한 세부 정보를 반환 
		&interfaceData,
		detailData,
		length,
		&requiredLength,
		NULL);

	if (FALSE == bResult)
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		LocalFree(detailData);
		SetupDiDestroyDeviceInfoList(deviceInfo);
		return hr;
	}

	//
	// Give path to the caller. SetupDiGetDeviceInterfaceDetail ensured
	// DevicePath is NULL-terminated.
	//
	hr = StringCbCopy(DevicePath,
		BufLen,
		detailData->DevicePath); //구조체 포인터가 가리키는 주소에 데이터를 할당, 원본 문자열 detailData의 값을 DevicePath로 할당

	LocalFree(detailData); //메모리 개체를 해제
	SetupDiDestroyDeviceInfoList(deviceInfo);//지정된 로컬 메모리 개체를 해제하고 해당 핸들을 무효화

	return hr;
}


HRESULT DecaturCableDevice::RetrieveDevicePaths(std::vector<std::unique_ptr<TCHAR[]>> *DevicePaths)
{
	TCHAR path[MAX_PATH];
	HRESULT hr = ERROR_SUCCESS;
	BOOL deviceNotFound = FALSE;
	uint8_t index = 0;

	while (!deviceNotFound && SUCCEEDED(hr)) {
		hr = DecaturCableDevice::RetrieveDevicePath(path, sizeof(path), index, &deviceNotFound);
		
		if (SUCCEEDED(hr)) {
			std::unique_ptr<TCHAR[]> string = std::unique_ptr<TCHAR[]>(new TCHAR[MAX_PATH]);
			_tcscpy_s(string.get(), MAX_PATH, path);
			DevicePaths->push_back(std::move(string));//Vector(DevicePaths)의 마지막에 새로운 원소를 추가

			index++;
		}
	}

	if (deviceNotFound) {
		hr = ERROR_SUCCESS;
	}

	return hr;
}

uint32_t DecaturCableDevice::GetDataLength()
{
	return m_DataLength;
}

uint32_t DecaturCableDevice::GetPageCount()
{
	return m_PageCount;
}

HRESULT DecaturCableDevice::DetermineDataLength()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	uint32_t dataLength = 0;
	ULONG dataTransfered = 0;

	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));

	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_DATA_LENGTH;
	packet.Length = sizeof(dataLength);
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&dataLength, sizeof(dataLength), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	} else {
		m_DataLength = dataLength;
	}

	return hr;
}

HRESULT DecaturCableDevice::DeterminePageCount()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	uint32_t pageCount = 0;
	ULONG dataTransfered = 0;

	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));

	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_PAGE_COUNT;
	packet.Length = sizeof(pageCount);
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&pageCount, sizeof(pageCount), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	} else {
		m_PageCount = pageCount;
	}

	return hr;
}

HRESULT DecaturCableDevice::DetermineSerialNumber()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	uint32_t dataLength = 0;
	ULONG dataTransfered = 0;
	char buffer[SERIAL_NUMBER_LENGTH + 1];

	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));

	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_GET_SERIAL;
	packet.Length = SERIAL_NUMBER_LENGTH;
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)buffer, sizeof(buffer), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	} else {
		buffer[SERIAL_NUMBER_LENGTH] = '\0'; //null 문자 삽입(널문자는 문자열의 끝을 판단)
		m_SerialNumber.assign(buffer); //buffer의 값을 m_SerialNumber에 할당
	}

	return hr;
}

// dae0 : 20220613
// [-------------------------------------------------------
HRESULT DecaturCableDevice::DetermineMonitorFWVersion()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	uint32_t dataLength = 0;
	ULONG dataTransfered = 0;
	char buffer[FW_VERSION_LENGTH + 1] = "--------";

	// if (m_RevisionNumber > 19) // C300
	if (GetCableType(19) == CABLE_TYPE_C350) // dae0 : 20220701 : C350
	{
      	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));
      
      	packet.RequestType = 0xC0;
      	packet.Request = CONTROL_REQUEST_GET_FW_VERSION;
      	packet.Length = FW_VERSION_LENGTH;
      	packet.Index = 0;
      	packet.Value = 0;
      
      	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)buffer, sizeof(buffer), &dataTransfered, NULL);
      
      	if (!bResult) {
      		hr = HRESULT_FROM_WIN32(GetLastError());
      	}
      	else {
      		buffer[FW_VERSION_LENGTH] = '\0';
      		m_MonitorFWVersion.assign(buffer);
      	}
	}
	else
	{
  	   buffer[FW_VERSION_LENGTH] = '\0';
	   m_MonitorFWVersion.assign(buffer);
	}
	return hr;
}

HRESULT DecaturCableDevice::DetermineMonitorSerialNumber()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	uint32_t dataLength = 0;
	ULONG dataTransfered = 0;
	char buffer[MONITOR_SERIAL_NUMBER_LENGTH + 1] = "--------";

	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));

	// if (m_RevisionNumber > 19) // C300
	if (GetCableType(19) == CABLE_TYPE_C350) // dae0 : 20220701 : C350
	{
      	packet.RequestType = 0xC0;
      	packet.Request = CONTROL_REQUEST_GET_MONITOR_SERIAL;
      	packet.Length = MONITOR_SERIAL_NUMBER_LENGTH;
      	packet.Index = 0;
      	packet.Value = 0;
      
      	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)buffer, sizeof(buffer), &dataTransfered, NULL);
      
      	if (!bResult) {
      		hr = HRESULT_FROM_WIN32(GetLastError());
      	}
      	else {
      		buffer[MONITOR_SERIAL_NUMBER_LENGTH] = '\0'; //시리얼 마지막에 null 문자 삽입, c언어 stting 형식의 젤 마지막에는 null 문자 필수
      		m_MonitorSerialNumber.assign(buffer);
      	}
	}
	else
	{
		buffer[MONITOR_SERIAL_NUMBER_LENGTH] = '\0';
		m_MonitorSerialNumber.assign(buffer);
	}
	return hr;
}
// -------------------------------------------------------]

HRESULT DecaturCableDevice::DetermineRevisionNumber()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	uint16_t revisionNumber = 0;
	ULONG dataTransfered = 0;

	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET)); // 메모리 영역을 0X00으로 채우는 매크로이다. 함수 X


	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_GET_REVISION;
	packet.Length = sizeof(revisionNumber);
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&revisionNumber, sizeof(revisionNumber), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	} else {
		m_RevisionNumber = revisionNumber;
	}

	return hr;
}

HRESULT DecaturCableDevice::MonitorPresent(bool* present)
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	ULONG dataTransfered = 0;

	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));

	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_MONITOR_PRESENT;
	packet.Length = sizeof(bool);
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)present, sizeof(bool), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
		*present = false;
	}

	if (*present != m_MonitorWasPresent) {
		RefreshMonitorData();
		m_MonitorWasPresent = *present;
	}

	return hr;
}


HRESULT DecaturCableDevice::DetermineMonitorType()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	ULONG dataTransfered = 0;

	// if (m_RevisionNumber >= 19) // dae0 : 20220518 :
	if (GetCableType(0) == CABLE_TYPE_C350) // dae0 : 20220701 : C350
	{
		ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));

		packet.RequestType = 0xC0;
		packet.Request = CONTROL_REQUEST_MONITOR_TYPE;
		packet.Length = sizeof(uint8_t);
		packet.Index = 0;
		packet.Value = 0;

		bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&m_monitorType, sizeof(uint8_t), &dataTransfered, NULL);

		if (!bResult) {
			hr = HRESULT_FROM_WIN32(GetLastError());
			m_monitorType = 0;
		}
	}
	else // old version support C300 only.
		m_monitorType = 1; 

	return hr;
}



HRESULT DecaturCableDevice::InitiateDownload(uint32_t start_page, uint32_t page_count, bool retain_current_page)
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	InitiateDownloadPacket download_packet;
	uint8_t request_result;
	ULONG dataTransfered = 0;

	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));
	ZeroMemory(&download_packet, sizeof(download_packet));

	packet.RequestType = 0x40;
	packet.Request = CONTROL_REQUEST_DOWNLOAD;
	packet.Length = sizeof(download_packet);
	packet.Index = 0;
	packet.Value = 0;

	download_packet.StartPage = start_page;
	download_packet.PageCount = page_count;
	download_packet.RetainCurrentPage = retain_current_page;

	// Clear any partial buffers from previous transfers
	bResult = WinUsb_FlushPipe(m_DeviceHandle, USB_PIPE_BULK_IN_1);

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&download_packet, sizeof(download_packet), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}
	   	
	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));
	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_DOWNLOAD_RESULT;
	packet.Length = sizeof(request_result);
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&request_result, sizeof(request_result), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	} else if (request_result != CONTROL_RESULT_SUCCESS) {
		hr = E_NOT_VALID_STATE;
	}

	return hr;
}

HRESULT DecaturCableDevice::EraseFlash()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	uint8_t request_result;
	ULONG dataTransfered = 0;

	// New: Extend timeout since larger flash takes longer to erase
	ULONG timeout = USB_READ_TIMEOUT_MS * 3;
	bResult = WinUsb_SetPipePolicy(m_DeviceHandle, 0x00 /*control ep*/, PIPE_TRANSFER_TIMEOUT, sizeof(timeout), (void*)&timeout);


	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));

	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_ERASE;
	packet.Length = sizeof(request_result);
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&request_result, sizeof(request_result), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	} else if (request_result == CONTROL_RESULT_NO_MONITOR) {
		hr = E_NOT_VALID_STATE;
	}


	// Restore default
	timeout = USB_READ_TIMEOUT_MS;
	bResult = WinUsb_SetPipePolicy(m_DeviceHandle, 0x00 /*control ep*/, PIPE_TRANSFER_TIMEOUT, sizeof(timeout), (void*)&timeout);



	return hr;
}

HRESULT DecaturCableDevice::Open(LPTSTR DevicePath)
{
	HRESULT hr = S_OK;
	BOOL    bResult;

	m_HandlesOpen = FALSE;

	_tcscpy_s(m_DevicePath, sizeof(m_DevicePath), DevicePath);
	//strcpy_s(m_DevicePath, sizeof(m_DevicePath), DevicePath);

	m_FileHandle = CreateFile(m_DevicePath,
		GENERIC_WRITE | GENERIC_READ,
		FILE_SHARE_WRITE | FILE_SHARE_READ,
		NULL,
		OPEN_EXISTING,
		FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED,
		NULL);

	if (INVALID_HANDLE_VALUE == m_FileHandle) {
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}

	bResult = WinUsb_Initialize(m_FileHandle,
		&m_DeviceHandle);

	if (FALSE == bResult) {

		hr = HRESULT_FROM_WIN32(GetLastError());
		CloseHandle(m_FileHandle);
		return hr;
	}

#if defined (CABLE_C400)	// dae0 : 20220506 :
	//bResult = WinUsb_SetCurrentAlternateSetting(m_DeviceHandle, 1);

	//if (FALSE == bResult) {
	//	hr = HRESULT_FROM_WIN32(GetLastError());
	//	WinUsb_Free(m_DeviceHandle);
	//	CloseHandle(m_FileHandle);
	//	return hr;
	//}
#else
	bResult = WinUsb_SetCurrentAlternateSetting(m_DeviceHandle, 1);

	if (FALSE == bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
		WinUsb_Free(m_DeviceHandle);
		CloseHandle(m_FileHandle);
		return hr;
	}
#endif

	ULONG timeout = USB_READ_TIMEOUT_MS;
	bResult = WinUsb_SetPipePolicy(m_DeviceHandle, USB_PIPE_BULK_IN_1, PIPE_TRANSFER_TIMEOUT, sizeof(timeout), (void*)&timeout);

	if (FALSE == bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
		WinUsb_Free(m_DeviceHandle);
		CloseHandle(m_FileHandle);
		return hr;
	}

	m_HandlesOpen = TRUE;

#if defined (CABLE_C400)	// dae0 : 20220506 :
	WINUSB_PIPE_INFORMATION pipe_info;

	WinUsb_QueryPipe(m_DeviceHandle, 0, 0, &pipe_info);
	m_maximumPacketSize = pipe_info.MaximumPacketSize;
#endif

	DetermineSerialNumber();
	DetermineRevisionNumber();
	
	RefreshMonitorData();

	return hr;
}

HRESULT DecaturCableDevice::DownloadData(uint8_t* buffer, uint32_t buffer_length, unsigned long* length_received)
{
	HRESULT hr = S_OK;
	BOOL    bResult;

	bResult = WinUsb_ReadPipe(m_DeviceHandle, USB_PIPE_BULK_IN_1, buffer, buffer_length, length_received, NULL);

#if defined (CABLE_C400)	// dae0 : 20220506 :
	// Read a ZLP if exactly the right amount of data is rcvd AND it's a multiple of pipe size
	// winusb should be doing this (?) but it doesn't

	if (*length_received == buffer_length && ((buffer_length % m_maximumPacketSize) == 0))
	{
		uint8_t xb[4];
		unsigned long xlen;

		WinUsb_ReadPipe(m_DeviceHandle, USB_PIPE_BULK_IN_1, xb, 4, &xlen, NULL);
	}
#endif
	if (FALSE == bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	}

// RWS: Duet cradle firmware returns the data byte-swapped.
// Fixing it on the cradle would reduce bandwidth, so do it here.

	if (m_monitorType > MONITOR_TYPE_SOLO)// dae0 : 20220506 : duet ...
	{
		//if (m_RevisionNumber >= 19)
		if (GetCableType(0) == CABLE_TYPE_C350) // dae0 : 20220701 : C350
		{	
			uint16_t* ptr = (uint16_t*)buffer;
			for (unsigned long i = 0; i < *length_received / 2; i++)
			{
				*ptr = _byteswap_ushort(*ptr);
				ptr++;
			}
		}
	}

	return hr;
}

// dae0 : 20220617 :
HRESULT DecaturCableDevice::UploadData(uint8_t* buffer, uint32_t buffer_length, unsigned long* length_received)
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	ULONG timeout = USB_READ_TIMEOUT_MS  ;

	// Clear any partial buffers from previous transfers
	//bResult = WinUsb_FlushPipe(m_DeviceHandle, USB_PIPE_BULK_OUT_2);
	bResult = WinUsb_SetPipePolicy(m_DeviceHandle, USB_PIPE_BULK_OUT_2, PIPE_TRANSFER_TIMEOUT, sizeof(timeout), (void*)&timeout);

#if 0
	if (m_monitorType > MONITOR_TYPE_SOLO) // dae0 : 20220627 : duet ...
	{
		//if (m_RevisionNumber >= 19)
		if (GetCableType(0) == CABLE_C350) // dae0 : 20220701 : C350
		{
			uint16_t* ptr = (uint16_t*)buffer;
			for (unsigned long i = 0; i <  buffer_length / 2; i++)
			{
				*ptr = _byteswap_ushort(*ptr);
				ptr++;
			}
		}
	}
#endif

	bResult = WinUsb_WritePipe(m_DeviceHandle, USB_PIPE_BULK_OUT_2, buffer, buffer_length, length_received, NULL);
	
	if (FALSE == bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	}

	return hr;
}

VOID DecaturCableDevice::Close()
{
	if (FALSE == m_HandlesOpen) {

		//
		// Called on an uninitialized DeviceData
		//
		return;
	}

	WinUsb_Free(m_DeviceHandle);
	CloseHandle(m_FileHandle);
	m_HandlesOpen = FALSE;

	return;
}

TCHAR* DecaturCableDevice::GetPath()
{
	return m_DevicePath;
}

std::string DecaturCableDevice::GetSerialNumber()
{
	return m_SerialNumber;
}

// dae0 : 20220613
// [-------------------------------------------------------
std::string DecaturCableDevice::GetMonitorFWVersion()
{
	return m_MonitorFWVersion;
}

std::string DecaturCableDevice::GetMonitorSerialNumber()
{
	return m_MonitorSerialNumber;
}
// -------------------------------------------------------]

// dae0 : 20220701 : get cable type
uint8_t DecaturCableDevice::GetCableType(uint8_t minVer)
{
	uint8_t cable_type = (m_RevisionNumber >> 8) & 0xFF;

	if (minVer == 0) // C350 all version.
	{
		if (m_RevisionNumber == 19) // first C350
			return CABLE_TYPE_C350;

		return  cable_type;
	}

	if (cable_type)
		if (minVer < (m_RevisionNumber & 0xFF))
			return CABLE_TYPE_NONE;
	return  cable_type;
}

uint16_t DecaturCableDevice::GetRevisionNumber()
{
	return m_RevisionNumber;
}

uint8_t DecaturCableDevice::GetMonitorType()
{
	return m_monitorType;
}

HRESULT DecaturCableDevice::RefreshMonitorData()
{
	HRESULT hr = S_OK;

	hr = DetermineDataLength();

	if (hr == S_OK) {
		hr = DeterminePageCount();
		hr = DetermineMonitorType();
		// dae0 : 20220614 :
		if (hr == S_OK)
			hr = DetermineMonitorFWVersion();
		if (hr == S_OK)
			hr = DetermineMonitorSerialNumber();
	}

	return hr;
}

// dae0 : 20220615 : 
HRESULT DecaturCableDevice::FW_Upgrade()
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	uint8_t request_result;
	ULONG dataTransfered = 0;

	// New: Extend timeout since larger flash takes longer to erase
	ULONG timeout = USB_READ_TIMEOUT_MS * 3;
	bResult = WinUsb_SetPipePolicy(m_DeviceHandle, 0x00 /*control ep*/, PIPE_TRANSFER_TIMEOUT, sizeof(timeout), (void*)&timeout);


	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));

	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_CABLE_TEST;
	packet.Length = sizeof(request_result);
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&request_result, sizeof(request_result), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	}
	else if (request_result == CONTROL_RESULT_NO_MONITOR) {
		hr = E_NOT_VALID_STATE;
	}


	// Restore default
	timeout = USB_READ_TIMEOUT_MS;
	bResult = WinUsb_SetPipePolicy(m_DeviceHandle, 0x00 /*control ep*/, PIPE_TRANSFER_TIMEOUT, sizeof(timeout), (void*)&timeout);



	return hr;
}

HRESULT DecaturCableDevice::InitiateUpload(uint32_t start_page, uint32_t page_count, bool retain_current_page)
{
	HRESULT hr = S_OK;
	BOOL    bResult;
	WINUSB_SETUP_PACKET packet;
	InitiateDownloadPacket download_packet;
	uint8_t request_result;
	ULONG dataTransfered = 0;
	ULONG timeout = USB_READ_TIMEOUT_MS * 3;


	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));
	ZeroMemory(&download_packet, sizeof(download_packet));

	packet.RequestType = 0x40;
	packet.Request = CONTROL_REQUEST_BULK_UPLOAD;
	packet.Length = sizeof(download_packet);
	packet.Index = 0;
	packet.Value = 0;

	download_packet.StartPage = start_page;
	download_packet.PageCount = page_count;
	download_packet.RetainCurrentPage = retain_current_page;

	bResult = WinUsb_SetPipePolicy(m_DeviceHandle, 0x00 /*control ep*/, PIPE_TRANSFER_TIMEOUT, sizeof(timeout), (void*)&timeout);
	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&download_packet, sizeof(download_packet), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	}

	ZeroMemory(&packet, sizeof(WINUSB_SETUP_PACKET));
	packet.RequestType = 0xC0;
	packet.Request = CONTROL_REQUEST_UPLOAD_RESULT;
	packet.Length = sizeof(request_result);
	packet.Index = 0;
	packet.Value = 0;

	bResult = WinUsb_ControlTransfer(m_DeviceHandle, packet, (PUCHAR)&request_result, sizeof(request_result), &dataTransfered, NULL);

	if (!bResult) {
		hr = HRESULT_FROM_WIN32(GetLastError());
	}
	else if (request_result != CONTROL_RESULT_SUCCESS) {
		hr = E_NOT_VALID_STATE;
	}

	return hr;
}
