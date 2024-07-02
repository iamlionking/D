using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decatur
{
    public enum DecaturSignatureType
    {
        INVALID,
        STANDARD,
        RESTART
    }

    public class DecaturBlockHeader
    {
        public int OffsetSample { get; private set; }
        public int AbsoluteSample { get; private set; }
        public int Status { get; private set; }
        public int BatteryVoltage { get; private set; }
        public int Timestamp { get; private set; }
        public int TimeInterval { get; private set; }

        public bool ButtonPressed
        {
            get
            {
                return ((this.Status & 0x02) == 0x02);
            }
        }

        public bool LeadsOff
        {
            get
            {
                return ((this.Status & 0x04) == 0x04);
            }
        }

        public DecaturBlockHeader(int offsetSample, int absoluteSample, int status, int batteryVoltage, int timestamp, int timeInterval)
        {
            this.OffsetSample = offsetSample;
            this.AbsoluteSample = absoluteSample;
            this.Status = status;
            this.BatteryVoltage = batteryVoltage;
            this.Timestamp = timestamp;
            this.TimeInterval = timeInterval;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(this.OffsetSample)
                .Append(',')
                .Append(this.AbsoluteSample)
                .Append(',')
                .Append(this.Status)
                .Append(',')
                .Append(this.BatteryVoltage)
                .Append(',')
                .Append(this.Timestamp);

            return builder.ToString();
        }

        public string ToShortString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(this.Status)
                .Append(',')
                .Append(this.BatteryVoltage)
                .Append(',')
                .Append(this.Timestamp)
                .Append(',')
                .Append(this.TimeInterval);

            return builder.ToString();
        }
    }

    public class DecaturData
    {
        private int c_Traces = 1;
        public const int PageSignature = 0xABCD;
        public const int PageSignatureReboot = 0xDCBA;

        private UInt32 POLYNOME = 0x04C11DB7;
        private UInt32 INITIAL_CRC_VALUE = 0xFFFFFFFF;
        private int LOWER = 0; /* lower limit */
        private int STEP = 1; /* step size */
        private int CRCUPPER = 4 * 8; /* CRC software upper limit */
        private int CRC_SHIFT = 1;
        private UInt32 MSB_MASK = 0x80000000;
        private int CRC_LEN = 4;

        private byte[][] pages;
        private List<int> restartSignaturePages = new List<int>();
        private List<int> invalidSignaturePages = new List<int>();
        private List<int> invalidCrcPages = new List<int>();
        private List<DecaturBlockHeader> blockHeaders = new List<DecaturBlockHeader>();
        private List<int> samples = new List<int>();
        private double[] filteredSamples;
        private bool filtered = false;
        private string monitorVersion = "Unknown";

        public ReadOnlyCollection<DecaturBlockHeader> BlockHeaders
        {
            get
            {
                return this.blockHeaders.AsReadOnly();
            }
        }

        public ReadOnlyCollection<int> Samples
        {
            get
            {
                return this.samples.AsReadOnly();
            }
        }

        public bool Analyzed { get; private set; }
        public int FirstPageIndex { get; private set; }
        public double AverageInterval { get; private set; }
        public double AverageFrequency { get; private set; }
        public double AverageBatteryVoltage { get; private set; }
        public double ECG { get; private set; }
        public double Noise { get; private set; }
        public int FirstPeakOffset { get; private set; }
        public int SecondPeakOffset { get; private set; }
        public int SamplesBetweenPeaks { get; private set; }
        public int ButtonPressSeconds { get; private set; }

        // TODO: Think about whether or not this should be by reference
        public DecaturData(byte[][] pages, int firstPageIndex, bool analyze)
        {
            this.pages = pages;
            this.FirstPageIndex = firstPageIndex;
            this.Analyzed = false;
            this.AverageInterval = 0;
            this.AverageFrequency = 0;
            this.AverageBatteryVoltage = 0;
            this.ECG = 0;

            this.Process(analyze);
        }

        private void Process(bool analyze)
        {
            this.monitorVersion = "Unknown";
            byte[][] streams = this.ParsePages();

            for (int index = 0; index < streams.Length; index++)
            {
                this.ParseStream(ref streams[index], (index != 0));
            }

            if (analyze)
            {
                this.Analyze();
            }
        }

        private void AnaylyzeInterval()
        {
            const int intervalAverageOffset = 9;
            const int intervalAverageCount = 10;

            this.AverageInterval = 0;

            for (int index = intervalAverageOffset; index < intervalAverageOffset + intervalAverageCount; index++)
            {
                this.AverageInterval += this.blockHeaders[index].TimeInterval;
            }

            this.AverageInterval /= intervalAverageCount;
        }

        private void AnalyzeFrequency()
        {
            this.AverageFrequency = 250f * (32768f / (float)this.AverageInterval);
        }

        private void AnalyzeBatteryVoltage()
        {
            const int batteryAverageOffset = 5;
            const int batteryAverageCount = 5;

            this.AverageBatteryVoltage = 0;

            for (int index = batteryAverageOffset; index < batteryAverageOffset + batteryAverageCount; index++)
            {
                this.AverageBatteryVoltage += this.blockHeaders[index].BatteryVoltage;
            }

            this.AverageBatteryVoltage /= batteryAverageCount;

            this.AverageBatteryVoltage *= (3.3f / 4096f);
        }

        private void AnalyzeWave()
        {
            const int firstPeakOffset = 100;
            const int firstPeakCount = 400;
            const int secondPeakCount = 350;
            int secondPeakOffset = 0;

            int max = int.MinValue;

            for(int index = firstPeakOffset; index < firstPeakOffset + firstPeakCount; index++)
            {
                if (this.samples[index] > max)
                {
                    this.FirstPeakOffset = index;
                    max = this.samples[index];
                }
            }

            secondPeakOffset = this.FirstPeakOffset + 30;

            max = int.MinValue;

            for(int index = secondPeakOffset; index < secondPeakOffset + secondPeakCount; index++)
            {
                if (this.samples[index] > max)
                {
                    this.SecondPeakOffset = index;
                    max = this.samples[index];
                }
            }

            this.SamplesBetweenPeaks = this.SecondPeakOffset - this.FirstPeakOffset;
        }

        private void AnalyzeECG()
        {
            const int ecgOffset = 100;
            const int ecgCount = 750;
            const int noiseCount = 250;
            int noiseOffset = this.FirstPeakOffset + 30;

            int max = int.MinValue;
            int min = int.MaxValue;

            for (int index = ecgOffset; index < ecgOffset + ecgCount; index++)
            {
                max = Math.Max(this.samples[index], max);
                min = Math.Min(this.samples[index], min);
            }

            this.ECG = 1000f * (max - min) * (2.42f / 6f) / 524288f;

            max = int.MinValue;
            min = int.MaxValue;

            for (int index = noiseOffset; index < noiseOffset + noiseCount; index++)
            {
                max = Math.Max(this.samples[index], max);
                min = Math.Min(this.samples[index], min);
            }

            this.Noise = 1000000L * (long)(max - min) * (2.42d / 6d) / 524288d;
        }

        private void AnalyzeButton()
        {
            this.ButtonPressSeconds = -1;

            for (int index = 0; index < this.blockHeaders.Count; index++)
            {
                if ((this.blockHeaders[index].Status & 0x02) > 0)
                {
                    this.ButtonPressSeconds = index;
                    break;
                }
            }
        }

        private void Analyze()
        {
            if (this.pages.Length < 4)
            {
                this.Analyzed = false;
                return;
            }

            this.AnaylyzeInterval();
            this.AnalyzeFrequency();
            this.AnalyzeBatteryVoltage();
            this.AnalyzeWave();
            this.AnalyzeECG();
            this.AnalyzeButton();

            this.filteredSamples = new double[this.samples.Count];

            for (int index = 0; index < this.samples.Count;index++ )
            {
                this.filteredSamples[index] = this.samples[index];
            }

            // TODO: Remove -Generating fake 50hz noise
            /*
            for (int index = 0; index < this.samples.Count; index++)
            {
                this.filteredSamples[index] = Math.Sin(index * 1.256636) * 10;
                //this.filteredSamples[index] = Math.Sin(index * 1.570795) * 10000;
                //this.filteredSamples[index] = Math.Sin(index * 0.089) * 10000;
                this.samples[index] = (int)this.filteredSamples[index];
            }
             */

            this.AdaptiveLineFilter(50.0f);
            this.AdaptiveLineFilter(60.0f);
            this.HiPassBaselineFilter();

            this.Analyzed = true;
        }

        private void HiPassBaselineFilter()
        {
            // Butterworth single pole highpass 0.05 Hz at 250 Hz
            this.filteredSamples[0] = 0f;

            for (int i = 1; i < this.filteredSamples.Length - 1; i++)
            {
                this.filteredSamples[i] = (this.filteredSamples[i + 1] - this.filteredSamples[i]) / 1.0006283186f + 0.9987441518459681f * (this.filteredSamples[i - 1]);
            }

            this.filteredSamples[this.samples.Count - 1] = this.filteredSamples[this.samples.Count - 2];

            this.filtered = true;
        }

        private void AdaptiveLineFilter(float Freq)
        {
            float ECur, Dif;
            float[] XS = new float[2];
            float[] ES = new float[2];

            float TwoNAdapt = 2f * (float)Math.Cos(2f * 3.14159f * Freq / this.AverageFrequency);// NOTE adaptive const = 2*Cos(2*Pi*Line_freq/sampling freq) - set 
            XS.Initialize();
            ES.Initialize();
            //float Rate = .1f;
            float Rate = 1f;

            for (int i = 0; i < this.filteredSamples.Length; i++)
            {
                XS[0] = XS[1];
                XS[1] = (float)this.filteredSamples[i];
                ECur = TwoNAdapt * ES[1] - ES[0];
                this.filteredSamples[i] -= ECur;
                Dif = (XS[1] - ECur) - (XS[0] - ES[1]);

                if (Dif > 5f) ECur += Rate;
                else if (Dif < -5f) ECur -= Rate;
                ES[0] = ES[1];
                ES[1] = ECur;
            }
        }

        private DecaturSignatureType CheckSignature(int pageIndex)
        {
            if (this.pages[pageIndex][0] == 0xAB && this.pages[pageIndex][1] == 0xCD)
            {
                return DecaturSignatureType.STANDARD;
            }
            else if (this.pages[pageIndex][0] == 0xDC && this.pages[pageIndex][1] == 0xBA)
            {
                return DecaturSignatureType.RESTART;
            }

            return DecaturSignatureType.INVALID;
        }

        private bool CheckCRC(int pageIndex)
        {
            using(var stream = new BinaryReader(new MemoryStream(this.pages[pageIndex])))
            {
                UInt32 crc = this.INITIAL_CRC_VALUE;
                int words = (Decatur.PAGE_SIZE - this.CRC_LEN) / 4;

                while(words-- > 0)
                {
                    crc = CrcSoftwareFunc(crc, stream.ReadUInt32(), this.POLYNOME);
                }

                return (crc == stream.ReadUInt32());
            }
        }

        private byte[][] ParsePages()
        {
            List<byte[]> streams = new List<byte[]>();
            int restartIndex = 0;
            bool restart = false;
            int dataLength = (int)(this.pages.Length * Decatur.PAGE_SIZE - this.pages.Length * 6);

            if(this.FirstPageIndex == 0)
            {
                dataLength -= 10;
            }

            MemoryStream stream = new MemoryStream();

            do
            {
                restart = false;

                for (int index = restartIndex; index < this.pages.Length; index++)
                {
                    DecaturSignatureType signatureType = this.CheckSignature(index);

                    if (signatureType == DecaturSignatureType.INVALID)
                    {
                        this.invalidSignaturePages.Add(index);

                        // TODO: Handle gracefully by searching for the next 
                        continue;
                    }
                    else if (signatureType == DecaturSignatureType.RESTART)
                    {
                        this.restartSignaturePages.Add(index);
                        restart = true;
                        restartIndex = index + 1;
                        streams.Add(stream.ToArray());
                        stream.Close();
                        stream = new MemoryStream();
                        break;
                    }

                    if (!this.CheckCRC(index))
                    {
                        this.invalidCrcPages.Add(index);
                        continue;
                    }

                    if ((index == 0 && this.FirstPageIndex == 0) || (restartIndex != 0 && index == restartIndex))
                    {
                        this.monitorVersion = System.Text.Encoding.UTF8.GetString(this.pages[index], 2, 10);
                        stream.Write(this.pages[index], 12, (int)(Decatur.PAGE_SIZE - 16));
                    }
                    else
                    {
                        stream.Write(this.pages[index], 2, (int)(Decatur.PAGE_SIZE - 6));
                    }
                }
            } while (restart);

            streams.Add(stream.ToArray());

            return streams.ToArray();
        }

        private void ParseStream(ref byte[] data, bool restart)
        {
            Byte status;
            UInt16 voltage;
            UInt16 timestamp;
            int timeInterval;
            const int blockSize = 382;
            const int signatureLength = 2;
            const int crcLength = 4;
            const int versionLength = 10;

            const int dataSize = Decatur.PAGE_SIZE - signatureLength - crcLength;
            int finalSum = 0;
            UInt32 cursor = (UInt32)(this.FirstPageIndex * dataSize - versionLength);

            cursor %= blockSize;
            cursor = blockSize - cursor;

            if (this.FirstPageIndex == 0 || restart)
            {
                cursor = 0;
            }

            Int32 sample;
            Int32 sampleSave;
            bool firstSample = true;

            try
            {
                while (true)
                {
                    sample = data[cursor++] << 16;
                    sample |= data[cursor++] << 8;
                    sample |= data[cursor++];

                    if ((sample & 0x80000) != 0)
                    {
                        sample |= unchecked((Int32)0xFFF00000);
                    }

                    status = (Byte)(data[cursor] >> 5);
                    voltage = (UInt16)((data[cursor++] & 0xF) << 8);
                    voltage |= data[cursor++];
                    timestamp = (UInt16)(data[cursor++] << 8);
                    timestamp |= data[cursor++];

                    if (firstSample)
                    {
                        finalSum = sample;
                        firstSample = false;
                        timeInterval = 0;
                    }
                    else
                    {
                        timeInterval = this.blockHeaders[this.blockHeaders.Count - 1].Timestamp;

                        if (timestamp > timeInterval)
                        {
                            timeInterval = timestamp - timeInterval;
                        }
                        else
                        {
                            timeInterval = 0x10000 + timestamp - timeInterval;
                        }
                    }

                    this.blockHeaders.Add(new DecaturBlockHeader(finalSum, sample, status, voltage, timestamp, timeInterval));
                    this.samples.Add(sample);
                    sampleSave = sample;

                    for (int index = 0; index < 124; index++)
                    {
                        sample = data[cursor++] << 4;
                        sample |= data[cursor] >> 4;

                        if ((sample & 0x800) != 0)
                        {
                            sample |= unchecked((Int32)0xFFFFF000);
                        }

                        sampleSave -= sample;

                        this.samples.Add(sampleSave);

                        sample = (data[cursor++] & 0x0F) << 8;
                        sample |= data[cursor++];

                        if ((sample & 0x800) != 0)
                        {
                            sample |= unchecked((Int32)0xFFFFF000);
                        }

                        sampleSave -= sample;

                        this.samples.Add(sampleSave);

                        if (cursor >= data.Length)
                        {
                            return;
                        }
                    }

                    sample = data[cursor++] << 4;
                    sample |= data[cursor] >> 4;

                    if ((sample & 0x800) != 0)
                    {
                        sample |= unchecked((Int32)0xFFFFF000);
                    }

                    sampleSave -= sample;

                    this.samples.Add(sampleSave);

                    sample = (data[cursor++] & 0x0F) << 8;
                    sample |= data[cursor++];

                    if ((sample & 0x800) != 0)
                    {
                        sample |= unchecked((Int32)0xFFFFF000);
                    }

                    sampleSave -= sample;
                    finalSum = sampleSave;

                    if (cursor >= data.Length)
                    {
                        return;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private UInt32 CrcSoftwareFunc(UInt32 Initial_Crc, uint Input_Data, UInt32 POLY)
        {
            /* Initial XOR operation with the previous Crc value */
            UInt32 Crc = Initial_Crc ^ Input_Data;

            /* The CRC algorithm routine */
            for (int bindex = LOWER; bindex < CRCUPPER; bindex = bindex + STEP)
            {
                if ((Crc & MSB_MASK) != (UInt32)0)
                {
                    Crc = (Crc << CRC_SHIFT) ^ POLY;
                }
                else
                {
                    Crc = (Crc << CRC_SHIFT);
                }
            }
            return Crc;
        }

        public void WriteCSV(string path)
        {
            // TODO: Support unfiltered
            using (var file = new StreamWriter(File.Create(path)))
            {
                if (this.Analyzed)
                {
                    file.Write(this.filteredSamples[0]);
                    file.Write(',');
                    file.Write(this.blockHeaders[0]);
                    file.Write(',');
                    file.Write("sample #, Status, Vbat, timer, Counts");
                    file.WriteLine(",,,,,Pass/Fail");

                    file.Write(this.filteredSamples[1]);
                    file.Write(',');
                    file.Write(this.samples[1]);
                    file.Write(",,,,,251,");
                    file.Write(this.blockHeaders[1].ToShortString());
                    file.Write(",,,Clock:,");
                    file.Write(this.AverageInterval.ToString("#.0"));
                    file.WriteLine(",NA");

                    file.Write(this.filteredSamples[2]);
                    file.Write(',');
                    file.Write(this.samples[2]);
                    file.Write(",,,,,501,");
                    file.WriteLine(this.blockHeaders[2].ToShortString());

                    file.Write(this.filteredSamples[3]);
                    file.Write(',');
                    file.Write(this.samples[3]);
                    file.Write(",,,,,751,");
                    file.Write(this.blockHeaders[3].ToShortString());
                    file.Write(",,,Est 250Hz:,");
                    file.Write(this.AverageFrequency.ToString("#.00"));
                    file.WriteLine(",248.75 to 251.25");

                    file.Write(this.filteredSamples[4]);
                    file.Write(',');
                    file.Write(this.samples[4]);
                    file.Write(",,,,,1001,");
                    file.WriteLine(this.blockHeaders[4].ToShortString());

                    file.Write(this.filteredSamples[5]);
                    file.Write(',');
                    file.Write(this.samples[5]);
                    file.Write(",,,,,1251,");
                    file.Write(this.blockHeaders[5].ToShortString());
                    file.Write(",,,Vbat:,");
                    file.Write(this.AverageBatteryVoltage.ToString("#.000"));
                    file.WriteLine("V,NA");

                    file.Write(this.filteredSamples[6]);
                    file.Write(',');
                    file.Write(this.samples[6]);
                    file.Write(",,,,,1501,");
                    file.WriteLine(this.blockHeaders[6].ToShortString());

                    file.Write(this.filteredSamples[7]);
                    file.Write(',');
                    file.Write(this.samples[7]);
                    file.Write(",,,,,1751,");
                    file.Write(this.blockHeaders[7].ToShortString());
                    file.Write(",,,ECG:,");
                    file.Write(this.ECG.ToString("#.000"));
                    file.WriteLine("mV,1.95mV to 2.05mV");

                    file.Write(this.filteredSamples[8]);
                    file.Write(',');
                    file.Write(this.samples[8]);
                    file.Write(",,,,,2001,");
                    file.WriteLine(this.blockHeaders[8].ToShortString());

                    file.Write(this.filteredSamples[9]);
                    file.Write(',');
                    file.Write(this.samples[9]);
                    file.Write(",,,,,2251,");
                    file.Write(this.blockHeaders[9].ToShortString());
                    file.Write(",,,Noise:,");
                    file.Write(this.Noise.ToString("#.0"));
                    file.WriteLine("uV,<15uV");

                    file.Write(this.filteredSamples[10]);
                    file.Write(',');
                    file.Write(this.samples[10]);
                    file.Write(",,,,,2501,");
                    file.WriteLine(this.blockHeaders[10].ToShortString());

                    file.Write(this.filteredSamples[11]);
                    file.Write(',');
                    file.Write(this.samples[11]);
                    file.Write(",,,,,2751,");
                    file.Write(this.blockHeaders[11].ToShortString());
                    file.Write(",,,Samples between peaks:,");
                    file.Write(this.SamplesBetweenPeaks);
                    file.WriteLine(",311 to 314");

                    file.Write(this.filteredSamples[12]);
                    file.Write(',');
                    file.Write(this.samples[12]);
                    file.Write(",,,,,3001,");
                    file.WriteLine(this.blockHeaders[12].ToShortString());

                    file.Write(this.filteredSamples[13]);
                    file.Write(',');
                    file.Write(this.samples[13]);
                    file.Write(",,,,,3251,");
                    file.Write(this.blockHeaders[13].ToShortString());
                    file.Write(",,,Button Press at time:,");
                    file.Write(this.ButtonPressSeconds);
                    file.WriteLine(" sec");

                    file.Write(this.filteredSamples[14]);
                    file.Write(',');
                    file.Write(this.samples[14]);
                    file.Write(",,,,,3501,");
                    file.WriteLine(this.blockHeaders[14].ToShortString());

                    file.Write(this.filteredSamples[15]);
                    file.Write(',');
                    file.Write(this.samples[15]);
                    file.Write(",,,,,3751,");
                    file.Write(this.blockHeaders[15].ToShortString());
                    file.Write(",,,Monitor version:,");
                    file.WriteLine(this.monitorVersion);

                    for (int index = 16; index < filteredSamples.Length; index++)
                    {
                        if (index % 250 == 0)
                        {
                            file.Write(this.filteredSamples[index]);
                            file.Write(',');
                            file.Write(this.blockHeaders[index / 250]);

                            if (index < this.blockHeaders.Count)
                            {
                                file.Write(',');
                                file.Write(index * 250 + 1);
                                file.Write(',');
                                file.Write(this.blockHeaders[index].ToShortString());
                            }
                        }
                        else
                        {
                            file.Write(this.filteredSamples[index]);
                            file.Write(',');
                            file.Write(this.samples[index]);

                            if(index < this.blockHeaders.Count)
                            {
                                file.Write(",,,,,");
                                file.Write(index * 250 + 1);
                                file.Write(',');
                                file.Write(this.blockHeaders[index].ToShortString());
                            }
                        }

                        file.WriteLine();
                    }
                }
                else
                {
                    for (int index = 0; index < filteredSamples.Length; index++)
                    {
                        if (index % 250 == 0)
                        {
                            file.Write(this.filteredSamples[index]);
                            file.Write(',');
                            file.WriteLine(this.blockHeaders[index / 250]);
                        }
                        else
                        {
                            file.WriteLine(this.filteredSamples[index]);
                            file.Write(',');
                            file.Write(this.samples[index]);
                        }
                    }
                }
            }
        }
    }
}
