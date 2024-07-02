using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Decatur;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Collections.ObjectModel;

namespace Decatur
{

    public static class Util
    {
        public static Int32 Int32From24(byte[] data)
        {
            Int32 i;

            // 3 bytes big-endian
            i = (data[0] << 16) | (data[1] << 8) | data[2];
            // sign-extend
            if ((i & 0x00800000) != 0)
            {
                i |= unchecked((Int32)0xFF000000);
            }

            return i;
        }

        public static UInt32 UInt32From24(byte[] data)
        {
            UInt32 i;

            // 3 bytes big-endian
            i = (UInt32)((data[0] << 16) | (data[1] << 8) | data[2]);
            // do not sign-extend

            return i;
        }

         

        public static Tuple<Int32, Int32> IntPairFrom24(byte[] data)
        {
            Int32 first, second;

            first = (data[0] << 4) | ((data[1] & 0xF0) >> 4);
            // Sign extend from 12 to 32
            if ((first & 0x00000800) != 0)
            {
                first |= unchecked((Int32)0xFFFFF000);
            }

            second = ((data[1] & 0x0F) << 8) | data[2];
            // Sign extend from 12 to 32
            if ((second & 0x00000800) != 0)
            {
                second |= unchecked((Int32)0xFFFFF000);
            }

            return new Tuple<Int32, Int32>(first, second);
        }

    }



    public class DuetHeader
    {
        public const byte NORMAL = 0xAB;
        public const byte RESTART = 0x54;

        public byte Sentinel { get; private set; }
        public bool[] LeadOff { get; private set; }
        public UInt32 Voltage { get; private set; }
        public UInt32 Timestamp { get; private set; }
        public byte ButtonPress { get; private set; }
        public UInt32 StepCount { get; private set; }
        public Int32[] InitECG { get; private set; }
        public bool CRCIsValid { get; private set; }
        public UInt32 Sequence { get; private set; }
        public UInt64 AccelMS { get; private set; }
        public Int32[] AccelAxis { get; private set; }
        

        UInt32 crc;

        public DuetHeader()
        {
            LeadOff = new bool[2];
            InitECG = new int[2];
            AccelAxis = new int[3];
        }

        public void ParsePage(byte[] page)
        {
            byte[] tmp = new byte[8];

            Debug.Assert(page.Length >= 0x1000);

            Sentinel = page[0];

            if (Sentinel != NORMAL && Sentinel != RESTART)
            {
                // Invalid page! Don't bother parsing header
                return;
            }

            LeadOff[0] = (page[1] & 0x80) != 0;
            LeadOff[1] = (page[1] & 0x40) != 0;


            ButtonPress = (byte)((page[1] & 0x38) >> 3);

            // StepCount = (uint)(page[1] & 0x07);          

            Array.Copy(page, 2, tmp, 0, 3);
            InitECG[0] = Util.Int32From24(tmp);

            Array.Copy(page, 5, tmp, 0, 3);
            InitECG[1] = Util.Int32From24(tmp);

            Voltage = (UInt32)page[8];

            // 4 bytes big-endian
            Timestamp = (UInt32)((page[9] << 24) | (page[10] << 16) | (page[11] << 8) | page[12]);

            // 4 bytes big-endian
            Sequence = (UInt32)((page[26] << 24) | (page[27] << 16) | (page[28] << 8) | page[29]);

            // New: 8-bit step count
            StepCount = (UInt32)page[30];

            // 8 bytes big-endian
            Array.Copy(page, 13, tmp, 0, 8);
            Array.Reverse(tmp);
            AccelMS = BitConverter.ToUInt64(tmp, 0);

            Array.Copy(page, 21, tmp, 0, 3);
            Tuple<Int32, Int32> acc = Util.IntPairFrom24(tmp);
            AccelAxis[0] = acc.Item1;
            AccelAxis[1] = acc.Item2;

            AccelAxis[2] = (page[24] << 4) | ((page[25] & 0xF0) >> 4);
            if ((AccelAxis[2] & 0x00000800) != 0)                       // sign-extend
                AccelAxis[2] |= unchecked((Int32)0xFFFFF000);


            // 4 bytes big-endian
            //crc = BitConverter.ToUInt32(page, page.Length - 4);   // -- solo is little-endian
            crc = (UInt32)((page[4092] << 24) | (page[4093] << 16) | (page[4094] << 8) | page[4095]);   

            CRCIsValid = (ComputePageCRC(page) == crc);
        }



