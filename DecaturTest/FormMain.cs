using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Decatur;
using System.IO;

namespace DecaturTest
{
    public partial class FormMain : Form
    {
        private const int PAGES_AVAILABLE_COLUMN_INDEX = 2;
        private const int BYTES_AVAILABLE_COLUMN_INDEX = 3;
        private const int MONITOR_PRESENT_COLUMN_INDEX = 4;
        // dae0 : 20220613 :
        private const int FW_VERSION_COLUMN_INDEX = 5;
        private const int BLE_SERIAL_COLUMN_INDEX = 6;
        private const int ACTIVE_TIME_COLUMN_INDEX = 7;

        DateTime operationStartTime;
        private List<DecaturDevice> devices = new List<DecaturDevice>();
        private string emcGoldPath;
        private string emcTestPath;
        DecaturDevice emcDevice;
        private bool emcDumpingGold = false;
        private bool emcTesting = false;
        private bool emcTestEnding = false;
        private StreamWriter emcLogStream = null;
        private Timer monitorDetectTimer = new Timer();

        private bool savingIMU = false;

        int rangepage = 0;
        const int movePage = 1;
        int startTime = 0;
        int rangeStartpage = 0;
        int rangeEndpage = 0;


        public FormMain()
        {
            InitializeComponent();
        }

        private DecaturDevice GetSelectedDevice() //close
        {
            if (this.listViewDevices.SelectedIndices.Count <= 0 || this.listViewDevices.SelectedIndices[0] > this.devices.Count)
            {
                return null;
            }

            return this.devices[this.listViewDevices.SelectedIndices[0]];
        }
        /*
        private void buttonDownload_Click(object sender, EventArgs e) //close
        {
            var device = this.GetSelectedDevice();

            if (device == null)
            {
                MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (device.CurrentOperation != DecaturOperation.Idle)
            {
                MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (this.saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.labelProgress.Text = "0 / " + device.BytesAvailable.ToString() + " Bytes Downloaded";
                this.toolStripStatusLabelOperation.Text = "Download Data";
                device.DumpToFile(this.saveFileDialog.FileName);
                this.operationStartTime = DateTime.Now;
            }
        }
        */
        private void RefreshDeviceList() //close
        {
            this.listViewDevices.Items.Clear();

            if (this.devices != null)
            {
                this.devices.Clear();
            }

            this.devices = Decatur.Decatur.GetConnectedDevices();

            if (this.devices == null)
            {
                return;
            }

            foreach (var device in this.devices)
            {
                device.OperationComplete += device_OperationComplete;
                device.OperationProgress += device_OperationProgress;

                try
                {
                    // Interpret as major.minor, as per Cardiac's SDP
                    string revstr = String.Format("{0}.{1}", (device.Revision >> 8) & 0xFF, device.Revision & 0xFF);

                    this.listViewDevices.Items.Add(
                        new ListViewItem(
                            // dae0 : 20220613 :
                            //new string[] { device.SerialNumber, revstr, device.PageCount.ToString(), device.BytesAvailable.ToString(), device.MonitorPresent.ToString() }
                            //new string[] { device.SerialNumber, revstr, device.PageCount.ToString(), device.BytesAvailable.ToString(), device.MonitorPresent.ToString(), device.MonitorFWVersion, device.MonitorSerialNumber }
                            new string[] { device.SerialNumber, revstr, device.PageCount.ToString(), device.BytesAvailable.ToString(), device.MonitorPresent ? device.MonitorType.ToString() : device.MonitorPresent.ToString(), device.MonitorFWVersion, device.MonitorSerialNumber, device.ActiveTime }
                        )
                    );

                    // dae0 : 20220617 :
                    if (device.MonitorPresent)
                        listViewDevices.Items[0].Selected = true;

                }
                catch (Exception)
                {
                    // Error collecting data from device.
                }
            }

            this.monitorDetectTimer.Enabled = (this.devices.Count > 0);
        }

        private void buttonListDevices_Click(object sender, EventArgs e) //close
        {
            this.RefreshDeviceList();
        }

        void device_OperationProgress(object sender, DecaturOperationProgressEventArgs e) //close
        {
            DecaturDevice device = sender as DecaturDevice;

            switch (e.Operation)
            {
                case DecaturOperation.DownloadRangeAsync:
                case DecaturOperation.RangeAsync:
                case DecaturOperation.DownloadToFile:
                    labelProgress.Text = e.BytesReceived.ToString() + " / " + e.TotalBytes.ToString() + " Bytes Downloaded";
                    progressBarOperation.Value = e.PercentComplete;
                    break;
                case DecaturOperation.UploadFromFile: // dae0 : 20220617 :
                    labelProgress.Text = e.BytesReceived.ToString() + " / " + e.TotalBytes.ToString() + " Bytes Uploaded";
                    progressBarOperation.Value = e.PercentComplete;
                    break;
            }
        }

