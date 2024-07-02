#pragma once

#if 0 // for C400
DEFINE_GUID(g_DecaturDeviceGUID, 0x263BE2B3, 0x3A27, 0x486E, 0x84, 0xBC, 0xFF, 0xFF, 0x74, 0x7C, 0x13, 0xA9);
// Changed a couple bytes to FF for Duet version
#else
DEFINE_GUID(g_DecaturDeviceGUID, 0x263BE2B3, 0x3A27, 0x486E, 0x84, 0xBC, 0xA6, 0xA2, 0x74, 0x7C, 0x13, 0xA9);
#endif

#define CONTROL_REQUEST_DOWNLOAD			0x00
#define CONTROL_REQUEST_DATA_LENGTH			0x01
#define CONTROL_REQUEST_ERASE				0x02
#define CONTROL_REQUEST_MONITOR_PRESENT		0x03
#define CONTROL_REQUEST_GET_SERIAL			0x04
#define CONTROL_REQUEST_PAGE_COUNT			0x05
#define CONTROL_REQUEST_DOWNLOAD_RESULT		0x06
#define CONTROL_REQUEST_GET_REVISION		0x07
#define CONTROL_REQUEST_MONITOR_TYPE		0x08

// dae0 : 20220613 :
#define CONTROL_REQUEST_GET_MONITOR_SERIAL	0x09
#define CONTROL_REQUEST_GET_FW_VERSION		0x0A
#define CONTROL_REQUEST_CABLE_TEST				0x0B
#define CONTROL_REQUEST_BULK_UPLOAD			0x0C
#define CONTROL_REQUEST_UPLOAD_RESULT		0x0D

#define CONTROL_RESULT_SUCCESS				0x00
#define CONTROL_RESULT_NO_MONITOR			0x01
#define CONTROL_RESULT_INVALID_PAGE			0x02
#define CONTROL_RESULT_INVALID_LENGTH		0x03
#define CONTROL_RESULT_INVALID_PAYLOAD		0x04

#define SERIAL_NUMBER_LENGTH				32

// dae0 : 20220614 :
#define FW_VERSION_LENGTH							16
#define MONITOR_SERIAL_NUMBER_LENGTH	16

#define USB_READ_TIMEOUT_MS					5000
#define USB_PIPE_BULK_IN_1					0x81
#define USB_PIPE_BULK_OUT_2					0x02

// dae0 : 20220506 : add monitor type 
// This matches enum in the Windows DLL
#define MONITOR_TYPE_NONE   (0)
#define MONITOR_TYPE_SOLO   (1)
#define MONITOR_TYPE_DUET_4GB   (2)
#define MONITOR_TYPE_DUET_8GB   (3)
#define ADX_4GB    (4)

// dae0 : 20220701 : for cable type
#define CABLE_TYPE_NONE		-1
#define CABLE_TYPE_C300		0
#define CABLE_TYPE_C350		1
// #define CABLE_TYPE_C400		2 // not support

#pragma pack(1)
struct InitiateDownloadPacket {
	uint32_t StartPage;
	uint32_t PageCount;
	bool RetainCurrentPage;
};
#pragma pack()

class DecaturCableDevice
{
private:
	BOOL						m_HandlesOpen;
	HANDLE						m_FileHandle;
	WINUSB_INTERFACE_HANDLE		m_DeviceHandle;
	TCHAR						m_DevicePath[MAX_PATH];
	USB_DEVICE_DESCRIPTOR		m_DeviceDescriptor;
	USB_INTERFACE_DESCRIPTOR	m_InterfaceDescriptor;
	uint32_t					m_DataLength;
	uint32_t					m_PageCount;
	std::string					m_SerialNumber;
	uint16_t					m_RevisionNumber;
	bool						m_MonitorWasPresent;
	uint8_t						m_monitorType;
	uint16_t					m_maximumPacketSize;
	// dae0 : 20220613
	std::string					m_MonitorFWVersion;
	std::string					m_MonitorSerialNumber;


	static HRESULT RetrieveDevicePath(
		_Out_bytecap_(BufLen) LPTSTR DevicePath,
		_In_                  ULONG  BufLen,
		_In_				  uint8_t DeviceIndex,
		_Out_opt_             PBOOL  FailureDeviceNotFound
		);

	HRESULT DetermineDataLength();
	HRESULT DeterminePageCount();
	HRESULT DetermineSerialNumber();
	HRESULT DetermineRevisionNumber();
	HRESULT DetermineMonitorType();
	// dae0 : 20220613 
	HRESULT DetermineMonitorFWVersion();
	HRESULT DetermineMonitorSerialNumber();
	// dae0 : 20220701 : get cable type. && support
	uint8_t GetCableType(uint8_t minVer );
public:
	DecaturCableDevice(LPTSTR DevicePath);
	~DecaturCableDevice();

	static HRESULT DecaturCableDevice::RetrieveDevicePaths(std::vector<std::unique_ptr<TCHAR[]>> *DevicePaths);

	HRESULT Open(_Out_opt_ LPTSTR DevicePath);
	VOID Close();
	HRESULT InitiateDownload(uint32_t start_page, uint32_t page_count, bool retain_current_page);
	uint32_t GetDataLength();
	uint32_t GetPageCount();
	HRESULT DownloadData(uint8_t* buffer, uint32_t buffer_length, unsigned long* length_received);
	HRESULT EraseFlash();
	HRESULT MonitorPresent(bool* present);
	uint8_t GetMonitorType();
	HRESULT RefreshMonitorData();

	TCHAR* GetPath();
	std::string GetSerialNumber();
	uint16_t GetRevisionNumber();

	// dae0 : 20220613 
	std::string GetMonitorFWVersion();
	std::string GetMonitorSerialNumber();
	// dae0 : 20220615 :
	HRESULT FW_Upgrade();
	// dae0 : 20220617 :
	HRESULT InitiateUpload(uint32_t start_page, uint32_t page_count, bool retain_current_page);
	HRESULT UploadData(uint8_t* buffer, uint32_t buffer_length, unsigned long* length_received);
};