        public bool HeaderIsValid()
        {
            return (Sentinel == NORMAL || Sentinel == RESTART) && CRCIsValid;
        }


        public static string HeaderLine
        { get { return "Sentinel,Sequence,LeadOff1,Leadoff2,Chan1,Chan2,Voltage,Timestamp,ButtonPress,StepCount"; } }


        public string Format()
        {
            string str;

            str = String.Format("{0:X2},{1},{2},{3},{4,6},{5,6},{6},{7},{8},{9}",
                (CRCIsValid ? Sentinel : 0xCC),
   
                Sequence,
                LeadOff[0],LeadOff[1],
                InitECG[0], InitECG[1],
                Voltage,
                Timestamp,
                ButtonPress,
                StepCount
                );

            return str;
        }


        public static  UInt32 ComputePageCRC(byte[] page)
        {
            UInt32 crc = 0xFFFFFFFF;

            for(int i=0; i < page.Length - 4; i+=4)
            {
                // extract little-endian word
                UInt32 word = BitConverter.ToUInt32(page, i);

                crc = AdvanceCRC(crc, word);
            }
            return crc;
        }


        private static UInt32 AdvanceCRC(UInt32 initial, UInt32 input_value)
        {
            UInt32 crc = initial ^ input_value;

            for (int i = 0; i < 32; i++)
            {
                if((crc & 0x80000000) != 0)
                {
                    crc = (crc << 1) ^ 0x04C11DB7;
                }
                else
                {
                    crc = (crc << 1);
                }
            }
            return crc;
        }

    }

    //[Obsolete]
    //public struct IMUSample
    //{
    //    public Int32 X;
    //    public Int32 Y;
    //    public Int32 Z;

    //    public IMUSample(Int32 x, Int32 y, Int32 z)
    //    {
    //        X = x;
    //        Y = y;
    //        Z = z;
    //    }

    //    public override string ToString()
    //    {
    //        return String.Format("{0,4},{1,4},{2,4}", X, Y, Z);
    //    }
    //}



    public class IMUPageData
    {
        public const int IMU_OFFSET = 35;
        public const int IMU_COUNT = 169;

        Int32[] imusamples;

        public ReadOnlyCollection<Int32> Samples
        {
            get {
                return Array.AsReadOnly<Int32>(imusamples);
            }
        }

        public IMUPageData()
        {
            imusamples = new Int32[IMU_COUNT];
        }

        public void ParsePage(byte[] page)
        {
            int index;
            byte[] tmp = new byte[3];

            for (int i=0; i < IMU_COUNT; i += 2)
            {
 
                index = IMU_OFFSET + ((i/2)*3);

                Array.Copy(page, index, tmp, 0, 3);
                Tuple<Int32, Int32> pair1 = Util.IntPairFrom24(tmp);

                imusamples[i] = pair1.Item1;
                if(i+1 < IMU_COUNT)
                    imusamples[i+1] = pair1.Item2;
            }
        }

    }


    public class ECGPageData
    {
        const int ECG_OFFSET = (35 + 254);
        public const int ECG_COUNT = 1690;

        //public Int32[] Chan1deltas;
        //public Int32[] Chan2deltas;

        private Int32[] chan1;
        private Int32[] chan2;

        Int32 initial1, initial2;

        public Int32[][] ecgChannel
        {
            get
            {
                return new Int32[][]  { chan1, chan2 };
            }
        }


        public ECGPageData(Int32 init1, Int32 init2)
        {
            //Chan1deltas = new int[ECG_COUNT];
            //Chan2deltas = new int[ECG_COUNT];
            chan1 = new int[ECG_COUNT];
            chan2 = new int[ECG_COUNT];

            initial1 = init1;
            initial2 = init2;
        }