        void device_OperationComplete(object sender, DecaturOperationCompleteEventArgs e) //close
        {
            DecaturDevice device = sender as DecaturDevice;
            TimeSpan interval = DateTime.Now - this.operationStartTime;
            this.toolStripStatusLabelTimeValue.Text = interval.ToString();

            switch (e.Operation)
            {
                case DecaturOperation.UploadFromFile: // dae0 : 20220617 :
                    if (e.Result == DecaturResult.Success)
                    {
                        MessageBox.Show("Finished uploading results from device " + device.SerialNumber + " to " + device.DumpPath + ".", "Upload Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshDeviceList();
                    }
                    else
                    {
                        MessageBox.Show("Upload operation for device " + device.SerialNumber + " has failed with result " + e.Result.ToString() + ".", "Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case DecaturOperation.DownloadToFile:
                    if (this.emcTesting) //false가 기본값 -> EMC Test Button 누르면서 true
                    {
                        EmcDownloadComplete(e);
                        return;
                    }

                    if (this.emcTestEnding)
                    {
                        this.emcTestEnding = false;
                        File.Delete(this.emcTestPath);
                        this.toolStripStatusLabelOperation.Text = "Idle";
                        return;
                    }

                    if (e.Result == DecaturResult.Success)
                    {
                        MessageBox.Show("Finished downloading results from device " + device.SerialNumber + " to " + device.DumpPath + ".", "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Download operation for device " + device.SerialNumber + " has failed with result " + e.Result.ToString() + ".", "Download Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;

                case DecaturOperation.DownloadRangeAsync:
                    if (e.Result == DecaturResult.Success)
                    {
                        try
                        {
                            if (Path.GetExtension(this.saveFileDialog.FileName).ToUpper() == ".CSV")
                            {
                                // TODO:
                                // Different path for SOLO?


                                device.ParsePages(/*chkAnalyze.Checked*/ true);
                                var data = device.ParsedData;
                                Check_Leadoff(data.Leadoff_data);
                                if (data.Analyzed)
                                {
                                    this.groupBoxAnalysis.Text = "Analysis";
                                    this.textBoxAnalysisClock.Text = data.AverageInterval.ToString("#.0");
                                    this.textBoxAnalysisHz.Text = data.AverageFrequency.ToString("#.00") + "Hz";
                                    this.textBoxAnalysisBattery.Text = data.AverageBatteryVoltage.ToString("#.000") + "V";
                                    this.textBoxAnalysisEcg.Text = data.ECG[0].ToString("#.000") + "mV";
                                    this.textBoxAnalysisNoise.Text = data.Noise[0].ToString("#.0") + "uV";
                                    this.textBoxAnalysisPeaks.Text = data.SamplesBetweenPeaks[0].ToString();
                                    this.textBoxAnalysisButton.Text = data.ButtonPressSeconds.ToString() + " sec";
                                    this.textBoxAnalysisEcg1.Text = data.ECG[1].ToString("#.000") + "mV";
                                    this.textBoxAnalysisNoise1.Text = data.Noise[1].ToString("#.0") + "uV";
                                    this.textBoxAnalysisPeaks1.Text = data.SamplesBetweenPeaks[1].ToString();
                                }
                                else
                                {
                                    this.groupBoxAnalysis.Text = "Analysis (Not Analyzed)";
                                    this.textBoxAnalysisClock.Text = string.Empty;
                                    this.textBoxAnalysisHz.Text = string.Empty;
                                    this.textBoxAnalysisBattery.Text = string.Empty;
                                    this.textBoxAnalysisEcg.Text = string.Empty;
                                    this.textBoxAnalysisNoise.Text = string.Empty;
                                    this.textBoxAnalysisPeaks.Text = string.Empty;
                                    this.textBoxAnalysisButton.Text = string.Empty;
                                    this.textBoxAnalysisEcg1.Text = string.Empty;
                                    this.textBoxAnalysisNoise1.Text = string.Empty;
                                    this.textBoxAnalysisPeaks1.Text = string.Empty;
                                }

                                if (savingIMU)
                                {
                                    savingIMU = false;
                                    data.WriteIMUCSV(this.saveFileDialog.FileName);

                                }
                                else
                                {
                                    data.WriteCSV(this.saveFileDialog.FileName);
                                }
                            }
                            else
                            {
                                using (var file = new BinaryWriter(File.Create(this.saveFileDialog.FileName)))
                                {
                                    for (int index = 0; index < device.PageRangeCount; index++)
                                    {
                                        file.Write(device.GetRetrievedPage(index));
                                    }
                                }
                            }

                            MessageBox.Show("Pages saved to file.", "Pages Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error saving pages to file: " + ex.ToString(), "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            savingIMU = false;
                            device.ClearRetrievedPages();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error downloading pages: " + e.Result.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;

                case DecaturOperation.RangeAsync:
                    if (e.Result == DecaturResult.Success)
                    {
                        device.ParsePages(/*chkAnalyze.Checked*/ true);
                        var data = device.ParsedData;
                        Check_Leadoff(data.Leadoff_data);

                        if (data.Analyzed)
                        {
                            this.groupBoxAnalysis.Text = "Analysis";
                            this.textBoxAnalysisClock.Text = data.AverageInterval.ToString("#.0");
                            this.textBoxAnalysisHz.Text = data.AverageFrequency.ToString("#.00") + "Hz";
                            this.textBoxAnalysisBattery.Text = data.AverageBatteryVoltage.ToString("#.000") + "V";
                            this.textBoxAnalysisEcg.Text = data.ECG[0].ToString("#.000") + "mV";
                            this.textBoxAnalysisNoise.Text = data.Noise[0].ToString("#.0") + "uV";
                            this.textBoxAnalysisPeaks.Text = data.SamplesBetweenPeaks[0].ToString();
                            this.textBoxAnalysisButton.Text = data.ButtonPressSeconds.ToString() + " sec";
                            this.textBoxAnalysisEcg1.Text = data.ECG[1].ToString("#.000") + "mV";
                            this.textBoxAnalysisNoise1.Text = data.Noise[1].ToString("#.0") + "uV";
                            this.textBoxAnalysisPeaks1.Text = data.SamplesBetweenPeaks[1].ToString();
                        }
                        else
                        {
                            this.groupBoxAnalysis.Text = "Analysis (Not Analyzed)";
                            this.textBoxAnalysisClock.Text = string.Empty;
                            this.textBoxAnalysisHz.Text = string.Empty;
                            this.textBoxAnalysisBattery.Text = string.Empty;
                            this.textBoxAnalysisEcg.Text = string.Empty;
                            this.textBoxAnalysisNoise.Text = string.Empty;
                            this.textBoxAnalysisPeaks.Text = string.Empty;
                            this.textBoxAnalysisButton.Text = string.Empty;
                            this.textBoxAnalysisEcg1.Text = string.Empty;
                            this.textBoxAnalysisNoise1.Text = string.Empty;
                            this.textBoxAnalysisPeaks1.Text = string.Empty;
                        }
                    }

                    break;


            }

            this.toolStripStatusLabelOperation.Text = "Idle";
        }

        private void Check_Leadoff(Dictionary<int, bool[]> data)
        {
            RichTextBox_LeadOffPages.Clear();
            string page = "";

            foreach (KeyValuePair<int, bool[]> item in data)
            {
                for (int i = 0; i < item.Value.Length; i++)
                {
                    if (item.Value[i])
                    {
                        if (page != (item.Key + numericUpDownStartPage.Value + movePage).ToString())
                        {
                            page = (item.Key + numericUpDownStartPage.Value + movePage).ToString();
                            RichTextBox_LeadOffPages.AppendText("Page " + page + "\n");
                        }
                    }
                }

            }
        }

        private async Task<bool> EmcCompare() //close
        {
            using (var gold = new FileStream(this.emcGoldPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (var test = new FileStream(this.emcTestPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                Task[] tasks = new Task[2];

                int index = 0;
                byte[] goldBuffer = new byte[4096];
                byte[] testBuffer = new byte[4096];

                while (index < gold.Length)
                {
                    tasks[0] = gold.ReadAsync(goldBuffer, 0, goldBuffer.Length);
                    tasks[1] = test.ReadAsync(testBuffer, 0, testBuffer.Length);

                    await Task.WhenAll(tasks);

                    for (int bufferIndex = 0; bufferIndex < goldBuffer.Length; bufferIndex++)
                    {
                        if (goldBuffer[bufferIndex] != testBuffer[bufferIndex])
                        {
                            return false;
                        }
                    }

                    index += goldBuffer.Length;
                }
            }

            return true;
        }

        private async void EmcDownloadComplete(DecaturOperationCompleteEventArgs e) //close
        {
            if (this.emcDumpingGold) //false가 기본값 -> EMC Test Button 누르면서 true
            {
                if (e.Result != DecaturResult.Success)
                {
                    // Error dumping gold comparison file, try downloading it again
                    this.emcLogStream.Write(DateTime.Now.ToString("u")); //출력형식 변경_aej_230922
                    this.emcLogStream.WriteLine(",GOLD_FAILURE");
                    this.emcDevice.DumpToFile(this.emcGoldPath);

                    return;
                }

                this.emcLogStream.Write(DateTime.Now.ToString("u")); //출력형식 변경_aej_230922
                this.emcLogStream.WriteLine(",GOLD_SUCCESS");

                this.emcDumpingGold = false;
            }
            else
            {
                // Compare downloaded file to gold standard
                if (await EmcCompare())
                {
                    this.emcLogStream.Write(DateTime.Now.ToString("u")); //출력형식 변경_aej_230922
                    this.emcLogStream.WriteLine(",SUCCESS");

                    this.richTextBoxPages.BeginInvoke(new Action(() =>
                    {
                        this.richTextBoxPages.AppendText(DateTime.Now.ToString() + ": Success\n");
                    }));
                }
                else
                {
                    this.emcLogStream.Write(DateTime.Now.ToString("u")); //출력형식 변경_aej_230922
                    this.emcLogStream.WriteLine(",FAILURE");

                    this.richTextBoxPages.BeginInvoke(new Action(() =>
                    {
                        this.richTextBoxPages.AppendText(DateTime.Now.ToString() + ": Failure\n");
                    }));
                }

                if (File.Exists(this.emcTestPath))
                {
                    File.Delete(this.emcTestPath);
                }

                //break 추가?
            }

            this.emcTestPath = Path.GetTempFileName();

            this.emcDevice.DumpToFile(this.emcTestPath);
        }

        private void FormMain_Load(object sender, EventArgs e)  //close
        {
            this.monitorDetectTimer.Interval = 250;
            this.monitorDetectTimer.Tick += monitorDetectTimer_Tick;

            this.RefreshDeviceList();

            // dae0 : 20220701 : add version info. on title bar
            //this.Text = this.Text + " ( Version : " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " )";

            if (this.listViewDevices.Items.Count > 0)
            {
                this.listViewDevices.Items[0].Selected = true;
            }

        }

        void monitorDetectTimer_Tick(object sender, EventArgs e) //close
        {
            if (this.devices == null)
            {
                return;
            }

            foreach (var device in this.devices)
            {
                string presence;

                try
                {
                    presence = device.MonitorPresent.ToString();
                }
                catch (Exception)
                {
                    // Exception trying to detect the monitor. Rate limit monitor checks to avoid locking up waiting for timeouts
                    monitorDetectTimer.Interval = 30000;
                    return;
                }

                for (int index = 0; index < this.listViewDevices.Items.Count; index++)
                {
                    var item = this.listViewDevices.Items[index];

                    if (item.Text == device.SerialNumber)
                    {
                        if (item.SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text != presence)
                        {
                            item.SubItems[PAGES_AVAILABLE_COLUMN_INDEX].Text = device.PageCount.ToString();
                            item.SubItems[BYTES_AVAILABLE_COLUMN_INDEX].Text = device.BytesAvailable.ToString();
                            if (device.MonitorPresent)    // dae0 : 20220617 : flash type
                                item.SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text = device.MonitorType.ToString();
                            else
                                item.SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text = presence;

                            if (device.MonitorPresent)
                                item.Selected = true;

                            // dae0 : 20220613 :
                            item.SubItems[FW_VERSION_COLUMN_INDEX].Text = device.MonitorFWVersion.ToString();
                            item.SubItems[BLE_SERIAL_COLUMN_INDEX].Text = device.MonitorSerialNumber.ToString();
                            item.SubItems[ACTIVE_TIME_COLUMN_INDEX].Text = device.ActiveTime;

                            this.UpdateDataRanges();
                        }

                        break;
                    }
                }

                // dae0 : 20220706 :
                btnViewPost.Enabled = buttonViewPage.Enabled = device.MonitorPresent;
                buttonErase.Enabled = buttonDownload.Enabled = Bulk_Upload_Btn.Enabled = device.MonitorPresent;
                buttonDownloadRange.Enabled = btnDownloadAccel.Enabled = device.MonitorPresent;
            }
        }

        private void buttonErase_Click(object sender, EventArgs e) //close
        {
            var device = this.GetSelectedDevice();

            if (device == null)
            {
                MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (device.CurrentOperation != DecaturOperation.Idle)
            {
                MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (MessageBox.Show("This operation will erase all data stored on device " + device.SerialNumber + ". Would you like to continue?", "Erase Flash", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
            {
                DecaturResult result = device.EraseFlash();

                if (result == DecaturResult.Success)
                {
                    MessageBox.Show("Data was erased successfully.", "Erase Flash", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshDeviceList();
                }
                else
                {
                    MessageBox.Show("Error erasing flash: " + result.ToString(), "Erase Flash", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonViewPage_Click(object sender, EventArgs e) //close
        {
            var device = this.GetSelectedDevice();

            if (device == null)
            {
                MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (device.CurrentOperation != DecaturOperation.Idle)
            {
                MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            //if (numericUpDownPage.Value >= device.PageCount)
            //{
            //    MessageBox.Show("The selected device does not have that many pages. Connect a monitor with more pages or select a lower page number.", "Invalid Page Count", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //    return;
            //}

            var pages = device.RetrievePages(1, (uint)numericUpDownPage.Value);

            if (pages == null)
            {
                MessageBox.Show("ViewPage: Error downloading data: " + device.LastResult.ToString());
                return;
            }

            StringBuilder text = new StringBuilder();

            text.AppendLine("Offset    00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");

            for (int index = 0; index < pages[0].Length; index++)
            {
                if (index % 16 == 0)
                {
                    text.AppendLine(); //enter
                    text.Append(index.ToString("X8")); //Offset : 16진수 8자리 맞추기. 빈공간 0으로 채움.
                    text.Append("  ");
                }

                text.Append(pages[0][index].ToString("X2")); //하나의 byte를 16진수(Hex) 문자열로 변경
                text.Append(" ");
            }

            richTextBoxPages.Text = text.ToString();
        }

        private void UpdateDataRanges() //close
        {
            // Range validation removed to make it easier to read POST result page etc.

            //if (this.listViewDevices.SelectedIndices.Count <= 0 || this.listViewDevices.SelectedIndices[0] > this.devices.Count)
            //{
            //    return;
            //}

            //int maxPage = this.devices[this.listViewDevices.SelectedIndices[0]].PageCount - 1;

            //if (maxPage < 0)
            //{
            //    maxPage = 0;
            //}

            this.numericUpDownPage.Maximum = 500000;// maxPage;
            this.numericUpDownStartPage.Maximum = 500000;// maxPage;
            this.numericUpDownPageCount.Maximum = 500000;//maxPage + 1;
        }

        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateDataRanges();
        }

        private void buttonDownloadRange_Click(object sender, EventArgs e) //close
        {
            var device = this.GetSelectedDevice();

            if (device == null)
            {
                MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (device.CurrentOperation != DecaturOperation.Idle)
            {
                MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            //if (numericUpDownStartPage.Value + numericUpDownPageCount.Value > device.PageCount)
            //{
            //    MessageBox.Show("You must choose a range that does not exceed page " + device.PageCount.ToString() + ".", "Invalid Range", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //    return;
            //}

            if (this.saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) //solo & duet 모두 다운로드 가능하게 수정, 현재는 duet만 가능
            {
                if ((Path.GetExtension(this.saveFileDialog.FileName).ToUpper() == ".CSV") && (device.MonitorType == DecaturMonitorType.SOLO))
                {
                    MessageBox.Show("Only binary download is available for Solo!", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.labelProgress.Text = "0 / " + (numericUpDownPageCount.Value * device.MonitorPageSize(device.MonitorType)).ToString() + " Bytes Downloaded";
                this.toolStripStatusLabelOperation.Text = "Download " + numericUpDownPageCount.Value.ToString() + " Pages";
                device.RetrievePagesAsync((uint)numericUpDownPageCount.Value, (uint)numericUpDownStartPage.Value);
                this.operationStartTime = DateTime.Now;
            }
        }

        private void buttonEmcTest_Click(object sender, EventArgs e) //close
        {
            if (!this.emcTesting) //false가 초기값
            {
                this.emcDevice = this.GetSelectedDevice();

                if (this.emcDevice == null)
                {
                    MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                if (emcDevice.CurrentOperation != DecaturOperation.Idle)
                {
                    MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);


                    this.emcDevice = null;
                    return;
                }

                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                this.emcLogStream = new StreamWriter(File.Create(saveFileDialog.FileName));

                this.emcLogStream.Write(DateTime.Now.ToString("u")); //출력형식 변경_aej_230922
                this.emcLogStream.WriteLine(",BEGIN");

                buttonEmcTest.Text = "Stop EMC Test";

                this.emcGoldPath = Path.GetTempFileName();

                this.emcTesting = true;
                this.emcDumpingGold = true;

                this.richTextBoxPages.Focus();

                this.emcDevice.DumpToFile(this.emcGoldPath);
            }
            else
            {
                buttonEmcTest.Text = "Start EMC Test";

                this.emcTesting = false;
                this.emcTestEnding = true;

                if (this.emcGoldPath != null && File.Exists(this.emcGoldPath))
                {
                    File.Delete(this.emcGoldPath);
                    this.emcGoldPath = null;
                }

                if (this.emcLogStream != null)
                {
                    this.emcLogStream.Write(DateTime.Now.ToString("u")); //출력형식 변경_aej_230922
                    this.emcLogStream.WriteLine(",END");

                    this.emcLogStream.Close();
                }
            }
        }



        private void btnDownloadAccel_Click(object sender, EventArgs e)
        {
            savingIMU = true;
            buttonDownloadRange_Click(sender, e);
        }


        private void groupBoxAnalysis_Enter(object sender, EventArgs e)
        {

        }

        private void btnViewPost_Click(object sender, EventArgs e) //close
        {
            var device = this.GetSelectedDevice();

            numericUpDownPage.Value = 131071; //m300 post page는 last page

            // dae0 : 20220706 : add condition
            if (device.MonitorType != DecaturMonitorType.SOLO)
            {
                string[] tmp_version = device.MonitorFWVersion.Split('.');

                if (tmp_version[0].All(char.IsDigit))
                {
                    int version = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        version |= Convert.ToInt32(tmp_version[i]) << (8 * (2 - i));
                    }

                    if (version > 0x000200)
                        numericUpDownPage.Value = 0; //m400/dm500 post page는 0page
                }
            }

            buttonViewPage_Click(null, null);
        }

        private void FW_Upgrade_Btn_Click(object sender, EventArgs e) //close
        {
            // dae0 : 20220615 :
            var device = this.GetSelectedDevice();

            if (device == null)
            {
                MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (device.CurrentOperation != DecaturOperation.Idle)
            {
                MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            DecaturResult result = device.FW_Upgrade();

            if (result == DecaturResult.Success)
            {
                MessageBox.Show("Cable Test successfully.", "Cable Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Cable Test Fail " + result.ToString(), "Cable Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Bulk_Upload_Btn_Click(object sender, EventArgs e) //close
        {
            // dae0 : 20220615 :
            var device = this.GetSelectedDevice();

            if (device == null)
            {
                MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (device.CurrentOperation != DecaturOperation.Idle)
            {
                MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (this.openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // if ( (device.Revision < 30))  
                if (((device.Revision >> 8) & 0xFF) == 0)
                {
                    MessageBox.Show("Under (rev 1.0), binary upload is not supported!!!", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                this.labelProgress.Text = "0 / " + new FileInfo(this.openFileDialog.FileName).Length + " Bytes Uploaded "; //FileInfo.length -> 파일의 크기(바이트)를 가져옴
                this.toolStripStatusLabelOperation.Text = "Upload Data";
                if (Path.GetExtension(this.openFileDialog.FileName).ToUpper() == ".BIN")
                {
                    device.UploadFromFile(this.openFileDialog.FileName);
                }
                else if (Path.GetExtension(this.openFileDialog.FileName).ToUpper() == ".TXT")
                {
                    device.UploadFromTXTFile(this.openFileDialog.FileName);
                }
                this.operationStartTime = DateTime.Now;
            }
        }


        private void buttonViewPage_Enter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var device = this.GetSelectedDevice();

                if (device == null)
                {
                    MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                if (device.CurrentOperation != DecaturOperation.Idle)
                {
                    MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                //if (numericUpDownPage.Value >= device.PageCount)
                //{
                //    MessageBox.Show("The selected device does not have that many pages. Connect a monitor with more pages or select a lower page number.", "Invalid Page Count", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //    return;
                //}

                var pages = device.RetrievePages(1, (uint)numericUpDownPage.Value);

                if (pages == null)
                {
                    MessageBox.Show("ViewPage: Error downloading data: " + device.LastResult.ToString());
                    return;
                }

                StringBuilder text = new StringBuilder();

                text.AppendLine("Offset    00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");

                for (int index = 0; index < pages[0].Length; index++)
                {
                    if (index % 16 == 0)
                    {
                        text.AppendLine(); //enter
                        text.Append(index.ToString("X8")); //Offset : 16진수 8자리 맞추기. 빈공간 0으로 채움.
                        text.Append("  ");
                    }

                    text.Append(pages[0][index].ToString("X2")); //하나의 byte를 16진수(Hex) 문자열로 변경
                    text.Append(" ");
                }

                richTextBoxPages.Text = text.ToString();
            }
        }

        private void View_Analysis_Button_Click(object sender, EventArgs e)
        {
            var device = this.GetSelectedDevice();

            if (device == null)
            {
                MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (device.CurrentOperation != DecaturOperation.Idle)
            {
                MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            this.labelProgress.Text = "0 / " + (numericUpDownPageCount.Value * device.MonitorPageSize(device.MonitorType)).ToString() + " Bytes Downloaded";
            device.RetrieveViewPagesAsync((uint)numericUpDownPageCount.Value, (uint)numericUpDownStartPage.Value);
            this.operationStartTime = DateTime.Now;

            rangeStartpage = Convert.ToInt32(numericUpDownStartPage.Value);
            int totalsec_startpage;

            if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
            {
                totalsec_startpage = (int)(rangeStartpage * 5.34);
                textBoxRangeStartDay.Text = (totalsec_startpage / (24 * 60 * 60)).ToString();
                textBoxRangeStartHour.Text = ((totalsec_startpage % (24 * 60 * 60)) / (60 * 60)).ToString();
                textBoxRangeStartMin.Text = ((totalsec_startpage % (60 * 60)) / 60).ToString();
                textBoxRangeStartSec.Text = (totalsec_startpage % 60).ToString();
            }
            else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
            {
                totalsec_startpage = (int)(rangeStartpage * 6.76);
                textBoxRangeStartDay.Text = (totalsec_startpage / (24 * 60 * 60)).ToString();
                textBoxRangeStartHour.Text = ((totalsec_startpage % (24 * 60 * 60)) / (60 * 60)).ToString();
                textBoxRangeStartMin.Text = ((totalsec_startpage % (60 * 60)) / 60).ToString();
                textBoxRangeStartSec.Text = (totalsec_startpage % 60).ToString();
            }
            else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
            {
                totalsec_startpage = (int)(rangeStartpage * 6.76);
                textBoxRangeStartDay.Text = (totalsec_startpage / (24 * 60 * 60)).ToString();
                textBoxRangeStartHour.Text = ((totalsec_startpage % (24 * 60 * 60)) / (60 * 60)).ToString();
                textBoxRangeStartMin.Text = ((totalsec_startpage % (60 * 60)) / 60).ToString();
                textBoxRangeStartSec.Text = (totalsec_startpage % 60).ToString();
            }

            rangepage = Convert.ToInt32(numericUpDownStartPage.Value) + Convert.ToInt32(numericUpDownPageCount.Value);

            int totalsec_page;

            if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
            {
                totalsec_page = (int)(rangepage * 5.34);
                textBoxRangeEndDay.Text = (totalsec_page / (24 * 60 * 60)).ToString();
                textBoxRangeEndHour.Text = ((totalsec_page % (24 * 60 * 60)) / (60 * 60)).ToString();
                textBoxRangeEndMin.Text = ((totalsec_page % (60 * 60)) / 60).ToString();
                textBoxRangeEndSec.Text = (totalsec_page % 60).ToString();
            }
            else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
            {
                totalsec_page = (int)(rangepage * 6.76);
                textBoxRangeEndDay.Text = (totalsec_page / (24 * 60 * 60)).ToString();
                textBoxRangeEndHour.Text = ((totalsec_page % (24 * 60 * 60)) / (60 * 60)).ToString();
                textBoxRangeEndMin.Text = ((totalsec_page % (60 * 60)) / 60).ToString();
                textBoxRangeEndSec.Text = (totalsec_page % 60).ToString();
            }
            else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
            {
                totalsec_page = (int)(rangepage * 6.76);
                textBoxRangeEndDay.Text = (totalsec_page / (24 * 60 * 60)).ToString();
                textBoxRangeEndHour.Text = ((totalsec_page % (24 * 60 * 60)) / (60 * 60)).ToString();
                textBoxRangeEndMin.Text = ((totalsec_page % (60 * 60)) / 60).ToString();
                textBoxRangeEndSec.Text = (totalsec_page % 60).ToString();
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            var device = this.GetSelectedDevice();

            if (device == null)
            {
                MessageBox.Show("You must select a valid device from the connected devices list first.", "Invalid Device", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (device.CurrentOperation != DecaturOperation.Idle)
            {
                MessageBox.Show("The selected device is busy. You must wait for the current operation to complete before starting another one.", "Invalid State", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (this.saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.labelProgress.Text = "0 / " + device.BytesAvailable.ToString() + " Bytes Downloaded";
                this.toolStripStatusLabelOperation.Text = "Download Data";
                if (Path.GetExtension(this.saveFileDialog.FileName).ToUpper() == ".BIN")
                {
                    device.DumpToFile(this.saveFileDialog.FileName);
                }
                else if (Path.GetExtension(this.saveFileDialog.FileName).ToUpper() == ".TXT")
                {
                    device.DumpToCSVFile(this.saveFileDialog.FileName);
                }

                this.operationStartTime = DateTime.Now;
            }
        }


        /*
        private void textBoxSingleDay_TextChanged(object sender, EventArgs e)
        {
            int sec, day;
            if (textBoxSingleDay.Text != "")
            {
                day = Convert.ToInt32(textBoxSingleDay.Text);
                sec = day * 24 * 60 * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    numericUpDownPage.Value += (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    numericUpDownPage.Value += (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    numericUpDownPage.Value += (int)(sec / 6.76);
                }
            }
        }
        */
        /*
        private void textBoxSingleHour_TextChanged(object sender, EventArgs e)
        {
            int sec, hour;
            //if(textBoxSingleHour.KeyUp ==)
            if (textBoxSingleDay.Text != "")
            {
                hour = Convert.ToInt32(textBoxSingleDay.Text);
                sec = hour * 60 * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    numericUpDownPage.Value += (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    numericUpDownPage.Value += (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    numericUpDownPage.Value += (int)(sec / 6.76);
                }
            }
        }
        */
        /*
        private void textBoxSingleDay_KeyPress(object sender, KeyPressEventArgs e)
        {
            int day, sec;
            if (char.IsDigit(e.KeyChar))
            {
                
                day = Convert.ToInt32(textBoxSingleDay.Text);
                sec = day * 24 * 60 * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    numericUpDownPage.Value -= (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    numericUpDownPage.Value -= (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    numericUpDownPage.Value -= (int)(sec / 6.76);
                }

               // day_sec = 0;
            }
        
        }
        */
        private void textBoxRangeStartDay_KeyUp(object sender, KeyEventArgs e)
        {
            int day, sec;

            if (e.KeyCode == Keys.Enter && textBoxRangeStartDay.Text != "")
            {
                day = Convert.ToInt32(textBoxRangeStartDay.Text);
                sec = day * 24 * 60 * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    rangeStartpage = (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    rangeStartpage = (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    rangeStartpage = (int)(sec / 6.76);
                }

                this.ActiveControl = textBoxRangeStartHour;
            }
            if (e.KeyCode == Keys.Escape)
            {
                textBoxRangeStartDay.Text = textBoxRangeStartHour.Text = textBoxRangeStartMin.Text = textBoxRangeStartSec.Text = "";
                numericUpDownStartPage.Value = 0;
            }
        }

        private void textBoxRangeStartHour_KeyUp(object sender, KeyEventArgs e)
        {
            int hour, sec;

            if (e.KeyCode == Keys.Enter && textBoxRangeStartHour.Text != "")
            {
                hour = Convert.ToInt32(textBoxRangeStartHour.Text);
                sec = hour * 60 * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    rangeStartpage += (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    rangeStartpage += (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    rangeStartpage += (int)(sec / 6.76);
                }

                this.ActiveControl = textBoxRangeStartMin;
            }
        }

        private void textBoxRangeStartMin_KeyUp(object sender, KeyEventArgs e)
        {
            int min, sec;

            if (e.KeyCode == Keys.Enter && textBoxRangeStartMin.Text != "")
            {
                min = Convert.ToInt32(textBoxRangeStartMin.Text);
                sec = min * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    rangeStartpage += (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    rangeStartpage += (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    rangeStartpage += (int)(sec / 6.76);
                }

                this.ActiveControl = textBoxRangeStartSec;
            }
        }

        private void textBoxRangeStartSec_KeyUp(object sender, KeyEventArgs e)
        {
            int sec;

            if (e.KeyCode == Keys.Enter && textBoxRangeStartSec.Text != "")
            {
                sec = Convert.ToInt32(textBoxRangeStartSec.Text);


                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    rangeStartpage += (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    rangeStartpage += (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    rangeStartpage += (int)(sec / 6.76);
                }

                numericUpDownStartPage.Value = rangeStartpage;
                numericUpDownPageCount.Value = numericUpDownStartPage.Value - rangepage;

                this.ActiveControl = textBoxRangeEndDay;
            }
        }

        private void textBoxRangeEndDay_KeyUp(object sender, KeyEventArgs e)
        {
            int day, sec;

            if (e.KeyCode == Keys.Enter && textBoxRangeEndDay.Text != "")
            {
                day = Convert.ToInt32(textBoxRangeEndDay.Text);
                sec = day * 24 * 60 * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    rangepage = (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    rangepage = (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    rangepage = (int)(sec / 6.76);
                }

                this.ActiveControl = textBoxRangeEndHour;
            }
            if(e.KeyCode == Keys.Escape)
            {
                textBoxRangeEndDay.Text = textBoxRangeEndHour.Text = textBoxRangeEndMin.Text = textBoxRangeEndSec.Text = "";
                rangepage = 0;
            }
        }

        private void textBoxRangeEndHour_KeyUp(object sender, KeyEventArgs e)
        {
            int hour, sec;

            
            if (e.KeyCode == Keys.Enter && textBoxRangeEndHour.Text != "")
            {
                hour = Convert.ToInt32(textBoxRangeEndHour.Text);
                sec = hour * 60 * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    rangepage += (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    rangepage += (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    rangepage += (int)(sec / 6.76);
                }

                this.ActiveControl = textBoxRangeEndMin;
            }
        }

        private void textBoxRangeEndMin_KeyUp(object sender, KeyEventArgs e)
        {
            int min, sec;

            
            if (e.KeyCode == Keys.Enter && textBoxRangeEndMin.Text != "")
            {
                min = Convert.ToInt32(textBoxRangeEndMin.Text);
                sec = min * 60;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    rangepage += (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    rangepage += (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    rangepage += (int)(sec / 6.76);
                }

                this.ActiveControl = textBoxRangeEndSec;
            }
        }

        private void textBoxRangeEndSec_KeyUp(object sender, KeyEventArgs e)
        {
            int sec;

            if (e.KeyCode == Keys.Enter && textBoxRangeEndSec.Text != "")
            {
                sec = Convert.ToInt32(textBoxRangeEndSec.Text);


                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    rangepage += (int)(sec / 5.34);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    rangepage += (int)(sec / 6.76);
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    rangepage += (int)(sec / 6.76);
                }

            }

            
            numericUpDownPageCount.Value = rangepage - numericUpDownStartPage.Value;

            this.ActiveControl = numericUpDownPageCount;
        }

        private void numericUpDownStartPage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                rangeStartpage = Convert.ToInt32(numericUpDownStartPage.Value);
                int totalsec;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    totalsec = (int)(rangeStartpage * 5.34);
                    textBoxRangeStartDay.Text = (totalsec / (24 * 60 * 60)).ToString();
                    textBoxRangeStartHour.Text = ((totalsec % (24 * 60 * 60)) / (60 * 60)).ToString();
                    textBoxRangeStartMin.Text = ((totalsec % (60 * 60)) / 60).ToString();
                    textBoxRangeStartSec.Text = (totalsec % 60).ToString();
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    totalsec = (int)(rangeStartpage * 6.76);
                    textBoxRangeStartDay.Text = (totalsec / (24 * 60 * 60)).ToString();
                    textBoxRangeStartHour.Text = ((totalsec % (24 * 60 * 60)) / (60 * 60)).ToString();
                    textBoxRangeStartMin.Text = ((totalsec % (60 * 60)) / 60).ToString();
                    textBoxRangeStartSec.Text = (totalsec % 60).ToString();
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    totalsec = (int)(rangeStartpage * 6.76);
                    textBoxRangeStartDay.Text = (totalsec / (24 * 60 * 60)).ToString();
                    textBoxRangeStartHour.Text = ((totalsec % (24 * 60 * 60)) / (60 * 60)).ToString();
                    textBoxRangeStartMin.Text = ((totalsec % (60 * 60)) / 60).ToString();
                    textBoxRangeStartSec.Text = (totalsec % 60).ToString();
                }

            }
        }

        private void numericUpDownPageCount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                rangepage = Convert.ToInt32(numericUpDownStartPage.Value) + Convert.ToInt32(numericUpDownPageCount.Value);

                int totalsec;

                if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "SOLO")
                {
                    totalsec = (int)(rangepage * 5.34);
                    textBoxRangeEndDay.Text = (totalsec / (24 * 60 * 60)).ToString();
                    textBoxRangeEndHour.Text = ((totalsec % (24 * 60 * 60)) / (60 * 60)).ToString();
                    textBoxRangeEndMin.Text = ((totalsec % (60 * 60)) / 60).ToString();
                    textBoxRangeEndSec.Text = (totalsec % 60).ToString();
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "ADX_4GB")
                {
                    totalsec = (int)(rangepage * 6.76);
                    textBoxRangeEndDay.Text = (totalsec / (24 * 60 * 60)).ToString();
                    textBoxRangeEndHour.Text = ((totalsec % (24 * 60 * 60)) / (60 * 60)).ToString();
                    textBoxRangeEndMin.Text = ((totalsec % (60 * 60)) / 60).ToString();
                    textBoxRangeEndSec.Text = (totalsec % 60).ToString();
                }
                else if (listViewDevices.Items[0].SubItems[MONITOR_PRESENT_COLUMN_INDEX].Text == "DM500")
                {
                    totalsec = (int)(rangepage * 6.76);
                    textBoxRangeEndDay.Text = (totalsec / (24 * 60 * 60)).ToString();
                    textBoxRangeEndHour.Text = ((totalsec % (24 * 60 * 60)) / (60 * 60)).ToString();
                    textBoxRangeEndMin.Text = ((totalsec % (60 * 60)) / 60).ToString();
                    textBoxRangeEndSec.Text = (totalsec % 60).ToString();
                }

            }
        }
    }
}
