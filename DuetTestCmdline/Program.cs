using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Decatur;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;

namespace DuetTestCmdline
{
    class Program
    {
        static List<DecaturDevice> devices = new List<DecaturDevice>();
        static DecaturDevice device;
        static int pagesize;

        static void Main(string[] args)
        {

            devices = Decatur.Decatur.GetConnectedDevices();
            if (devices.Count == 0)
            {
                Console.WriteLine("No cradle found.");
                return;
            }
            else
            {

                device = devices[0];

                Console.WriteLine("Cradle SN = {0}", device.SerialNumber);

                DecaturMonitorType t = device.MonitorType;
                Console.WriteLine("Type = {0}", t);
                pagesize = device.MonitorPageSize(t);
                //System.Console.WriteLine("Pagesize {0}", pagesize);
                Console.WriteLine("FW: {0}", fw_version());
                System.Console.WriteLine("Used Pages {0}", device.PageCount);
                //              System.Console.WriteLine("(Bytes {0})", device.BytesAvailable);


                Console.WriteLine("\n(1) Read speed test\n(2) Bad block scan (Duet)\n(3) Dump pages to binary file\n(4) Erase Flash\n(5) CRC test\n(6) Short CRC test");
                string inp = Console.ReadLine();

                if (!int.TryParse(inp, out int select)) select = 0;

                switch (select)
                {
                    case 1:
                        Console.Write("Number of pages to read? ");
                        string strnum = Console.ReadLine();
                        if (!int.TryParse(strnum, out int pages)) return;

                        readtest(pages);

                        break;

                    case 2:
                        badblock_scan();
                        break;
                    case 3:
                        Console.Write("Number of pages to read? ");
                        strnum = Console.ReadLine();
                        if (!int.TryParse(strnum, out pages)) return;

                        Console.Write("Start page? [0] ");
                        strnum = Console.ReadLine();
                        if (!int.TryParse(strnum, out int offset)) offset = 0;

                        read_to_file(pages, offset);

                        break;
                    case 4:
                        Console.Write("Enter 'Y' to confirm: ");
                        string ok = Console.ReadLine();
                        if (ok.ToUpper().Contains("Y"))
                        {
                            Console.WriteLine("Erasing...");
                            int ret = (int)device.EraseFlash();
                            Console.WriteLine("EraseFlash Returned {0} ({1})", ret, (DecaturResult)ret);
                        }
                        break;
                    case 5:
                        Console.WriteLine("How many times? ");
                        inp = Console.ReadLine();
                        if (!int.TryParse(inp, out int repcount)) return;

                        for (int i = 0; i < repcount; i++)
                        {
                            crc_test(shorttest: false);
                        }
                        break;
                    case 6:
                        Console.WriteLine("How many times? ");
                        inp = Console.ReadLine();
                        if (!int.TryParse(inp, out repcount)) return;

                        for (int i = 0; i < repcount; i++)
                        {
                            crc_test(shorttest: true);
                        }
                        break;
                    default:
                        return;
                }

            }

            //   Console.ReadLine();
            Thread.Sleep(1000);
        }

        private static void readtest(int numpages)
        {
            Stopwatch stopWatch = new Stopwatch();

            Console.WriteLine("Reading {0} pages...", numpages);

            stopWatch.Start();
            byte[][] a = device.RetrievePages((uint)numpages, 0);
            stopWatch.Stop();
            if (a == null)
            {
                Console.WriteLine("Data not retrieved ({0})", device.LastResult);
                //   break;
            }
            else
            {
                Console.WriteLine("Got {0} pages in {1} msec", a.GetLength(0), stopWatch.ElapsedMilliseconds);

                long thruput = (long)(((long)a.GetLength(0) * (long)(pagesize) * 8000L) / stopWatch.ElapsedMilliseconds);

                Console.WriteLine("{0} bits/sec", thruput);
#if BLAH
                int zeropages = 0;
                int badpages = 0;

                Console.WriteLine("Pattern check...");
                // Check for our random data test pattern
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    UInt32 pg = BitConverter.ToUInt32(a[i], 0);
                    UInt64 longlong = BitConverter.ToUInt64(a[i], 0);

                    if(longlong == 0)
                    {
                        zeropages++;
                    }
                    else if (pg != i || !page_checksum(a[i]))
                    {
                        badpages++;
                        //break;
                    }
                }

                Console.WriteLine("Bad = {0}, Zero = {1}", badpages, zeropages);
#endif
#if DEBUG
                using (SHA256Cng sha = new SHA256Cng())
                {
                    for (int i = 0; i < a.GetLength(0); i++)
                    {
                        if (i < a.GetLength(0) - 1)
                            sha.TransformBlock(a[i], 0, pagesize, a[i], 0);
                        else
                            sha.TransformFinalBlock(a[i], 0, pagesize);
                    }
                    Console.WriteLine("SHA: {0}", BytesToStr(sha.Hash));
                }
#endif 
            }

        }