        public void ParsePage(byte[] page)
        {
            Int32 accum1 = initial1;
            Int32 accum2 = initial2;
            Int32 bitoffset, byteoffset;
            int  shift1;

            for (int i=0; i < ECG_COUNT; i++)
            {
                chan1[i] = accum1;
                chan2[i] = accum2;

                bitoffset = i * 18;
                byteoffset = ECG_OFFSET + bitoffset / 8;

                UInt16 w = (ushort)((page[byteoffset] << 8) | page[byteoffset + 1]);
                shift1 = 7 - (bitoffset % 8);

                Int32 val = (w >> shift1) & 0x1FF;
                if((val & 0x100) != 0)
                     val |= unchecked((Int32)0xFFFFFF00);
 
                accum1 += val;


                bitoffset += 9;
                byteoffset = ECG_OFFSET + bitoffset / 8;

                w = (ushort)((page[byteoffset] << 8) | page[byteoffset + 1]);
                shift1 = 7 - (bitoffset % 8);

                val = (w >> shift1) & 0x1FF;
                if ((val & 0x100) != 0)
                    val |= unchecked((Int32)0xFFFFFF00);
  
                accum2 += val;

            }
            // last deltas don't count, or do they?

        }

    }


    public class DuetData
    {

        List<DuetHeader> hdrs = new List<DuetHeader>();
        List<Int32> allimu = new List<Int32>();
        List<Int32> allChan1 = new List<int>();
        List<Int32> allChan2 = new List<int>();

        private double[][] filteredSamples;
        private byte[] statuspage;
        MemoryStream analysisStream = new MemoryStream();

        public ReadOnlyCollection<DuetHeader> Headers;
        public ReadOnlyCollection<Int32> IMUSamples;
        public ReadOnlyCollection<Int32> ECGChannel1;
        public ReadOnlyCollection<Int32> ECGChannel2;

        public ReadOnlyCollection<Int32>[] ECGChannel;


        // Analysis properties from DecaturData
        public bool Analyzed { get; private set; }
        public double AverageInterval { get; private set; }
        public double AverageFrequency { get; private set; }
        public double AverageBatteryVoltage { get; private set; }
        public double[] ECG { get; private set; }
        public double[] Noise { get; private set; }
        public int[] SamplesBetweenPeaks { get; private set; }
        public int ButtonPressSeconds { get; private set; }
        public int[] FirstPeakOffset { get; private set; }
        public int[] SecondPeakOffset { get; private set; }
        public string FirmwareVersion { get; private set; }
        public Dictionary<int, bool[]> Leadoff_data { get; private set; } = new Dictionary<int, bool[]>();
        //public bool[] Leadoff { get; private set; }


        public DuetData(byte[][] pages, bool analyze = false, byte[] statuspage = null)
        {
            Headers = this.hdrs.AsReadOnly();
            IMUSamples = this.allimu.AsReadOnly();
            ECGChannel1 = this.allChan1.AsReadOnly();
            ECGChannel2 = this.allChan2.AsReadOnly();
            ECGChannel = new ReadOnlyCollection<int>[] { ECGChannel1, ECGChannel2 };
            this.Analyzed = false;
            this.statuspage = statuspage;

            // Properties which now have one value for each ecg channel:
            ECG = new double[2];
            Noise = new double[2];
            SamplesBetweenPeaks = new int[2];
            FirstPeakOffset = new int[2];
            SecondPeakOffset = new int[2];

            ProcessPages(pages);

            if (analyze && Headers.Count >= 4)
                Analyze();

        }

