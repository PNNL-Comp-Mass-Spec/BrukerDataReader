using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using System.Diagnostics;

namespace BrukerDataReader.UnitTests
{
    [TestFixture]
    public class DataReader_PerformanceTesting
    {
        /// <summary>
        /// the test retrieves the same scan 20 times and ensures it is giving back the same value as the first scan retrieved.
        /// I need this test to make sure I'm moving the byte pointer properly. 
        /// </summary>
        [Test]
        public void sameScanGivesSameValuesOverandOver_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;

            Assert.AreEqual(4275, reader.GetNumMSScans());

            float[] mzvals = null;
            float[] intensities = null;

            List<long> timeList = new List<long>();

            int scanNum = 1000;
            reader.GetMassSpectrum(scanNum, ref mzvals, ref intensities);

            int testIndex = 100000;

            float testXVal = mzvals[testIndex];
            float testYVal = intensities[testIndex];


            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 20; i++)
            {
                sw.Start();

                Console.Write("scan= " + scanNum);


                reader.GetMassSpectrum(scanNum, ref mzvals, ref intensities);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();

                Assert.AreEqual(testXVal, mzvals[testIndex]);
                Assert.AreEqual(testYVal, intensities[testIndex]);

            }

            Console.WriteLine("Average time = " + timeList.Average());



        }

        public void consecutiveScans_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;

            Assert.AreEqual(4275, reader.GetNumMSScans());

            float[] mzvals = null;
            float[] intensities = null;

            List<long> timeList = new List<long>();

            int scanNum = 1000;
            reader.GetMassSpectrum(scanNum, ref mzvals, ref intensities);

            int testIndex = 100000;

            float testXVal = mzvals[testIndex];
            float testYVal = intensities[testIndex];


            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 100; i++)
            {
                sw.Start();

                int currentScan = scanNum + i;
                Console.Write("scan= " + currentScan);
                reader.GetMassSpectrum(currentScan, ref mzvals, ref intensities);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();



            }

            Console.WriteLine("Average time = " + timeList.Average());



        }

        public void consecutiveScans_SlowButSureTest1() // I find this to be about 3 to 10% slower than using a relative byte pointer
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;

            Assert.AreEqual(4275, reader.GetNumMSScans());

            float[] mzvals = null;
            float[] intensities = null;

            List<long> timeList = new List<long>();

            int scanNum = 1000;
            reader.GetMassSpectrum(scanNum, ref mzvals, ref intensities);

            int testIndex = 100000;

            float testXVal = mzvals[testIndex];
            float testYVal = intensities[testIndex];


            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 100; i++)
            {
                sw.Start();

                int currentScan = scanNum + i;
                Console.Write("scan= " + currentScan);
                reader.GetMassSpectrum(currentScan, ref mzvals, ref intensities);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();



            }

            Console.WriteLine("Average time = " + timeList.Average());



        }

        public void consecutiveScans_smallMZRangeTest1()     // result:  smaller m/z range doesn't make it quicker.
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;

            Assert.AreEqual(4275, reader.GetNumMSScans());

            float[] mzvals = null;
            float[] intensities = null;

            List<long> timeList = new List<long>();

            int scanNum = 1000;
            float minMZ = 695.5f;
            float maxMZ = 696.9f;


            reader.GetMassSpectrum(scanNum, minMZ, maxMZ, ref mzvals, ref intensities);

            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 100; i++)
            {
                sw.Start();

                int currentScan = scanNum + i;
                Console.Write("scan= " + currentScan);
                reader.GetMassSpectrum(currentScan, minMZ, maxMZ, ref mzvals, ref intensities);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();



            }

            Console.WriteLine("Average time = " + timeList.Average());

        }
    }
}
