namespace DecaturTest
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonListDevices = new System.Windows.Forms.Button();
            this.listViewDevices = new System.Windows.Forms.ListView();
            this.columnHeaderSerial = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRevision = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPages = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMonitorPresent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMonitorFirmWare = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMonitorSerial = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeaderActiveTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.progressBarOperation = new System.Windows.Forms.ProgressBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelOperationText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelOperation = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTimeValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelProgress = new System.Windows.Forms.Label();
            this.richTextBoxPages = new System.Windows.Forms.RichTextBox();
            this.groupBoxAllPages = new System.Windows.Forms.GroupBox();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.Bulk_Upload_Btn = new System.Windows.Forms.Button();
            this.buttonErase = new System.Windows.Forms.Button();
            this.groupBoxPageRange = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.textBoxRangeEndSec = new System.Windows.Forms.TextBox();
            this.textBoxRangeEndMin = new System.Windows.Forms.TextBox();
            this.textBoxRangeEndHour = new System.Windows.Forms.TextBox();
            this.textBoxRangeEndDay = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.textBoxRangeStartSec = new System.Windows.Forms.TextBox();
            this.textBoxRangeStartMin = new System.Windows.Forms.TextBox();
            this.textBoxRangeStartHour = new System.Windows.Forms.TextBox();
            this.textBoxRangeStartDay = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.View_Analysis_Button = new System.Windows.Forms.Button();
            this.btnDownloadAccel = new System.Windows.Forms.Button();
            this.RichTextBox_LeadOffPages = new System.Windows.Forms.RichTextBox();
            this.buttonDownloadRange = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownPageCount = new System.Windows.Forms.NumericUpDown();
            this.labelStartPage = new System.Windows.Forms.Label();
            this.numericUpDownStartPage = new System.Windows.Forms.NumericUpDown();
            this.groupBoxSinglePage = new System.Windows.Forms.GroupBox();
            this.btnViewPost = new System.Windows.Forms.Button();
            this.labelSinglePage = new System.Windows.Forms.Label();
            this.buttonViewPage = new System.Windows.Forms.Button();
            this.numericUpDownPage = new System.Windows.Forms.NumericUpDown();
            this.groupBoxAnalysis = new System.Windows.Forms.GroupBox();
            this.textBoxAnalysisEcg1 = new System.Windows.Forms.TextBox();
            this.textBoxAnalysisPeaks1 = new System.Windows.Forms.TextBox();
            this.textBoxAnalysisNoise1 = new System.Windows.Forms.TextBox();
            this.textBoxAnalysisButton = new System.Windows.Forms.TextBox();
            this.labelAnalysisButton = new System.Windows.Forms.Label();
            this.textBoxAnalysisPeaks = new System.Windows.Forms.TextBox();
            this.labelAnalysisPeaks = new System.Windows.Forms.Label();
            this.textBoxAnalysisNoise = new System.Windows.Forms.TextBox();
            this.labelAnalysisNoise = new System.Windows.Forms.Label();
            this.textBoxAnalysisEcg = new System.Windows.Forms.TextBox();
            this.labelAnalysisEcg = new System.Windows.Forms.Label();
            this.textBoxAnalysisBattery = new System.Windows.Forms.TextBox();
            this.labelAnalysisBattery = new System.Windows.Forms.Label();
            this.textBoxAnalysisHz = new System.Windows.Forms.TextBox();
            this.labelAnalysisHz = new System.Windows.Forms.Label();
            this.textBoxAnalysisClock = new System.Windows.Forms.TextBox();
            this.labelAnalysisClock = new System.Windows.Forms.Label();
            this.buttonEmcTest = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.FW_Upgrade = new System.Windows.Forms.Button();
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.statusStrip1.SuspendLayout();
            this.groupBoxAllPages.SuspendLayout();
            this.groupBoxPageRange.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPageCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartPage)).BeginInit();
            this.groupBoxSinglePage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPage)).BeginInit();
            this.groupBoxAnalysis.SuspendLayout();
            this.groupBoxGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonListDevices
            // 
            this.buttonListDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonListDevices.Location = new System.Drawing.Point(4, 126);
            this.buttonListDevices.Name = "buttonListDevices";
            this.buttonListDevices.Size = new System.Drawing.Size(118, 24);
            this.buttonListDevices.TabIndex = 1;
            this.buttonListDevices.Text = "Refresh Devices";
            this.buttonListDevices.UseVisualStyleBackColor = true;
            this.buttonListDevices.Click += new System.EventHandler(this.buttonListDevices_Click);
            // 
            // listViewDevices
            // 
            this.listViewDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewDevices.AutoArrange = false;
            this.listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSerial,
            this.columnHeaderRevision,
            this.columnHeaderPages,
            this.columnHeaderBytes,
            this.columnHeaderMonitorPresent,
            this.columnHeaderMonitorFirmWare,
            this.columnHeaderMonitorSerial,
            this.ColumnHeaderActiveTime});
            this.listViewDevices.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewDevices.FullRowSelect = true;
            this.listViewDevices.HideSelection = false;
            this.listViewDevices.Location = new System.Drawing.Point(11, 13);
            this.listViewDevices.MultiSelect = false;
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.Size = new System.Drawing.Size(835, 72);
            this.listViewDevices.TabIndex = 2;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            this.listViewDevices.View = System.Windows.Forms.View.Details;
            this.listViewDevices.SelectedIndexChanged += new System.EventHandler(this.listViewDevices_SelectedIndexChanged);
            // 
            // columnHeaderSerial
            // 
            this.columnHeaderSerial.Text = "Serial Number";
            this.columnHeaderSerial.Width = 102;
            // 
            // columnHeaderRevision
            // 
            this.columnHeaderRevision.Text = "Revision";
            this.columnHeaderRevision.Width = 63;
            // 
            // columnHeaderPages
            // 
            this.columnHeaderPages.Text = "Pages Available";
            this.columnHeaderPages.Width = 103;
            // 
            // columnHeaderBytes
            // 
            this.columnHeaderBytes.Text = "Bytes Available";
            this.columnHeaderBytes.Width = 101;
            // 
            // columnHeaderMonitorPresent
            // 
            this.columnHeaderMonitorPresent.Text = "Monitor Present";
            this.columnHeaderMonitorPresent.Width = 107;
            // 
            // columnHeaderMonitorFirmWare
            // 
            this.columnHeaderMonitorFirmWare.Text = "FW Version";
            this.columnHeaderMonitorFirmWare.Width = 82;
            // 
            // columnHeaderMonitorSerial
            // 
            this.columnHeaderMonitorSerial.Text = "Monitor Serial";
            this.columnHeaderMonitorSerial.Width = 130;
            // 
            // ColumnHeaderActiveTime
            // 
            this.ColumnHeaderActiveTime.Text = "ActiveTime";
            this.ColumnHeaderActiveTime.Width = 100;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Binary File|*.bin|Text File|*.txt|CSV File|*.csv";
            // 
            // progressBarOperation
            // 
            this.progressBarOperation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarOperation.Location = new System.Drawing.Point(13, 346);
            this.progressBarOperation.Name = "progressBarOperation";
            this.progressBarOperation.Size = new System.Drawing.Size(833, 24);
            this.progressBarOperation.TabIndex = 4;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelOperationText,
            this.toolStripStatusLabelOperation,
            this.toolStripStatusLabelTime,
            this.toolStripStatusLabelTimeValue});
            this.statusStrip1.Location = new System.Drawing.Point(0, 543);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 11, 0);
            this.statusStrip1.Size = new System.Drawing.Size(854, 26);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip";
            // 
            // toolStripStatusLabelOperationText
            // 
            this.toolStripStatusLabelOperationText.Name = "toolStripStatusLabelOperationText";
            this.toolStripStatusLabelOperationText.Size = new System.Drawing.Size(137, 20);
            this.toolStripStatusLabelOperationText.Text = "Current Operation:";
            // 
            // toolStripStatusLabelOperation
            // 
            this.toolStripStatusLabelOperation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelOperation.Name = "toolStripStatusLabelOperation";
            this.toolStripStatusLabelOperation.Size = new System.Drawing.Size(35, 20);
            this.toolStripStatusLabelOperation.Text = "Idle";
            // 
            // toolStripStatusLabelTime
            // 
            this.toolStripStatusLabelTime.Name = "toolStripStatusLabelTime";
            this.toolStripStatusLabelTime.Size = new System.Drawing.Size(150, 20);
            this.toolStripStatusLabelTime.Text = "Last Operation Time:";
            // 
            // toolStripStatusLabelTimeValue
            // 
            this.toolStripStatusLabelTimeValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelTimeValue.Name = "toolStripStatusLabelTimeValue";
            this.toolStripStatusLabelTimeValue.Size = new System.Drawing.Size(39, 20);
            this.toolStripStatusLabelTimeValue.Text = "N/A";
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProgress.Location = new System.Drawing.Point(10, 319);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(833, 24);
            this.labelProgress.TabIndex = 6;
            this.labelProgress.Text = "0 / 0 Bytes Downloaded";
            this.labelProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // richTextBoxPages
            // 
            this.richTextBoxPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxPages.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxPages.Location = new System.Drawing.Point(13, 94);
            this.richTextBoxPages.Name = "richTextBoxPages";
            this.richTextBoxPages.ReadOnly = true;
            this.richTextBoxPages.Size = new System.Drawing.Size(830, 142);
            this.richTextBoxPages.TabIndex = 8;
            this.richTextBoxPages.Text = "";
            // 
            // groupBoxAllPages
            // 
            this.groupBoxAllPages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAllPages.Controls.Add(this.buttonDownload);
            this.groupBoxAllPages.Controls.Add(this.Bulk_Upload_Btn);
            this.groupBoxAllPages.Controls.Add(this.buttonErase);
            this.groupBoxAllPages.Location = new System.Drawing.Point(751, 384);
            this.groupBoxAllPages.Name = "groupBoxAllPages";
            this.groupBoxAllPages.Size = new System.Drawing.Size(87, 161);
            this.groupBoxAllPages.TabIndex = 11;
            this.groupBoxAllPages.TabStop = false;
            this.groupBoxAllPages.Text = "All Pages";
            // 
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Location = new System.Drawing.Point(7, 127);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(74, 23);
            this.buttonDownload.TabIndex = 22;
            this.buttonDownload.Text = "Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // Bulk_Upload_Btn
            // 
            this.Bulk_Upload_Btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Bulk_Upload_Btn.Location = new System.Drawing.Point(7, 91);
            this.Bulk_Upload_Btn.Name = "Bulk_Upload_Btn";
            this.Bulk_Upload_Btn.Size = new System.Drawing.Size(74, 23);
            this.Bulk_Upload_Btn.TabIndex = 21;
            this.Bulk_Upload_Btn.Text = "Upload";
            this.Bulk_Upload_Btn.UseVisualStyleBackColor = true;
            this.Bulk_Upload_Btn.Click += new System.EventHandler(this.Bulk_Upload_Btn_Click);
            // 
            // buttonErase
            // 
            this.buttonErase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonErase.Location = new System.Drawing.Point(7, 55);
            this.buttonErase.Name = "buttonErase";
            this.buttonErase.Size = new System.Drawing.Size(74, 23);
            this.buttonErase.TabIndex = 9;
            this.buttonErase.Text = "Erase";
            this.buttonErase.UseVisualStyleBackColor = true;
            this.buttonErase.Click += new System.EventHandler(this.buttonErase_Click);
            // 
            // groupBoxPageRange
            // 
            this.groupBoxPageRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPageRange.Controls.Add(this.panel2);
            this.groupBoxPageRange.Controls.Add(this.panel3);
            this.groupBoxPageRange.Controls.Add(this.label4);
            this.groupBoxPageRange.Controls.Add(this.label3);
            this.groupBoxPageRange.Controls.Add(this.label2);
            this.groupBoxPageRange.Controls.Add(this.View_Analysis_Button);
            this.groupBoxPageRange.Controls.Add(this.btnDownloadAccel);
            this.groupBoxPageRange.Controls.Add(this.RichTextBox_LeadOffPages);
            this.groupBoxPageRange.Controls.Add(this.buttonDownloadRange);
            this.groupBoxPageRange.Controls.Add(this.label1);
            this.groupBoxPageRange.Controls.Add(this.numericUpDownPageCount);
            this.groupBoxPageRange.Controls.Add(this.labelStartPage);
            this.groupBoxPageRange.Controls.Add(this.numericUpDownStartPage);
            this.groupBoxPageRange.Location = new System.Drawing.Point(314, 378);
            this.groupBoxPageRange.Name = "groupBoxPageRange";
            this.groupBoxPageRange.Size = new System.Drawing.Size(420, 167);
            this.groupBoxPageRange.TabIndex = 12;
            this.groupBoxPageRange.TabStop = false;
            this.groupBoxPageRange.Text = "Page Range";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Window;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.textBoxRangeEndSec);
            this.panel2.Controls.Add(this.textBoxRangeEndMin);
            this.panel2.Controls.Add(this.textBoxRangeEndHour);
            this.panel2.Controls.Add(this.textBoxRangeEndDay);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Location = new System.Drawing.Point(279, 49);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(103, 22);
            this.panel2.TabIndex = 29;
            // 
            // textBoxRangeEndSec
            // 
            this.textBoxRangeEndSec.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxRangeEndSec.Location = new System.Drawing.Point(80, 3);
            this.textBoxRangeEndSec.Name = "textBoxRangeEndSec";
            this.textBoxRangeEndSec.Size = new System.Drawing.Size(18, 19);
            this.textBoxRangeEndSec.TabIndex = 3;
            this.textBoxRangeEndSec.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRangeEndSec.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRangeEndSec_KeyUp);
            // 
            // textBoxRangeEndMin
            // 
            this.textBoxRangeEndMin.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxRangeEndMin.Location = new System.Drawing.Point(55, 3);
            this.textBoxRangeEndMin.Name = "textBoxRangeEndMin";
            this.textBoxRangeEndMin.Size = new System.Drawing.Size(18, 19);
            this.textBoxRangeEndMin.TabIndex = 2;
            this.textBoxRangeEndMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRangeEndMin.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRangeEndMin_KeyUp);
            // 
            // textBoxRangeEndHour
            // 
            this.textBoxRangeEndHour.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxRangeEndHour.Location = new System.Drawing.Point(30, 3);
            this.textBoxRangeEndHour.Name = "textBoxRangeEndHour";
            this.textBoxRangeEndHour.Size = new System.Drawing.Size(18, 19);
            this.textBoxRangeEndHour.TabIndex = 1;
            this.textBoxRangeEndHour.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRangeEndHour.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRangeEndHour_KeyUp);
            // 
            // textBoxRangeEndDay
            // 
            this.textBoxRangeEndDay.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxRangeEndDay.Location = new System.Drawing.Point(5, 3);
            this.textBoxRangeEndDay.Name = "textBoxRangeEndDay";
            this.textBoxRangeEndDay.Size = new System.Drawing.Size(18, 19);
            this.textBoxRangeEndDay.TabIndex = 0;
            this.textBoxRangeEndDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRangeEndDay.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRangeEndDay_KeyUp);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(18, -1);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(19, 25);
            this.label9.TabIndex = 26;
            this.label9.Text = ":";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(69, -1);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(19, 25);
            this.label10.TabIndex = 28;
            this.label10.Text = ":";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(43, -1);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(19, 25);
            this.label11.TabIndex = 27;
            this.label11.Text = ":";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.Window;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.textBoxRangeStartSec);
            this.panel3.Controls.Add(this.textBoxRangeStartMin);
            this.panel3.Controls.Add(this.textBoxRangeStartHour);
            this.panel3.Controls.Add(this.textBoxRangeStartDay);
            this.panel3.Controls.Add(this.label12);
            this.panel3.Controls.Add(this.label13);
            this.panel3.Controls.Add(this.label14);
            this.panel3.Location = new System.Drawing.Point(279, 19);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(103, 22);
            this.panel3.TabIndex = 30;
            // 
            // textBoxRangeStartSec
            // 
            this.textBoxRangeStartSec.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxRangeStartSec.Location = new System.Drawing.Point(80, 3);
            this.textBoxRangeStartSec.Name = "textBoxRangeStartSec";
            this.textBoxRangeStartSec.Size = new System.Drawing.Size(18, 19);
            this.textBoxRangeStartSec.TabIndex = 3;
            this.textBoxRangeStartSec.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRangeStartSec.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRangeStartSec_KeyUp);
            // 
            // textBoxRangeStartMin
            // 
            this.textBoxRangeStartMin.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxRangeStartMin.Location = new System.Drawing.Point(55, 3);
            this.textBoxRangeStartMin.Name = "textBoxRangeStartMin";
            this.textBoxRangeStartMin.Size = new System.Drawing.Size(18, 19);
            this.textBoxRangeStartMin.TabIndex = 2;
            this.textBoxRangeStartMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRangeStartMin.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRangeStartMin_KeyUp);
            // 
            // textBoxRangeStartHour
            // 
            this.textBoxRangeStartHour.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxRangeStartHour.Location = new System.Drawing.Point(30, 3);
            this.textBoxRangeStartHour.Name = "textBoxRangeStartHour";
            this.textBoxRangeStartHour.Size = new System.Drawing.Size(18, 19);
            this.textBoxRangeStartHour.TabIndex = 1;
            this.textBoxRangeStartHour.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRangeStartHour.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRangeStartHour_KeyUp);
            // 
            // textBoxRangeStartDay
            // 
            this.textBoxRangeStartDay.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxRangeStartDay.Location = new System.Drawing.Point(5, 3);
            this.textBoxRangeStartDay.Name = "textBoxRangeStartDay";
            this.textBoxRangeStartDay.Size = new System.Drawing.Size(18, 19);
            this.textBoxRangeStartDay.TabIndex = 0;
            this.textBoxRangeStartDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRangeStartDay.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRangeStartDay_KeyUp);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(18, -1);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(19, 25);
            this.label12.TabIndex = 26;
            this.label12.Text = ":";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(69, -1);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(19, 25);
            this.label13.TabIndex = 28;
            this.label13.Text = ":";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(43, -1);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(19, 25);
            this.label14.TabIndex = 27;
            this.label14.Text = ":";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(199, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 17);
            this.label4.TabIndex = 24;
            this.label4.Text = "End Time :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(199, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 17);
            this.label3.TabIndex = 23;
            this.label3.Text = "Start Time :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(199, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 17);
            this.label2.TabIndex = 21;
            this.label2.Text = "Lead Off :";
            // 
            // View_Analysis_Button
            // 
            this.View_Analysis_Button.Location = new System.Drawing.Point(6, 140);
            this.View_Analysis_Button.Name = "View_Analysis_Button";
            this.View_Analysis_Button.Size = new System.Drawing.Size(165, 23);
            this.View_Analysis_Button.TabIndex = 19;
            this.View_Analysis_Button.Text = "View Analysis";
            this.View_Analysis_Button.UseVisualStyleBackColor = true;
            this.View_Analysis_Button.Click += new System.EventHandler(this.View_Analysis_Button_Click);
            // 
            // btnDownloadAccel
            // 
            this.btnDownloadAccel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownloadAccel.Location = new System.Drawing.Point(7, 113);
            this.btnDownloadAccel.Name = "btnDownloadAccel";
            this.btnDownloadAccel.Size = new System.Drawing.Size(165, 23);
            this.btnDownloadAccel.TabIndex = 18;
            this.btnDownloadAccel.Text = "Accel";
            this.btnDownloadAccel.UseVisualStyleBackColor = true;
            this.btnDownloadAccel.Click += new System.EventHandler(this.btnDownloadAccel_Click);
            // 
            // RichTextBox_LeadOffPages
            // 
            this.RichTextBox_LeadOffPages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RichTextBox_LeadOffPages.Location = new System.Drawing.Point(279, 86);
            this.RichTextBox_LeadOffPages.Name = "RichTextBox_LeadOffPages";
            this.RichTextBox_LeadOffPages.Size = new System.Drawing.Size(103, 70);
            this.RichTextBox_LeadOffPages.TabIndex = 20;
            this.RichTextBox_LeadOffPages.Text = "";
            // 
            // buttonDownloadRange
            // 
            this.buttonDownloadRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownloadRange.Location = new System.Drawing.Point(7, 84);
            this.buttonDownloadRange.Name = "buttonDownloadRange";
            this.buttonDownloadRange.Size = new System.Drawing.Size(164, 23);
            this.buttonDownloadRange.TabIndex = 17;
            this.buttonDownloadRange.Text = "Download";
            this.buttonDownloadRange.UseVisualStyleBackColor = true;
            this.buttonDownloadRange.Click += new System.EventHandler(this.buttonDownloadRange_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 17);
            this.label1.TabIndex = 16;
            this.label1.Text = "Page Count:";
            // 
            // numericUpDownPageCount
            // 
            this.numericUpDownPageCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownPageCount.Location = new System.Drawing.Point(88, 51);
            this.numericUpDownPageCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPageCount.Name = "numericUpDownPageCount";
            this.numericUpDownPageCount.Size = new System.Drawing.Size(83, 26);
            this.numericUpDownPageCount.TabIndex = 15;
            this.numericUpDownPageCount.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownPageCount.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numericUpDownPageCount_KeyUp);
            // 
            // labelStartPage
            // 
            this.labelStartPage.AutoSize = true;
            this.labelStartPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStartPage.Location = new System.Drawing.Point(4, 24);
            this.labelStartPage.Name = "labelStartPage";
            this.labelStartPage.Size = new System.Drawing.Size(90, 17);
            this.labelStartPage.TabIndex = 14;
            this.labelStartPage.Text = "Start Page:";
            // 
            // numericUpDownStartPage
            // 
            this.numericUpDownStartPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownStartPage.Location = new System.Drawing.Point(89, 19);
            this.numericUpDownStartPage.Name = "numericUpDownStartPage";
            this.numericUpDownStartPage.Size = new System.Drawing.Size(83, 26);
            this.numericUpDownStartPage.TabIndex = 13;
            this.numericUpDownStartPage.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownStartPage.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numericUpDownStartPage_KeyUp);
            // 
            // groupBoxSinglePage
            // 
            this.groupBoxSinglePage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSinglePage.Controls.Add(this.btnViewPost);
            this.groupBoxSinglePage.Controls.Add(this.labelSinglePage);
            this.groupBoxSinglePage.Controls.Add(this.buttonViewPage);
            this.groupBoxSinglePage.Controls.Add(this.numericUpDownPage);
            this.groupBoxSinglePage.Location = new System.Drawing.Point(171, 380);
            this.groupBoxSinglePage.Name = "groupBoxSinglePage";
            this.groupBoxSinglePage.Size = new System.Drawing.Size(141, 165);
            this.groupBoxSinglePage.TabIndex = 13;
            this.groupBoxSinglePage.TabStop = false;
            this.groupBoxSinglePage.Text = "Single Page";
            // 
            // btnViewPost
            // 
            this.btnViewPost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnViewPost.Location = new System.Drawing.Point(6, 94);
            this.btnViewPost.Name = "btnViewPost";
            this.btnViewPost.Size = new System.Drawing.Size(129, 24);
            this.btnViewPost.TabIndex = 13;
            this.btnViewPost.Text = "View POST Page";
            this.btnViewPost.UseVisualStyleBackColor = true;
            this.btnViewPost.Click += new System.EventHandler(this.btnViewPost_Click);
            // 
            // labelSinglePage
            // 
            this.labelSinglePage.AutoSize = true;
            this.labelSinglePage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSinglePage.Location = new System.Drawing.Point(4, 23);
            this.labelSinglePage.Name = "labelSinglePage";
            this.labelSinglePage.Size = new System.Drawing.Size(50, 17);
            this.labelSinglePage.TabIndex = 12;
            this.labelSinglePage.Text = "Page:";
            // 
            // buttonViewPage
            // 
            this.buttonViewPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonViewPage.Location = new System.Drawing.Point(6, 58);
            this.buttonViewPage.Name = "buttonViewPage";
            this.buttonViewPage.Size = new System.Drawing.Size(129, 24);
            this.buttonViewPage.TabIndex = 11;
            this.buttonViewPage.Text = "View Page";
            this.buttonViewPage.UseVisualStyleBackColor = true;
            this.buttonViewPage.Click += new System.EventHandler(this.buttonViewPage_Click);
            // 
            // numericUpDownPage
            // 
            this.numericUpDownPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownPage.Location = new System.Drawing.Point(52, 19);
            this.numericUpDownPage.Name = "numericUpDownPage";
            this.numericUpDownPage.Size = new System.Drawing.Size(83, 26);
            this.numericUpDownPage.TabIndex = 10;
            this.numericUpDownPage.KeyUp += new System.Windows.Forms.KeyEventHandler(this.buttonViewPage_Enter_KeyUp);
            // 
            // groupBoxAnalysis
            // 
            this.groupBoxAnalysis.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisEcg1);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisPeaks1);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisNoise1);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisButton);
            this.groupBoxAnalysis.Controls.Add(this.labelAnalysisButton);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisPeaks);
            this.groupBoxAnalysis.Controls.Add(this.labelAnalysisPeaks);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisNoise);
            this.groupBoxAnalysis.Controls.Add(this.labelAnalysisNoise);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisEcg);
            this.groupBoxAnalysis.Controls.Add(this.labelAnalysisEcg);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisBattery);
            this.groupBoxAnalysis.Controls.Add(this.labelAnalysisBattery);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisHz);
            this.groupBoxAnalysis.Controls.Add(this.labelAnalysisHz);
            this.groupBoxAnalysis.Controls.Add(this.textBoxAnalysisClock);
            this.groupBoxAnalysis.Controls.Add(this.labelAnalysisClock);
            this.groupBoxAnalysis.Location = new System.Drawing.Point(13, 237);
            this.groupBoxAnalysis.Name = "groupBoxAnalysis";
            this.groupBoxAnalysis.Size = new System.Drawing.Size(830, 77);
            this.groupBoxAnalysis.TabIndex = 14;
            this.groupBoxAnalysis.TabStop = false;
            this.groupBoxAnalysis.Text = "Analysis";
            this.groupBoxAnalysis.Enter += new System.EventHandler(this.groupBoxAnalysis_Enter);
            // 
            // textBoxAnalysisEcg1
            // 
            this.textBoxAnalysisEcg1.Location = new System.Drawing.Point(321, 47);
            this.textBoxAnalysisEcg1.Name = "textBoxAnalysisEcg1";
            this.textBoxAnalysisEcg1.ReadOnly = true;
            this.textBoxAnalysisEcg1.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisEcg1.TabIndex = 16;
            // 
            // textBoxAnalysisPeaks1
            // 
            this.textBoxAnalysisPeaks1.Location = new System.Drawing.Point(544, 47);
            this.textBoxAnalysisPeaks1.Name = "textBoxAnalysisPeaks1";
            this.textBoxAnalysisPeaks1.ReadOnly = true;
            this.textBoxAnalysisPeaks1.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisPeaks1.TabIndex = 15;
            // 
            // textBoxAnalysisNoise1
            // 
            this.textBoxAnalysisNoise1.Location = new System.Drawing.Point(544, 19);
            this.textBoxAnalysisNoise1.Name = "textBoxAnalysisNoise1";
            this.textBoxAnalysisNoise1.ReadOnly = true;
            this.textBoxAnalysisNoise1.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisNoise1.TabIndex = 14;
            // 
            // textBoxAnalysisButton
            // 
            this.textBoxAnalysisButton.Location = new System.Drawing.Point(696, 19);
            this.textBoxAnalysisButton.Name = "textBoxAnalysisButton";
            this.textBoxAnalysisButton.ReadOnly = true;
            this.textBoxAnalysisButton.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisButton.TabIndex = 13;
            // 
            // labelAnalysisButton
            // 
            this.labelAnalysisButton.AutoSize = true;
            this.labelAnalysisButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalysisButton.Location = new System.Drawing.Point(637, 23);
            this.labelAnalysisButton.Name = "labelAnalysisButton";
            this.labelAnalysisButton.Size = new System.Drawing.Size(60, 17);
            this.labelAnalysisButton.TabIndex = 12;
            this.labelAnalysisButton.Text = "Button:";
            // 
            // textBoxAnalysisPeaks
            // 
            this.textBoxAnalysisPeaks.Location = new System.Drawing.Point(477, 47);
            this.textBoxAnalysisPeaks.Name = "textBoxAnalysisPeaks";
            this.textBoxAnalysisPeaks.ReadOnly = true;
            this.textBoxAnalysisPeaks.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisPeaks.TabIndex = 11;
            // 
            // labelAnalysisPeaks
            // 
            this.labelAnalysisPeaks.AutoSize = true;
            this.labelAnalysisPeaks.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalysisPeaks.Location = new System.Drawing.Point(421, 51);
            this.labelAnalysisPeaks.Name = "labelAnalysisPeaks";
            this.labelAnalysisPeaks.Size = new System.Drawing.Size(57, 17);
            this.labelAnalysisPeaks.TabIndex = 10;
            this.labelAnalysisPeaks.Text = "Peaks:";
            // 
            // textBoxAnalysisNoise
            // 
            this.textBoxAnalysisNoise.Location = new System.Drawing.Point(477, 19);
            this.textBoxAnalysisNoise.Name = "textBoxAnalysisNoise";
            this.textBoxAnalysisNoise.ReadOnly = true;
            this.textBoxAnalysisNoise.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisNoise.TabIndex = 9;
            // 
            // labelAnalysisNoise
            // 
            this.labelAnalysisNoise.AutoSize = true;
            this.labelAnalysisNoise.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalysisNoise.Location = new System.Drawing.Point(421, 23);
            this.labelAnalysisNoise.Name = "labelAnalysisNoise";
            this.labelAnalysisNoise.Size = new System.Drawing.Size(54, 17);
            this.labelAnalysisNoise.TabIndex = 8;
            this.labelAnalysisNoise.Text = "Noise:";
            // 
            // textBoxAnalysisEcg
            // 
            this.textBoxAnalysisEcg.Location = new System.Drawing.Point(254, 47);
            this.textBoxAnalysisEcg.Name = "textBoxAnalysisEcg";
            this.textBoxAnalysisEcg.ReadOnly = true;
            this.textBoxAnalysisEcg.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisEcg.TabIndex = 7;
            // 
            // labelAnalysisEcg
            // 
            this.labelAnalysisEcg.AutoSize = true;
            this.labelAnalysisEcg.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalysisEcg.Location = new System.Drawing.Point(192, 51);
            this.labelAnalysisEcg.Name = "labelAnalysisEcg";
            this.labelAnalysisEcg.Size = new System.Drawing.Size(45, 17);
            this.labelAnalysisEcg.TabIndex = 6;
            this.labelAnalysisEcg.Text = "ECG:";
            // 
            // textBoxAnalysisBattery
            // 
            this.textBoxAnalysisBattery.Location = new System.Drawing.Point(254, 19);
            this.textBoxAnalysisBattery.Name = "textBoxAnalysisBattery";
            this.textBoxAnalysisBattery.ReadOnly = true;
            this.textBoxAnalysisBattery.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisBattery.TabIndex = 5;
            // 
            // labelAnalysisBattery
            // 
            this.labelAnalysisBattery.AutoSize = true;
            this.labelAnalysisBattery.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalysisBattery.Location = new System.Drawing.Point(192, 23);
            this.labelAnalysisBattery.Name = "labelAnalysisBattery";
            this.labelAnalysisBattery.Size = new System.Drawing.Size(65, 17);
            this.labelAnalysisBattery.TabIndex = 4;
            this.labelAnalysisBattery.Text = "Battery:";
            // 
            // textBoxAnalysisHz
            // 
            this.textBoxAnalysisHz.Location = new System.Drawing.Point(88, 47);
            this.textBoxAnalysisHz.Name = "textBoxAnalysisHz";
            this.textBoxAnalysisHz.ReadOnly = true;
            this.textBoxAnalysisHz.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisHz.TabIndex = 3;
            // 
            // labelAnalysisHz
            // 
            this.labelAnalysisHz.AutoSize = true;
            this.labelAnalysisHz.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalysisHz.Location = new System.Drawing.Point(6, 51);
            this.labelAnalysisHz.Name = "labelAnalysisHz";
            this.labelAnalysisHz.Size = new System.Drawing.Size(87, 17);
            this.labelAnalysisHz.TabIndex = 2;
            this.labelAnalysisHz.Text = "Est 250Hz:";
            // 
            // textBoxAnalysisClock
            // 
            this.textBoxAnalysisClock.Location = new System.Drawing.Point(88, 19);
            this.textBoxAnalysisClock.Name = "textBoxAnalysisClock";
            this.textBoxAnalysisClock.ReadOnly = true;
            this.textBoxAnalysisClock.Size = new System.Drawing.Size(63, 26);
            this.textBoxAnalysisClock.TabIndex = 1;
            // 
            // labelAnalysisClock
            // 
            this.labelAnalysisClock.AutoSize = true;
            this.labelAnalysisClock.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalysisClock.Location = new System.Drawing.Point(6, 23);
            this.labelAnalysisClock.Name = "labelAnalysisClock";
            this.labelAnalysisClock.Size = new System.Drawing.Size(52, 17);
            this.labelAnalysisClock.TabIndex = 0;
            this.labelAnalysisClock.Text = "Clock:";
            // 
            // buttonEmcTest
            // 
            this.buttonEmcTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonEmcTest.Location = new System.Drawing.Point(4, 94);
            this.buttonEmcTest.Name = "buttonEmcTest";
            this.buttonEmcTest.Size = new System.Drawing.Size(118, 24);
            this.buttonEmcTest.TabIndex = 15;
            this.buttonEmcTest.Text = "Start EMC Test";
            this.buttonEmcTest.UseVisualStyleBackColor = true;
            this.buttonEmcTest.Click += new System.EventHandler(this.buttonEmcTest_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // FW_Upgrade
            // 
            this.FW_Upgrade.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.FW_Upgrade.Enabled = false;
            this.FW_Upgrade.Location = new System.Drawing.Point(4, 59);
            this.FW_Upgrade.Name = "FW_Upgrade";
            this.FW_Upgrade.Size = new System.Drawing.Size(118, 24);
            this.FW_Upgrade.TabIndex = 19;
            this.FW_Upgrade.Text = "FW Upgrade";
            this.FW_Upgrade.UseVisualStyleBackColor = true;
            this.FW_Upgrade.Visible = false;
            this.FW_Upgrade.Click += new System.EventHandler(this.FW_Upgrade_Btn_Click);
            // 
            // groupBoxGeneral
            // 
            this.groupBoxGeneral.Controls.Add(this.FW_Upgrade);
            this.groupBoxGeneral.Controls.Add(this.buttonEmcTest);
            this.groupBoxGeneral.Controls.Add(this.buttonListDevices);
            this.groupBoxGeneral.Location = new System.Drawing.Point(13, 379);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(128, 159);
            this.groupBoxGeneral.TabIndex = 20;
            this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Text = "General";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 569);
            this.Controls.Add(this.groupBoxGeneral);
            this.Controls.Add(this.groupBoxAnalysis);
            this.Controls.Add(this.groupBoxSinglePage);
            this.Controls.Add(this.groupBoxPageRange);
            this.Controls.Add(this.groupBoxAllPages);
            this.Controls.Add(this.richTextBoxPages);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.progressBarOperation);
            this.Controls.Add(this.listViewDevices);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FormMain";
            this.Text = "Decatur Test (DM500)";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBoxAllPages.ResumeLayout(false);
            this.groupBoxPageRange.ResumeLayout(false);
            this.groupBoxPageRange.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPageCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartPage)).EndInit();
            this.groupBoxSinglePage.ResumeLayout(false);
            this.groupBoxSinglePage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPage)).EndInit();
            this.groupBoxAnalysis.ResumeLayout(false);
            this.groupBoxAnalysis.PerformLayout();
            this.groupBoxGeneral.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonListDevices;
        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.ColumnHeader columnHeaderSerial;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ProgressBar progressBarOperation;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelOperationText;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelOperation;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.ColumnHeader columnHeaderBytes;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTime;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTimeValue;
        private System.Windows.Forms.ColumnHeader columnHeaderMonitorPresent;
        private System.Windows.Forms.ColumnHeader columnHeaderPages;
        private System.Windows.Forms.RichTextBox richTextBoxPages;
        private System.Windows.Forms.GroupBox groupBoxAllPages;
        private System.Windows.Forms.Button buttonErase;
        private System.Windows.Forms.GroupBox groupBoxPageRange;
        private System.Windows.Forms.Button buttonDownloadRange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownPageCount;
        private System.Windows.Forms.Label labelStartPage;
        private System.Windows.Forms.NumericUpDown numericUpDownStartPage;
        private System.Windows.Forms.GroupBox groupBoxSinglePage;
        private System.Windows.Forms.Label labelSinglePage;
        private System.Windows.Forms.Button buttonViewPage;
        private System.Windows.Forms.NumericUpDown numericUpDownPage;
        private System.Windows.Forms.GroupBox groupBoxAnalysis;
        private System.Windows.Forms.TextBox textBoxAnalysisButton;
        private System.Windows.Forms.Label labelAnalysisButton;
        private System.Windows.Forms.TextBox textBoxAnalysisPeaks;
        private System.Windows.Forms.Label labelAnalysisPeaks;
        private System.Windows.Forms.TextBox textBoxAnalysisNoise;
        private System.Windows.Forms.Label labelAnalysisNoise;
        private System.Windows.Forms.TextBox textBoxAnalysisEcg;
        private System.Windows.Forms.Label labelAnalysisEcg;
        private System.Windows.Forms.TextBox textBoxAnalysisBattery;
        private System.Windows.Forms.Label labelAnalysisBattery;
        private System.Windows.Forms.TextBox textBoxAnalysisHz;
        private System.Windows.Forms.Label labelAnalysisHz;
        private System.Windows.Forms.TextBox textBoxAnalysisClock;
        private System.Windows.Forms.Label labelAnalysisClock;
        private System.Windows.Forms.Button buttonEmcTest;
        private System.Windows.Forms.ColumnHeader columnHeaderRevision;
        private System.Windows.Forms.Button btnDownloadAccel;
        private System.Windows.Forms.TextBox textBoxAnalysisPeaks1;
        private System.Windows.Forms.TextBox textBoxAnalysisNoise1;
        private System.Windows.Forms.TextBox textBoxAnalysisEcg1;
        private System.Windows.Forms.Button btnViewPost;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ColumnHeader columnHeaderMonitorFirmWare;
        private System.Windows.Forms.ColumnHeader columnHeaderMonitorSerial;
        private System.Windows.Forms.Button FW_Upgrade;
        private System.Windows.Forms.Button Bulk_Upload_Btn;
        private System.Windows.Forms.Button View_Analysis_Button;
        private System.Windows.Forms.ColumnHeader ColumnHeaderActiveTime;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox RichTextBox_LeadOffPages;
        private System.Windows.Forms.GroupBox groupBoxGeneral;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox textBoxRangeEndSec;
        private System.Windows.Forms.TextBox textBoxRangeEndMin;
        private System.Windows.Forms.TextBox textBoxRangeEndHour;
        private System.Windows.Forms.TextBox textBoxRangeEndDay;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox textBoxRangeStartSec;
        private System.Windows.Forms.TextBox textBoxRangeStartMin;
        private System.Windows.Forms.TextBox textBoxRangeStartHour;
        private System.Windows.Forms.TextBox textBoxRangeStartDay;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
    }
}