        private void ProcessPages(byte[][] pages)
        {
            int page_index = 0; //page index

            foreach (byte[] p in pages)
            {
                //Page_index.Add(page_index++);

                DuetHeader hdr = new DuetHeader();
                IMUPageData imuPage = new IMUPageData();

                hdr.ParsePage(p);
                //hdr.LeadOff.CopyTo(leadoff, 0);
                Leadoff_data.Add(page_index, hdr.LeadOff);


                //    Console.WriteLine(hdr.Format());
                hdrs.Add(hdr);

                if (hdr.HeaderIsValid())
                {
                    ECGPageData ecg = new ECGPageData(init1: hdr.InitECG[0], init2: hdr.InitECG[1]);

                    imuPage.ParsePage(p);
                    allimu.AddRange(imuPage.Samples);

                    ecg.ParsePage(p);

                    allChan1.AddRange(ecg.ecgChannel[0]);
                    allChan2.AddRange(ecg.ecgChannel[1]);

                    // TODO: CHeck for sequence number errors?
                }
                else
                {
                    // Add dummy data when the page is invalid (Flag byte bad or bad crc)

                    for (int i = 0; i < IMUPageData.IMU_COUNT; i++)
                        allimu.Add(0);

                    for(int i=0; i < ECGPageData.ECG_COUNT; i++)
                    {
                        allChan1.Add(0);
                        allChan2.Add(0);
                    }
                }

                page_index++;
            }
        }


        
        public void WriteCSV(string path)
        {
            int valid;
            // "valid" field set to 0 if page is invalid
            const string spacer = ",,,,,,,,,,,";
            string str;
            StreamReader sr = new StreamReader(analysisStream);

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("valid,ch1,ch2,filteredCh1,filteredCh2," + DuetHeader.HeaderLine);
                for (int j = 0; j < allChan1.Count; j++)
                {

                    valid = (hdrs[j / ECGPageData.ECG_COUNT].HeaderIsValid() ? 1 : 0);

                    sw.Write("{0}, {1}, {2}, {3}, {4}", valid, allChan1[j], allChan2[j], filteredSamples[0][j], filteredSamples[1][j]);

                    if((j % ECGPageData.ECG_COUNT) == 0)
                    {
                        sw.Write("," + hdrs[j / ECGPageData.ECG_COUNT].Format());
                    }

                    str = sr.ReadLine();
                    if (str != null)
                        sw.Write(spacer + str);

                    sw.WriteLine();
                }
            }

        }