        static void parse_test(byte[][] a)
        {
            List<DuetHeader> hdrs = new List<DuetHeader>(a.GetLength(0));
            List<Int32> allimu = new List<Int32>();
            List<Int32> allChan1 = new List<int>();
            List<Int32> allChan2 = new List<int>();

            foreach (byte[] p in a)
            {
                DuetHeader hdr = new DuetHeader();
                IMUPageData i = new IMUPageData();

                hdr.ParsePage(p);
                Console.WriteLine(hdr.Format());
                hdrs.Add(hdr);

                ECGPageData ecg = new ECGPageData(init1: hdr.InitECG[0], init2: hdr.InitECG[1]);

                // TBD: if hdr.Sentinel is corrupt, don't parse data?

                i.ParsePage(p);
                allimu.AddRange(i.Samples);

                ecg.ParsePage(p);

                allChan1.AddRange(ecg.ecgChannel[0]);
                allChan2.AddRange(ecg.ecgChannel[1]);
            }


            //String path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            //using (StreamWriter sw = new StreamWriter(path + "\\" + "ecg.csv"))
            //{
            //    sw.WriteLine("ch1,ch2");
            //    for (int j = 0; j < allChan1.Count; j++)
            //    {
            //        sw.WriteLine("{0}, {1}", allChan1[j], allChan2[j]);
            //    }
            //}

        }


        static void badblock_scan()
        {
            int npages;
            
            if (device.MonitorType == DecaturMonitorType.DUET_4GB)
                npages = 2048;
            else if (device.MonitorType == DecaturMonitorType.DUET_8GB)
                npages = 4096;
            else
                return;

            // 2048/4096 blocks of 64 pages each


            for (uint i = 0; i < npages; i++)
            {
                byte[][] a = device.RetrievePages(1, i * 64);

                UInt64 dw = BitConverter.ToUInt64(a[0], 0);

                if (dw == 0)
                    Console.WriteLine("Block {0} is bad!", i);
                else
                    Console.Write(".");
            }


        }

        static void read_to_file(int numpages, int offset = 0)
        {
            Console.WriteLine("Reading {0} pages...", numpages);

            byte[][] a = device.RetrievePages((uint)numpages, (uint)offset);

            if(a == null)
            {
                Console.WriteLine("Failed!");
                return;
            }

            DateTime dt = DateTime.Now;
            String fname = dt.ToString("yyyyMMdd_HHmmss") + ".bin";

            String path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            Console.WriteLine("Writing to file {0} on Desktop", fname);

            using (FileStream fs = File.Create(path + "\\" + fname))
            {
                foreach(byte[] page in a)
                {
                    fs.Write(page, 0, page.Length);
                }
            }
            Console.WriteLine("Done!");
        }



        static void crc_test(bool shorttest = false)
        {
            int numpages = (shorttest? Math.Min(device.PageCount, 32*1024) : device.PageCount);
            int pagesize = device.MonitorPageSize(device.MonitorType);
            int good = 0;
            int bad = 0;
            int zeropages = 0;
            UInt32 expectedseq = 0;
            int sequence_errs = 0;

            Console.WriteLine("Checking {0} pages...", numpages);

            byte[][] a = device.RetrievePages((uint)numpages, 0);

            using (SHA256Cng sha = new SHA256Cng())
            {
  //              foreach (byte[] page in a)
                for(int index = 0; index < a.GetLength(0); index++)
                {
                    byte[] page = a[index];
                    UInt32 crc = DuetHeader.ComputePageCRC(page);
                    UInt32 crc2;

                    UInt64 longlong = BitConverter.ToUInt64(page, 0);

                    // Hack
                    if (device.MonitorType == DecaturMonitorType.SOLO)
                        crc2 = BitConverter.ToUInt32(page, pagesize - 4);
                    else
                        crc2 = (UInt32)((page[pagesize - 4] << 24) | (page[pagesize - 3] << 16) | (page[pagesize - 2] << 8) | page[pagesize - 1]);

                    if (longlong == 0)
                        zeropages++;
                    else if (crc == crc2)
                        good++;
                    else
                        bad++;

                    // Hacky check for header sequence number errors
                    if (device.MonitorType != DecaturMonitorType.SOLO)
                    {
                        if (page[0] == 0x54 || page[0] == 0xAB)
                        {
                            if (page[0] == 0x54)
                                expectedseq = 0;

                            UInt32 seq = (UInt32)((page[26] << 24) | (page[27] << 16) | (page[28] << 8) | page[29]);

                            if (seq != expectedseq)
                                sequence_errs++;

                            expectedseq++;
                        }
                    }


                    if (index < a.GetLength(0) - 1)
                        sha.TransformBlock(page, 0, pagesize, page, 0);
                    else
                        sha.TransformFinalBlock(page, 0, pagesize);
                }

                Console.WriteLine("CRC Good = {0}, Bad = {1} Zero = {2}", good, bad, zeropages);
                if (device.MonitorType != DecaturMonitorType.SOLO)
                    Console.WriteLine("Sequence errs = {0}", sequence_errs);
                Console.WriteLine("SHA: {0}", BytesToStr(sha.Hash));
            }
        }


        static byte[][] read_binary_file(string filename)
        {
            byte[][] pages;
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                long npages = fs.Length / 4224;
                pages = new byte[npages][];

                for (int i = 0; i < npages; i++)
                {
                    byte[] page = new byte[4224];

                    fs.Read(page, 0, 4224);
                    pages[i] = page;
                }
            }

            return pages;
        }


        public static string fw_version()
        {
            string str = "0.0.0";

            if (device.MonitorType == DecaturMonitorType.ADX_4GB)
            {
                byte[][] a = device.RetrievePages(1, (uint)131071);

                if (a != null)
                {
                    int maj, min, rev;
                    maj = a[0][8];
                    min = a[0][9];
                    rev = a[0][10];

                    if (maj != 0xFF && min != 0xFF && rev != 0xFF)
                    {
                        str = string.Format("{0}.{1}.{2}", maj, min, rev);
                    }
                }
            }
            return str;
        }

        public static string BytesToStr(byte[] bytes)
        {
            StringBuilder str = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
                str.AppendFormat("{0:X2}", bytes[i]);

            return str.ToString();
        }

    }
}
