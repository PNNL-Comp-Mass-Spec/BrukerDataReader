using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;

namespace BrukerDataReader.UnitTests
{
    [TestFixture]
    public class DataReaderTests
    {

        [Test]
        public void GetNumMSScans_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_12T_ser_File1);
            reader.Parameters = new GlobalParameters();
            reader.Parameters.NumValuesInScan = 524288;
            Assert.AreEqual(8, reader.GetNumMSScans());
        }

        [Test]
        public void GetNumMSScans_Test2()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_12T_FID_File1);
            reader.Parameters = new GlobalParameters();
            reader.Parameters.NumValuesInScan = 524288;
            Assert.AreEqual(1, reader.GetNumMSScans());
        }

        [Test]
        public void GetMassSpectrum_Bruker12T_FID_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_12T_FID_File1);
            reader.Parameters.SampleRate = 909090.90909090906;
            reader.Parameters.NumValuesInScan = 524288;
            reader.Parameters.CalA = 184345587.303392;
            reader.Parameters.CalB = 5.78258691122419;

            float[] mzvals = null;
            float[] intensities = null;

            reader.GetMassSpectrum(0, ref mzvals, ref intensities);

            Assert.NotNull(mzvals);
            Assert.AreNotEqual(0, mzvals.Length);

            TestUtilities.DisplayXYValues(mzvals, intensities);
        }







        [Test]
        public void GetMassSpectrum_Bruker9T_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);

            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;

            Assert.AreEqual(4275, reader.GetNumMSScans());
            Assert.AreEqual(FileRefs.Bruker_9T_ser_File1, reader.FileName);

            float[] mzvals = null;
            float[] intensities = null;

            int testScan = 1000;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            reader.GetMassSpectrum(testScan, ref mzvals, ref intensities);
            sw.Stop();
            TestUtilities.DisplayXYValues(mzvals, intensities);
            Console.WriteLine();
            Console.WriteLine("Time= " + sw.ElapsedMilliseconds);

            reader.GetMassSpectrum(testScan, ref mzvals, ref intensities);


        }


        [Test]
        public void Get_Summed_MassSpectrum_Bruker9T_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);

            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;

            Assert.AreEqual(4275, reader.GetNumMSScans());
            Assert.AreEqual(FileRefs.Bruker_9T_ser_File1, reader.FileName);

            float[] mzvals = null;
            float[] intensities = null;

            float[] mzValsSummed = null;
            float[] intensitiesSummed = null;


            int testScan = 1000;
            reader.GetMassSpectrum(testScan, ref mzvals, ref intensities);

            int[] testScans = { 999, 1000, 1001 };
            reader.GetMassSpectrum(testScans, ref mzValsSummed, ref intensitiesSummed);

            int testPoint = 118966;

            Assert.AreEqual(713.6588m, (decimal)mzvals[testPoint]);
            Assert.AreEqual(4970526m, (decimal)intensities[testPoint]);

            Assert.AreEqual(713.6588m, (decimal)mzValsSummed[testPoint]);
            Assert.AreEqual(16332480m, (decimal)intensitiesSummed[testPoint]);




            //713.658752441406	4970525.5

        }




        [Test]
        public void GetMassSpectrum_Bruker15T_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker15TFile1)
            {
                Parameters =
                {
                    CalA = 230337466.449292,                // ML1
                    CalB = 7.00924491472374,                // ML2
                    SampleRate = 576923.0769230769 * 2,     // SW_h
                    NumValuesInScan = 524288                // TD
                }
            };

            Assert.AreEqual(18, reader.GetNumMSScans());


            float[] mzvals = null;
            float[] intensities = null;
            int testScan = 4;
            reader.GetMassSpectrum(testScan, ref mzvals, ref intensities);
            Assert.AreEqual(789.9679m, (decimal)mzvals[129658]);

            TestUtilities.DisplayXYValues(mzvals, intensities, 0, 714, 717);

            TestUtilities.DisplayXYValues(mzvals, intensities, 0, 857, 858);

        }

        [Test]
        public void GetMassSpectrum_Bruker15T_Test2()
        {
            var reader = new DataReader(FileRefs.Bruker15TFile2)
            {
                Parameters =
                {
                    CalA = 230344000.2436387,                // ML1
                    CalB = 8.3640041691183,                  // ML2
                    SampleRate = 2500000 * 2,                // SW_h
                    NumValuesInScan = 4194304                // TD
                }
            };

            Assert.AreEqual(21, reader.GetNumMSScans());


            float[] mzvals = null;
            float[] intensities = null;
            int testScan = 10;
            reader.GetMassSpectrum(testScan, ref mzvals, ref intensities);
            Assert.AreEqual(375.6136m, (decimal)mzvals[129658]);

            TestUtilities.DisplayXYValues(mzvals, intensities, 0, 428, 429);

        }
        [Test]
        public void GetMassSpectrum_Bruker15T_Test3()
        {

            var reader = new DataReader(FileRefs.Bruker15T_FID_File1)
            {
                // The parameters come from file apexAcquisition.method
                Parameters =
                {
                    CalA = 230343708.8145794,                // ML1
                    CalB = -14.171275734034667,              // ML2
                    SampleRate = 288461.53846153844 * 2,     // SW_h
                    NumValuesInScan = 524288                 // TD
                }
            };

            Assert.AreEqual(1, reader.GetNumMSScans());


            float[] mzvals = null;
            float[] intensities = null;
            int testScan = 0;
            reader.GetMassSpectrum(testScan, ref mzvals, ref intensities);
            Assert.AreEqual(1580.17m, (decimal)mzvals[129658]);

            TestUtilities.DisplayXYValues(mzvals, intensities, 0, 850, 865);


        }




        [Test]
        public void GetMassSpectrum_smallMZRange_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;


            float[] mzvals = null;
            float[] intensities = null;

            int testScan = 1000;

            float minMZ = 695.5f;
            float maxMz = 696.9f;

            reader.GetMassSpectrum(testScan, minMZ, maxMz, ref mzvals, ref intensities);
        }


        [Test]
        public void GetMassSpectrum_extremeMZRange_test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;


            float[] mzvals = null;
            float[] intensities = null;

            int testScan = 1000;

            float minMZ = 1f;
            float maxMz = 1e7f;

            reader.GetMassSpectrum(testScan, minMZ, maxMz, ref mzvals, ref intensities);
            int arrayLength = mzvals.Length;

            reader.GetMassSpectrum(testScan, ref mzvals, ref intensities);
            Assert.AreEqual(mzvals.Length, arrayLength);
        }

        [Test]
        public void SetParametersTest1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);

            GlobalParameters gp = new GlobalParameters();
            gp.CalA = 144378935.472081;
            gp.CalB = 20.3413771463121;
            gp.SampleRate = 740740.74074074;
            gp.NumValuesInScan = 524288;

            reader.SetParameters(gp);

            Assert.AreEqual(144378935.472081, reader.Parameters.CalA);
            Assert.AreEqual(20.3413771463121, reader.Parameters.CalB);
        }

        [Test]
        public void SetParameters_alternateConstructorTest1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.SetParameters(144378935.472081, 20.3413771463121, 740740.74074074, 524288);
            Assert.AreEqual(144378935.472081, reader.Parameters.CalA);
            Assert.AreEqual(20.3413771463121, reader.Parameters.CalB);
        }

        [Test]
        public void ExceptionTest_inputScanNumTooHigh_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;


            float[] mzvals = null;
            float[] intensities = null;

            int testScan = 5000;

            float minMZ = 695.5f;
            float maxMz = 696.9f;

            var ex = Assert.Throws<BrukerDataReader.PreconditionException>(() => reader.GetMassSpectrum(testScan, minMZ, maxMz, ref mzvals, ref intensities));
            Assert.That(ex.Message, Is.EqualTo("Cannot get mass spectrum. Requested scan num (5000) is greater than number of scans in dataset."));

        }

        [Test]
        public void ExceptionTest_inputScanNumTooHigh_Test2()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;


            float[] mzvals = null;
            float[] intensities = null;

            int testScan = 5000;

            var ex = Assert.Throws<BrukerDataReader.PreconditionException>(() => reader.GetMassSpectrum(testScan, ref mzvals, ref intensities));
            Assert.That(ex.Message, Is.EqualTo("Cannot get mass spectrum. Requested scan num (5000) is greater than number of scans in dataset."));

        }
        [Test]
        public void ExceptionTest_parametersNotSet_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);

            float[] mzvals = null;
            float[] intensities = null;

            int testScan = 1000;

            var ex = Assert.Throws<BrukerDataReader.PreconditionException>(() => reader.GetMassSpectrum(testScan, ref mzvals, ref intensities));
            Assert.That(ex.Message, Is.EqualTo("Cannot get mass spectrum. Need to first set Parameters."));

        }

        [Test]
        public void ExceptionTest_parametersNotSet_Test2()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);

            float[] mzvals = null;
            float[] intensities = null;

            int testScan = 1000;
            float minMZ = 695.5f;
            float maxMz = 696.9f;

            var ex = Assert.Throws<BrukerDataReader.PreconditionException>(() => reader.GetMassSpectrum(testScan, minMZ, maxMz, ref mzvals, ref intensities));
            Assert.That(ex.Message, Is.EqualTo("Cannot get mass spectrum. Need to first set Parameters."));

        }

        [Test]
        public void ExceptionTest_maxMZ_smallerThanMinMZ_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.Parameters.CalA = 144378935.472081;
            reader.Parameters.CalB = 20.3413771463121;
            reader.Parameters.SampleRate = 740740.74074074;
            reader.Parameters.NumValuesInScan = 524288;

            float[] mzvals = null;
            float[] intensities = null;

            int testScan = 1000;
            float minMZ = 700f;
            float maxMz = 600f;

            var ex = Assert.Throws<BrukerDataReader.PreconditionException>(() => reader.GetMassSpectrum(testScan, minMZ, maxMz, ref mzvals, ref intensities));
            Assert.That(ex.Message, Is.EqualTo("Cannot get mass spectrum. MinMZ is greater than MaxMZ - that's impossible."));

        }



    }
}