        public void WriteIMUCSV(string path)
        {

            int line = 0;

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write("Magnitude,MeanSquare,OrientX,OrientY,OrientZ,StepCount");
                sw.WriteLine(",Firmware=" + FirmwareVersion);
                foreach (DuetHeader hdr in Headers)
                {
                    sw.WriteLine("{0},{1},{2},{3},{4},{5}", IMUSamples[line], hdr.AccelMS, hdr.AccelAxis[0], hdr.AccelAxis[1], hdr.AccelAxis[2], hdr.StepCount);
                    line++;
                    for(int i=1; i < IMUPageData.IMU_COUNT; i++)
                    {
                        sw.WriteLine("{0}", IMUSamples[line]);
                        line++;
                    }
                }
            }
        }

        #region Analysis Functions


        private void Analyze()
        {
            this.AnaylyzeInterval();
            this.AnalyzeFrequency();
            this.AnalyzeBatteryVoltage();
            this.AnalyzeWave(0);
            this.AnalyzeWave(1);
            this.AnalyzeECG(0);
            this.AnalyzeECG(1);
            this.AnalyzeButton();
            this.AnalyzeStatusPage();

            // Dave's filtering functions
            this.filteredSamples = new double[][] { new double[allChan1.Count], new double[allChan1.Count] };

            // converting ints to doubles
            for (int i = 0; i < allChan1.Count; i++)
            {
                filteredSamples[0][i] = (double)allChan1[i];
                filteredSamples[1][i] = (double)allChan2[i];
            }

            this.AdaptiveLineFilter(50.0f);
            this.AdaptiveLineFilter(60.0f);
            this.HiPassBaselineFilter();


            this.Analyzed = true;

            this.CreateAnalysisSummary();

           
        }


        private void CreateAnalysisSummary()
        {
            // Create summary to include in CSV

            analysisStream.Seek(0, SeekOrigin.Begin);
            analysisStream.SetLength(0);

            StreamWriter sw = new StreamWriter(analysisStream);
            sw.AutoFlush = true;

            sw.WriteLine();

            sw.WriteLine("FW Revision:,{0}", FirmwareVersion);
            sw.WriteLine();
            sw.WriteLine("Clock:,{0:F2}", AverageInterval);
            sw.WriteLine();
            sw.WriteLine("Est 250Hz:,{0:F2}", AverageFrequency);
            sw.WriteLine();
            sw.WriteLine("Vbat,{0:F3}V", AverageBatteryVoltage);
            sw.WriteLine(",Ch1,Ch2");
            sw.WriteLine("ECG:,{0:F3}mV,{1:F3}mV", ECG[0], ECG[1]);
            sw.WriteLine();
            sw.WriteLine("Noise:,{0:F1}uV,{1:F1}uV", Noise[0], Noise[1]);
            sw.WriteLine();
            //sw.WriteLine("Samples between peaks:,{0},{1}", SamplesBetweenPeaks[0], SamplesBetweenPeaks[1]);
            //sw.WriteLine();
            sw.WriteLine("Button press at time:,{0} sec", ButtonPressSeconds);
            sw.WriteLine();
        //    sw.Close();

            analysisStream.Seek(0, SeekOrigin.Begin);
 
        }


        private void AnaylyzeInterval()
        {
            const int intervalAverageOffset = 2;
            const int intervalAverageCount = 2;

            this.AverageInterval = 0;

            for (int index = intervalAverageOffset; index < intervalAverageOffset + intervalAverageCount; index++)
            {
                this.AverageInterval += this.hdrs[index].Timestamp;
            }

            this.AverageInterval /= intervalAverageCount;
            //this.AverageInterval = 32768f;
        }



        private void AnalyzeFrequency()
        {
 //           this.AverageFrequency = 250f * (32768f / (float)this.AverageInterval);
            this.AverageFrequency = 250f * 6.764f * (65536f / (float)this.AverageInterval);
        }

        private void AnalyzeBatteryVoltage()
        {
            const int batteryAverageOffset = 1;
            const int batteryAverageCount = 2;

            this.AverageBatteryVoltage = 0;

            for (int index = batteryAverageOffset; index < batteryAverageOffset + batteryAverageCount; index++)
            {
                //this.AverageBatteryVoltage += 3.0f + (this.hdrs[index].Voltage / 255f);
                this.AverageBatteryVoltage += ((this.hdrs[index].Voltage * 2) / 255f) + 1.5f;

            }

            this.AverageBatteryVoltage /= batteryAverageCount;// * 1000.0;                                  //  전압분배 미 적용
            //this.AverageBatteryVoltage = this.AverageBatteryVoltage + (this.AverageBatteryVoltage / 11);  //  전압분배 적용

  //          this.AverageBatteryVoltage *= (3.3f / 4096f);
        }


        private void AnalyzeWave(int chan)
        {
            const int firstPeakOffset = 100;
            const int firstPeakCount = 400;
            const int secondPeakCount = 350;
            int secondPeakOffset = 0;

            int max = int.MinValue;

            for (int index = firstPeakOffset; index < firstPeakOffset + firstPeakCount; index++)
            {
                if (this.ECGChannel[chan][index] > max)
                {
                    this.FirstPeakOffset[chan] = index;
                    max = this.ECGChannel[chan][index];
                }
            }

            secondPeakOffset = this.FirstPeakOffset[chan] + 30;

            max = int.MinValue;

            for (int index = secondPeakOffset; index < secondPeakOffset + secondPeakCount; index++)
            {
                if (this.ECGChannel[chan][index] > max)
                {
                    this.SecondPeakOffset[chan] = index;
                    max = this.ECGChannel[chan][index];
                }
            }

            this.SamplesBetweenPeaks[chan] = this.SecondPeakOffset[chan] - this.FirstPeakOffset[chan];
        }


        private void AnalyzeECG(int chan)
        {
            const int ecgOffset = 100;
            const int ecgCount = 750;
            const int noiseCount = 250;
            int noiseOffset = this.FirstPeakOffset[chan] + 30;

            int max = int.MinValue;
            int min = int.MaxValue;

            for (int index = ecgOffset; index < ecgOffset + ecgCount; index++)
            {
                max = Math.Max(this.ECGChannel[chan][index], max);
                min = Math.Min(this.ECGChannel[chan][index], min);
            }

            this.ECG[chan] = 1000f * (max - min) * (2.42f / 6f) / 131072f;

            max = int.MinValue;
            min = int.MaxValue;

            for (int index = noiseOffset; index < noiseOffset + noiseCount; index++)
            {
                max = Math.Max(this.ECGChannel[chan][index], max);
                min = Math.Min(this.ECGChannel[chan][index], min);
            }

            this.Noise[chan] = 1000000L * (long)(max - min) * (2.42d / 6d) / 131072f;
        }


        private void AnalyzeButton()
        {
            this.ButtonPressSeconds = -1;

            for (int index = 0; index < hdrs.Count; index++)
            {
                if(hdrs[index].ButtonPress != 0)
                {
                    // Decode the byte value
                    float time = ((float)(hdrs[index].ButtonPress - 1) * (6.764f/7f)) + ((float)index * 6.764f);
                    this.ButtonPressSeconds = (int)time;
                    break;
                }
            }

        }

        private void AnalyzeStatusPage()
        {
            string ver = "0.0.0";

            if (statuspage != null) //pgs[0]
            {
                int maj, min, rev;
                maj = statuspage[8];
                min = statuspage[9];
                rev = statuspage[10];

                if (maj != 0xFF && min != 0xFF && rev != 0xFF)
                {
                    ver = string.Format("{0}.{1}.{2}", maj, min, rev);
                }
            }
            this.FirmwareVersion = ver;
        }

        private void HiPassBaselineFilter()
        {
            // Butterworth single pole highpass 0.05 Hz at 250 Hz
            for (int chan = 0; chan < 2; chan++)
            {
                this.filteredSamples[chan][0] = 0f;

                for (int i = 1; i < this.filteredSamples[chan].Length - 1; i++)
                {
                    if ((this.filteredSamples[chan][i - 1] == 0) || (this.filteredSamples[chan][i] == 0))
                        this.filteredSamples[chan][i] = (this.filteredSamples[chan][i + 1] - this.filteredSamples[chan][i]) / 1.0006283186f;
                    else this.filteredSamples[chan][i] = (this.filteredSamples[chan][i + 1] - this.filteredSamples[chan][i]) / 1.0006283186f + 0.9987441518459681f * (this.filteredSamples[chan][i - 1]);
                }

                this.filteredSamples[chan][this.allChan1.Count - 1] = this.filteredSamples[chan][this.allChan1.Count - 2];
            }
            //this.filtered = true;
        }      

        private void AdaptiveLineFilter(float Freq)
        {
            float ECur, Dif;
            float[] XS = new float[2];
            float[] ES = new float[2];

            float TwoNAdapt = 2f * (float)Math.Cos(2f * 3.14159f * Freq / this.AverageFrequency);// NOTE adaptive const = 2*Cos(2*Pi*Line_freq/sampling freq) - set 
            //float Rate = 1f;
            float Rate = 0.5f;  // The adaptive rate

            for (int chan = 0; chan < 2; chan++)
            {
                XS.Initialize();
                ES.Initialize();

                for (int i = 0; i < this.filteredSamples[chan].Length; i++)
                {
                    XS[0] = XS[1];
                    XS[1] = (float)this.filteredSamples[chan][i];
                    ECur = TwoNAdapt * ES[1] - ES[0];
                    this.filteredSamples[chan][i] -= ECur;
                    Dif = (XS[1] - ECur) - (XS[0] - ES[1]);

                    if (Dif > 5f) ECur += Rate;
                    else if (Dif < -5f) ECur -= Rate;
                    ES[0] = ES[1];
                    ES[1] = ECur;
                }
            }
        }

        #endregion

    }


}