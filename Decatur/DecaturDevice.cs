using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decatur
{
    public class DecaturDevice
    {
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private DecaturOperation currentOperation = DecaturOperation.Idle;
        private DecaturResult lastResult = DecaturResult.UnknownError;
        private int bytesReceived = 0;
        private int totalBytes = 0;
        private IntPtr handle;
        private byte[][] retrievedPages = null;
        private Object retrievedPagesLock = new Object();
        private DecaturData parsedData = null;
        private int monitorPageSize = 0;
        private DuetData parsedDuetData = null;
        const int movePage = 1;

        public event EventHandler<DecaturOperationCompleteEventArgs> OperationComplete;
        public event EventHandler<DecaturOperationProgressEventArgs> OperationProgress;

        public string DumpPath { get; private set; }
        public uint PageRangeStart { get; private set; }
        public uint PageRangeCount { get; private set; }
        public bool PageRangeRetain { get; private set; }

        public string SerialNumber {
            get
            {
                StringBuilder serial = new StringBuilder(33);

                DLL.DecaturComm_GetDeviceSerialNumber(this.handle, serial, serial.Capacity);
                return serial.ToString();
            }
        }

        public int Revision
        {
            get
            {
                int revision;

                DLL.DecaturComm_GetRevision(this.handle, out revision);

                return revision;
            }
        }

        public int BytesAvailable
        {
            get
            {
                int bytesAvailable = 0;

                DLL.DecaturComm_GetDataLength(this.handle, out bytesAvailable);

                return bytesAvailable;
            }
        }

        public int PageCount
        {
            get
            {
                int pageCount = 0;

                DLL.DecaturComm_GetPageCount(this.handle, out pageCount);

                return pageCount;
            }
        }

    


        public DecaturMonitorType MonitorType
        {
            get
            {
                byte value;

                DLL.DecaturComm_MonitorType(this.handle, out value);
                return (DecaturMonitorType)value;
            }
        }

        public string ActiveTime
        {
            get
            {
                double activeTime = 0;
                string time;

                if (MonitorType == DecaturMonitorType.SOLO)
                {
                    activeTime = PageCount * 5.34;
                }
                else if(MonitorType == DecaturMonitorType.ADX_4GB || MonitorType == DecaturMonitorType.DM500)
                {
                    activeTime = PageCount * 6.76;
                }

                var seconds = TimeSpan.FromSeconds(activeTime);
                time = seconds.ToString(@"dd\:hh\:mm\:ss");

                return time;
            }
        }

        public bool MonitorPresent {
            get
            {
                bool present = false;

                var result = Decatur.GetResult(DLL.DecaturComm_MonitorPresent(this.handle, out present));

                switch (result)
                {
                    case DecaturResult.Timeout:
                        throw new TimeoutException("MonitorPresent: Timeout");
                        break;

                    case DecaturResult.GeneralCommError:
                        throw new InvalidOperationException("MonitorPresent: Communication Error");
                        break;

                    case DecaturResult.InvalidParameter:
                        throw new ArgumentException("MonitorPresent: Invalid Parameter");
                        break;
                }

                return present;
            }
        }
        // dae0 : 20220613 :
        // [-------------------------------------------------------
        public string MonitorFWVersion {
            get
            {
                StringBuilder fw_version = new StringBuilder(17);
                DLL.DecaturComm_GetMonitorFWVersion(this.handle, fw_version, fw_version.Capacity);
                return fw_version.ToString();
            }
        }
        public string MonitorSerialNumber {
            get
            {
                StringBuilder monitor_serial = new StringBuilder(17);
                DLL.DecaturComm_GetMonitorSerialNumber(this.handle, monitor_serial, monitor_serial.Capacity);
                return monitor_serial.ToString();
            }
        }
        // -------------------------------------------------------]


        public DecaturOperation CurrentOperation
        {
            get
            {
                return this.currentOperation;
            }
        }

        //public DecaturData ParsedData
        //{
        //    get
        //    {
        //        return this.parsedData;
        //    }
        //}
        public DuetData ParsedData
        {
            get
            {
                return this.parsedDuetData;
            }
        }

        public DecaturResult LastResult
        {
            get
            {
                return this.lastResult;
            }
        }


        // dae0 : 20220708 : 
        public bool Is_FirstPage_Info
        {
            get
            {
                bool IsFirstPage = false;

                StringBuilder monitor_serial = new StringBuilder(17);
                DLL.DecaturComm_GetMonitorSerialNumber(this.handle, monitor_serial, monitor_serial.Capacity);

                if (MonitorType != DecaturMonitorType.SOLO)
                {
                    string[] tmp_version = MonitorFWVersion.Split('.');

                    if (tmp_version[0].All(char.IsDigit))
                    {
                        int version = 0;
                        for (int i = 0; i < 3; i++)
                            version |= Convert.ToInt32(tmp_version[i]) << (8 * (2 - i));

                        if (version > 0x000200)
                        {
                            IsFirstPage = true;
                        }
                    }
                }
                return IsFirstPage;
            }
        }

        public DecaturDevice(IntPtr handle)
        {
            this.handle = handle;
            this.backgroundWorker.ProgressChanged += this.backgroundWorker_ProgressChanged;
            this.backgroundWorker.RunWorkerCompleted += this.backgroundWorker_RunWorkerCompleted;
            this.backgroundWorker.WorkerReportsProgress = true;
        }


        public int MonitorPageSize(DecaturMonitorType mtype)
        {
            if (mtype == DecaturMonitorType.SOLO)
                return 2048;
            else if (mtype == DecaturMonitorType.ADX_4GB)
                return 4096;
            else
                return (4096 + 128);
        }



        public void DumpToFile(string path)
        {
            if (this.backgroundWorker.IsBusy)
            {
                throw new InvalidOperationException("An operation is currently in progress. You must wait for the operation to complete before starting a new operation.");
            }

            this.DumpPath = path;
            this.backgroundWorker.DoWork += this.OnDumpToFile;
            this.currentOperation = DecaturOperation.DownloadToFile;
            this.backgroundWorker.RunWorkerAsync();
        }

        public void DumpToCSVFile(string path)
        {
            if (this.backgroundWorker.IsBusy)
            {
                throw new InvalidOperationException("An operation is currently in progress. You must wait for the operation to complete before starting a new operation.");
            }

            this.DumpPath = path;
            this.backgroundWorker.DoWork += this.OnDumpToCSVFile;
            this.currentOperation = DecaturOperation.DownloadToFile;
            this.backgroundWorker.RunWorkerAsync();
        }

        // dae0 : 20220617 : 
        // [-------------------------------------------------------
        public void UploadFromFile(string path)
        {
            if (this.backgroundWorker.IsBusy)
            {
                throw new InvalidOperationException("An operation is currently in progress. You must wait for the operation to complete before starting a new operation.");
            }

            this.DumpPath = path;
            this.backgroundWorker.DoWork += this.OnUploadFromFile;
            this.currentOperation = DecaturOperation.UploadFromFile;
            this.backgroundWorker.RunWorkerAsync();
        }
        // -------------------------------------------------------]
        /*
        public void UploadFromCSVFile(string path)
        {
            if (this.backgroundWorker.IsBusy)
            {
                throw new InvalidOperationException("An operation is currently in progress. You must wait for the operation to complete before starting a new operation.");
            }

            this.DumpPath = path;
            this.backgroundWorker.DoWork += this.OnUploadFromCSVFile;
            this.currentOperation = DecaturOperation.UploadFromFile;
            this.backgroundWorker.RunWorkerAsync();
        }
        */
        public void UploadFromTXTFile(string path)
        {
            if (this.backgroundWorker.IsBusy)
            {
                throw new InvalidOperationException("An operation is currently in progress. You must wait for the operation to complete before starting a new operation.");
            }

            this.DumpPath = path;
            this.backgroundWorker.DoWork += this.OnUploadFromTXTFile;
            this.currentOperation = DecaturOperation.UploadFromFile;
            this.backgroundWorker.RunWorkerAsync();
        }

        public void RetrievePagesAsync(uint startPage, uint pageCount, bool retainCurrentPage)
        {
            if (this.backgroundWorker.IsBusy)
            {
                throw new InvalidOperationException("An operation is currently in progress. You must wait for the operation to complete before starting a new operation.");
            }

            this.PageRangeCount = pageCount;
            this.PageRangeStart = startPage;
            this.PageRangeRetain = retainCurrentPage;


            // dae0 : 20220707 : 
            if (this.Is_FirstPage_Info) //Monitor Type이 SOLO가 아니고, version > 0x000200일 때, true 반환 
            {
                //this.PageRangeStart = startPage + 64; // move start point
                this.PageRangeStart = startPage + movePage;
                if (this.PageRangeStart < this.PageCount) // if (starPage < lastPage)
                {
                    if ((this.PageRangeStart + this.PageRangeCount) > this.PageCount)  // boundary check.
                        this.PageRangeCount = (uint)(this.PageCount - this.PageRangeStart);
                }
                else
                    this.PageRangeCount = 0;
            }

            if (this.PageRangeCount > 0) // dae0 : 20220707 : add condition. 
            {
                this.backgroundWorker.DoWork += this.OnRetrievePagesAsync;
                this.currentOperation = DecaturOperation.DownloadRangeAsync;
                this.backgroundWorker.RunWorkerAsync();
            }
        }

        public void RetrieveViewPagesAsync(uint startPage, uint pageCount, bool retainCurrentPage)
        {
            if (this.backgroundWorker.IsBusy)
            {
                throw new InvalidOperationException("An operation is currently in progress. You must wait for the operation to complete before starting a new operation.");
            }

            this.PageRangeCount = pageCount;
            this.PageRangeStart = startPage;
            this.PageRangeRetain = retainCurrentPage;


            // dae0 : 20220707 : 
            if (this.Is_FirstPage_Info) //Monitor Type이 SOLO가 아니고, version > 0x000200일 때, true 반환 
            {
                this.PageRangeStart = startPage + 1; // move start point
                if (this.PageRangeStart < this.PageCount) // if (starPage < lastPage)
                {
                    if ((this.PageRangeStart + this.PageRangeCount) > this.PageCount)  // boundary check.
                        this.PageRangeCount = (uint)(this.PageCount - this.PageRangeStart);
                }
                else
                    this.PageRangeCount = 0;
            }

            if (this.PageRangeCount > 0) // dae0 : 20220707 : add condition. 
            {
                this.backgroundWorker.DoWork += this.OnRetrievePagesAsync;
                this.currentOperation = DecaturOperation.RangeAsync;
                this.backgroundWorker.RunWorkerAsync();
            }
        }

        public void ParsePages(bool analyze)
        {
            byte[] statpage = null;
            byte[][] pgs = RetrievePages(1, Decatur.STATUS_PAGE);
            if (pgs != null)
                statpage = pgs[0];


           this.parsedDuetData = new DuetData(this.retrievedPages, analyze, statpage);

  //          this.parsedData = new DecaturData(this.retrievedPages, (int)this.PageRangeStart, analyze);
        }

        private void OnRetrievePagesAsync(object sender, DoWorkEventArgs e)
        {
            lock (this.retrievedPagesLock)
            {
                var reportTimeSpan = new TimeSpan(0, 0, 0, 0, 100);
                DateTime nextReportTime = DateTime.Now;
                DecaturResult result = DecaturResult.UnknownError;

                try
                {
                    monitorPageSize = MonitorPageSize(MonitorType); //2048 or 4096 or 4224

                    this.retrievedPages = new byte[this.PageRangeCount][];
                    ulong lengthReceived = 0;
                    this.totalBytes = (int)this.PageRangeCount * monitorPageSize; // Decatur.PAGE_SIZE;
                    this.bytesReceived = 0;

                    DLL.DecaturComm_BeginDownload(this.handle, this.PageRangeStart, this.PageRangeCount, this.PageRangeRetain); 

                    for (int index = 0; index < this.PageRangeCount; index++)
                    {
                        this.retrievedPages[index] = new byte[monitorPageSize];

                        result = Decatur.GetResult(DLL.DecaturComm_DownloadData(this.handle, this.retrievedPages[index], (uint)this.retrievedPages[index].Length, out lengthReceived));

                        if (result != DecaturResult.Success)
                        {
                            this.lastResult = result;
                            return;
                        }

                        this.bytesReceived += (int)lengthReceived;

                        if (this.totalBytes > 0 && DateTime.Now > nextReportTime)
                        {
                            this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                            nextReportTime = DateTime.Now + reportTimeSpan;
                        }
                    }

                    if (this.totalBytes > 0)
                    {
                        this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                    }

                    result = DecaturResult.Success;
                }
                catch (Exception)
                {
                    result = DecaturResult.UnknownError;
                }
                finally
                {
                    this.backgroundWorker.DoWork -= this.OnRetrievePagesAsync;

                    this.lastResult = result;
                }
            }
        }

        public void RetrievePagesAsync(uint pageCount)
        {
            RetrievePagesAsync(0, pageCount, true);
        }

        public void RetrievePagesAsync(uint pageCount, uint startPage)
        {
            RetrievePagesAsync(startPage, pageCount, false);
        }

        public void RetrieveViewPagesAsync(uint pageCount, uint startPage)
        {
            RetrieveViewPagesAsync(startPage, pageCount, false);
        }

        private byte[][] RetrievePages(uint startPage, uint pageCount, bool retainCurrentPage)
        {
            DecaturResult result = DecaturResult.UnknownError;
            byte[][] pages = new byte[pageCount][];

            monitorPageSize = MonitorPageSize(MonitorType);

            try
            {
                ulong lengthReceived = 0;
                int retcode;
                retcode = DLL.DecaturComm_BeginDownload(this.handle, startPage, pageCount, retainCurrentPage);
                
                if(retcode != 0)
                {
                //    System.Console.WriteLine("BeginDownload returned {0}!", retcode);
                    this.lastResult = Decatur.GetResult(retcode);
                    return null;
                }
                for (int index = 0; index < pageCount; index++)
                {
                    pages[index] = new byte[monitorPageSize]; // was Decatur.PAGE_SIZE

                    result = Decatur.GetResult(DLL.DecaturComm_DownloadData(this.handle, pages[index], (uint)monitorPageSize, out lengthReceived));
//                    System.Console.WriteLine("Page {0} Received {1}", index, lengthReceived);
                    if (result != DecaturResult.Success)
                    {
                        this.lastResult = result;
                        return null;
                    }


                    // Temporary
                    //if ((index % 64) == 0)
                    //    System.Console.Write("[{0}]", index);
                    //if (index == pageCount - 1)
                    //    System.Console.WriteLine();
                }

                result = DecaturResult.Success;
            }
            catch (Exception)
            {
                result = DecaturResult.UnknownError;
            }

            this.lastResult = result;

            if (this.lastResult != DecaturResult.Success)
            {
                return null;
            }

            return pages;
        }

        public byte[][] RetrievePages(uint pageCount)
        {
            return RetrievePages(0, pageCount, true);
        }

        public byte[][] RetrievePages(uint pageCount, uint startPage)
        {
            return RetrievePages(startPage, pageCount, false);
        }

        public DecaturResult EraseFlash()
        {
            return Decatur.GetResult(DLL.DecaturComm_EraseFlash(this.handle));
        }

        private void OnDumpToFile(object sender, DoWorkEventArgs e)
        {
            var reportTimeSpan = new TimeSpan(0, 0, 0, 0, 100);
            DateTime nextReportTime = DateTime.Now;
            DecaturResult result = DecaturResult.UnknownError;

            try
            {
                ulong lengthReceived = 0;
                this.totalBytes = this.BytesAvailable; //RefreshDeviceList()에서 value 얻음
                this.bytesReceived = 0;
 
                monitorPageSize = MonitorPageSize(MonitorType); // added
                byte[] buffer = new byte[monitorPageSize];

                DLL.DecaturComm_BeginDownload(this.handle, 0, (uint)this.PageCount, false);
                //여기서부터 시작
                using (BinaryWriter file = new BinaryWriter(File.Create(this.DumpPath))) //리소스 범위 벗어나면 자동으로 리소스를 해제하여 관리
                {
                    while (this.bytesReceived < this.totalBytes)
                    {
                        result = Decatur.GetResult(DLL.DecaturComm_DownloadData(this.handle, buffer, (uint)buffer.Length, out lengthReceived));

                        if (result != DecaturResult.Success)
                        {
                            return;
                        }

                        file.Write(buffer, 0, (int)lengthReceived);

                        this.bytesReceived += (int)lengthReceived;

                        if (this.totalBytes > 0 && DateTime.Now > nextReportTime)
                        {
                            this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender); //사용자에게 비동기 작업의 진행률을 보고
                            nextReportTime = DateTime.Now + reportTimeSpan;
                        }
                    }
                }

                if (this.totalBytes > 0)
                {
                    this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                }

                result = DecaturResult.Success;
            }
            catch (Exception)
            {
                result = DecaturResult.UnknownError;
            }
            finally
            {
                this.backgroundWorker.DoWork -= this.OnDumpToFile;

                this.lastResult = result;
            }
        }

        private void OnDumpToCSVFile(object sender, DoWorkEventArgs e)
        {
            var reportTimeSpan = new TimeSpan(0, 0, 0, 0, 100);
            DateTime nextReportTime = DateTime.Now;
            DecaturResult result = DecaturResult.UnknownError;
            int count = 0;
            try
            {
                ulong lengthReceived = 0;
                this.totalBytes = this.BytesAvailable; //RefreshDeviceList()에서 value 얻음
                this.bytesReceived = 0;

                monitorPageSize = MonitorPageSize(MonitorType); // added
                byte[] buffer = new byte[monitorPageSize];
               

                DLL.DecaturComm_BeginDownload(this.handle, 0, (uint)this.PageCount, false);
                

                
                using (StreamWriter file = new StreamWriter(File.Create(this.DumpPath))) //리소스 범위 벗어나면 자동으로 리소스를 해제하여 관리
                {
                    while (this.bytesReceived < this.totalBytes)
                    {
                        result = Decatur.GetResult(DLL.DecaturComm_DownloadData(this.handle, buffer, (uint)buffer.Length, out lengthReceived));

                        if (result != DecaturResult.Success)
                        {
                            return;
                        }

                        file.WriteLine("Page {0}", count);
                        //count++;
                        //string str = Encoding.Default.GetString(buffer);
                        //string str = BitConverter.ToString(buffer).Replace("-", "");
                        //convert_data = System.Convert.ToByte(csv_data.Substring(count, 2), 16);
                        for (int index = 0; index < buffer.Length; index++)
                        {
                            if (index % 16 == 0)
                            {
                                file.WriteLine();
                                string str = BitConverter.ToString(buffer, index, 1).Replace("-", " ");
                                file.Write("{0} ",str);
                                //text.AppendLine(); //enter
                                //text.Append(index.ToString("X8")); //Offset : 16진수 8자리 맞추기. 빈공간 0으로 채움.
                                //text.Append("  ");
                            }
                            else if(index % 8 == 0)
                            {
                                
                                string str = BitConverter.ToString(buffer, index, 1).Replace("-", " ");
                                file.Write("     {0} ", str);
                            }
                            else
                            {
                                string str = BitConverter.ToString(buffer, index, 1).Replace("-", " ");
                                file.Write("{0} ", str);
                            }
                            
                            //text.Append(pages[0][index].ToString("X2")); //하나의 byte를 16진수(Hex) 문자열로 변경
                            //text.Append(" ");
                        }
                        count++;
                        file.WriteLine("\n"); //page별 섹션 구분
                        //file.WriteLine(BitConverter.ToString(buffer).Replace("-", " "));

                        this.bytesReceived += (int)lengthReceived;

                        if (this.totalBytes > 0 && DateTime.Now > nextReportTime)
                        {
                            this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender); //사용자에게 비동기 작업의 진행률을 보고
                            nextReportTime = DateTime.Now + reportTimeSpan;
                        }
                    }
                }

                if (this.totalBytes > 0)
                {
                    this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                }

                result = DecaturResult.Success;
            }
            catch (Exception)
            {
                result = DecaturResult.UnknownError;
            }
            finally
            {
                this.backgroundWorker.DoWork -= this.OnDumpToCSVFile;

                this.lastResult = result;
                count = 0;
            }
        }
        // [-------------------------------------------------------
        // dae0 : 20220510 :
        private void OnUploadFromFile(object sender, DoWorkEventArgs e)
        {
            var reportTimeSpan = new TimeSpan(0, 0, 0, 0, 100);
            DateTime nextReportTime = DateTime.Now;
            DecaturResult result = DecaturResult.UnknownError;

            try
            {
                ulong lengthReceived = 0;
                uint loop_cnt = 0;
                this.totalBytes = (int) new FileInfo(this.DumpPath).Length;
                this.bytesReceived = 0;
 
                monitorPageSize = MonitorPageSize(MonitorType); // added
                byte[] buffer = new byte[monitorPageSize];

                loop_cnt = (uint)( this.totalBytes / monitorPageSize );

                DLL.DecaturComm_BeginUpload(this.handle,  0, loop_cnt, false);
                
                using (BinaryReader file = new BinaryReader(File.Open(this.DumpPath, FileMode.Open)))
                {
                    while (this.bytesReceived < this.totalBytes)
                    {
                        file.Read(buffer, 0, (int)monitorPageSize); //내부 스트림에서 바이트를 읽고 스트림의 현재 위치를 앞으로 이동

                        result = Decatur.GetResult(DLL.DecaturComm_UploadData(this.handle, buffer, (uint)buffer.Length, out lengthReceived));

                        if (result != DecaturResult.Success)
                        {
                            return;
                        }
                        
                        this.bytesReceived += (int)lengthReceived;

                        if (this.totalBytes > 0 && DateTime.Now > nextReportTime)
                        {
                            this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                            nextReportTime = DateTime.Now + reportTimeSpan;
                        }
                    }
                }

                if (this.totalBytes > 0)
                {
                    this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                }

                    result = DecaturResult.Success;
            }
            catch (Exception)
            {
                result = DecaturResult.UnknownError;
            }
            finally
            {
                this.backgroundWorker.DoWork -= this.OnUploadFromFile; 

                this.lastResult = result;
            }
        }
        /*
        private void OnUploadFromCSVFile(object sender, DoWorkEventArgs e)
        {
            var reportTimeSpan = new TimeSpan(0, 0, 0, 0, 100);
            DateTime nextReportTime = DateTime.Now;
            DecaturResult result = DecaturResult.UnknownError;
            string temp_file = Path.GetTempFileName();
            BinaryWriter bin = new BinaryWriter(File.Create(temp_file));
            byte convert_data;
            try
            {
                //using (BinaryWriter bin = new BinaryWriter(File.Create(temp_path)))
                //{
                using (StreamReader reader = new StreamReader(this.DumpPath))
                {
                    while (!reader.EndOfStream)
                    {
                        string csv_data = reader.ReadLine();
                        for(int count = 0; count < csv_data.Length;count +=2)
                        {
                             convert_data = System.Convert.ToByte(csv_data.Substring(count, 2),16);
                             bin.Write(convert_data);
                        }
                        //byte[] convert_data = Encoding.Default.GetBytes(csv_data);
                        //bin.Write(convert_data);

                    }
                    bin.Close();
                }
                //}

                ulong lengthReceived = 0;
                uint loop_cnt = 0;
                this.totalBytes = (int)new FileInfo(temp_file).Length;
                this.bytesReceived = 0;

                monitorPageSize = MonitorPageSize(MonitorType); // added
                byte[] buffer = new byte[monitorPageSize];

                loop_cnt = (uint)(this.totalBytes / monitorPageSize);

                DLL.DecaturComm_BeginUpload(this.handle, 0, loop_cnt, false);

                using (BinaryReader file = new BinaryReader(File.Open(temp_file,FileMode.Open)))
                {
                    while (this.bytesReceived < this.totalBytes)
                    {
                        file.Read(buffer, 0, (int)monitorPageSize); //내부 스트림에서 바이트를 읽고 스트림의 현재 위치를 앞으로 이동

                        result = Decatur.GetResult(DLL.DecaturComm_UploadData(this.handle, buffer, (uint)buffer.Length, out lengthReceived));

                        if (result != DecaturResult.Success)
                        {
                            return;
                        }

                        this.bytesReceived += (int)lengthReceived;

                        if (this.totalBytes > 0 && DateTime.Now > nextReportTime)
                        {
                            this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                            nextReportTime = DateTime.Now + reportTimeSpan;
                        }
                    }
                }

                if (this.totalBytes > 0)
                {
                    this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                }

                result = DecaturResult.Success;
            }
            catch (Exception)
            {
                result = DecaturResult.UnknownError;
            }
            finally
            {
                this.backgroundWorker.DoWork -= this.OnUploadFromCSVFile;
                bin.Close();

                this.lastResult = result;
            }
        }
        */
        private void OnUploadFromTXTFile(object sender, DoWorkEventArgs e)
        {
            var reportTimeSpan = new TimeSpan(0, 0, 0, 0, 100);
            DateTime nextReportTime = DateTime.Now;
            DecaturResult result = DecaturResult.UnknownError;
            string temp_file = Path.GetTempFileName();
            BinaryWriter bin = new BinaryWriter(File.Create(temp_file));
            byte convert_data;
            try
            {
                //using (BinaryWriter bin = new BinaryWriter(File.Create(temp_path)))
                //{
                using (StreamReader reader = new StreamReader(this.DumpPath))
                {
                    while (!reader.EndOfStream)
                    {
                        string csv_data = reader.ReadLine();
                        csv_data = csv_data.Replace(" ", string.Empty);
                        if (csv_data != "")
                        {
                            if (!csv_data.StartsWith("Page"))
                            {
                                for (int count = 0; count < csv_data.Length; count += 2)
                                {
                                    convert_data = System.Convert.ToByte(csv_data.Substring(count, 2), 16);
                                    bin.Write(convert_data);
                                }
                            }
                        }
                        //byte[] convert_data = Encoding.Default.GetBytes(csv_data);
                        //bin.Write(convert_data);

                    }
                    bin.Close();
                }
                //}

                ulong lengthReceived = 0;
                uint loop_cnt = 0;
                this.totalBytes = (int)new FileInfo(temp_file).Length;
                this.bytesReceived = 0;

                monitorPageSize = MonitorPageSize(MonitorType); // added
                byte[] buffer = new byte[monitorPageSize];

                loop_cnt = (uint)(this.totalBytes / monitorPageSize);

                DLL.DecaturComm_BeginUpload(this.handle, 0, loop_cnt, false);

                using (BinaryReader file = new BinaryReader(File.Open(temp_file, FileMode.Open)))
                {
                    while (this.bytesReceived < this.totalBytes)
                    {
                        file.Read(buffer, 0, (int)monitorPageSize); //내부 스트림에서 바이트를 읽고 스트림의 현재 위치를 앞으로 이동

                        result = Decatur.GetResult(DLL.DecaturComm_UploadData(this.handle, buffer, (uint)buffer.Length, out lengthReceived));

                        if (result != DecaturResult.Success)
                        {
                            return;
                        }

                        this.bytesReceived += (int)lengthReceived;

                        if (this.totalBytes > 0 && DateTime.Now > nextReportTime)
                        {
                            this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                            nextReportTime = DateTime.Now + reportTimeSpan;
                        }
                    }
                }

                if (this.totalBytes > 0)
                {
                    this.backgroundWorker.ReportProgress((int)((this.bytesReceived / (float)this.totalBytes) * 100.0f), sender);
                }

                result = DecaturResult.Success;
            }
            catch (Exception)
            {
                result = DecaturResult.UnknownError;
            }
            finally
            {
                this.backgroundWorker.DoWork -= this.OnUploadFromTXTFile;
                bin.Close();

                this.lastResult = result;
            }
        }

        // -------------------------------------------------------]

        protected virtual void OnOperationComplete(DecaturOperationCompleteEventArgs e)
        {
            EventHandler<DecaturOperationCompleteEventArgs> handler = this.OperationComplete;

            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnOperationProgress(DecaturOperationProgressEventArgs e)
        {
            EventHandler<DecaturOperationProgressEventArgs> handler = this.OperationProgress;

            if(handler != null)
            {
                handler(this, e);
            }
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.OnOperationProgress(new DecaturOperationProgressEventArgs(this.currentOperation, e.ProgressPercentage, this.bytesReceived, this.totalBytes));
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var operation = this.currentOperation;
            this.currentOperation = DecaturOperation.Idle;
            this.OnOperationComplete(new DecaturOperationCompleteEventArgs(operation, this.lastResult));
        }

        public void ClearRetrievedPages()
        {
            lock (this.retrievedPagesLock)
            {
                this.retrievedPages = null;
            }
        }

        public byte[] GetRetrievedPage(int index)
        {
            lock (this.retrievedPagesLock)
            {
                if (this.retrievedPages == null || this.retrievedPages.Length < index)
                {
                    throw new IndexOutOfRangeException("Page not found");
                }

                return this.retrievedPages[index];
            }
        }

        public DecaturResult FW_Upgrade() // dae0 : 20220615 :
        {
            return Decatur.GetResult(DLL.DecaturComm_FW_Upgrade(this.handle));
        }
    }
}
