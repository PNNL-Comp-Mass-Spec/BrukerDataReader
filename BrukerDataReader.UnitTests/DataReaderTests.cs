using System;
using System.IO;
using NUnit.Framework;
using System.Diagnostics;

namespace BrukerDataReader.UnitTests
{
    [TestFixture]
    public class DataReaderTests
    {
        // Ignore Spelling: acqus

        [Test]
        public void GetNumMSScans_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker_12T_ser_File1)
            {
                Parameters = new GlobalParameters
                {
                    NumValuesInScan = 1048576
                }
            };
            Assert.AreEqual(7, reader.GetNumMSScans());
        }

        [Test]
        public void GetNumMSScans_Test2()
        {
            var reader = new DataReader(FileRefs.Bruker_12T_FID_File1)
            {
                Parameters = new GlobalParameters
                {
                    NumValuesInScan = 524288
                }
            };
            Assert.AreEqual(1, reader.GetNumMSScans());
        }

        [Test]
        public void GetMassSpectrum_Bruker12T_FID_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker_12T_FID_File1)
            {
                Parameters =
                {
                    SampleRate = 909090.90909090906,
                    NumValuesInScan = 524288,
                    ML1 = 184345587.303392,
                    ML2 = 5.78258691122419
                }
            };

            reader.GetMassSpectrum(0, out var mzVals, out var intensities);

            Assert.NotNull(mzVals);
            Assert.AreNotEqual(0, mzVals.Length);

            TestUtilities.DisplayXYValues(mzVals, intensities);
        }

        [Test]
        public void GetMassSpectrum_Bruker9T_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1)
            {
                Parameters =
                {
                    ML1 = 144378935.472081,
                    ML2 = 20.3413771463121,
                    SampleRate = 740740.74074074,
                    NumValuesInScan = 524288
                }
            };

            Assert.AreEqual(4275, reader.GetNumMSScans());
            Assert.AreEqual(FileRefs.Bruker_9T_ser_File1, reader.FileName);

            const int testScan = 1000;
            var sw = new Stopwatch();
            sw.Start();
            reader.GetMassSpectrum(testScan, out var mzVals, out var intensities);
            sw.Stop();
            TestUtilities.DisplayXYValues(mzVals, intensities);
            Console.WriteLine();
            Console.WriteLine("Time= " + sw.ElapsedMilliseconds);

            reader.GetMassSpectrum(testScan, out _, out _);
        }

        [Test]
        public void Get_Summed_MassSpectrum_Bruker9T_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1)
            {
                Parameters =
                {
                    ML1 = 144378935.472081,
                    ML2 = 20.3413771463121,
                    SampleRate = 740740.74074074,
                    NumValuesInScan = 524288
                }
            };

            Assert.AreEqual(4275, reader.GetNumMSScans());
            Assert.AreEqual(FileRefs.Bruker_9T_ser_File1, reader.FileName);

            const int testScan = 1000;
            reader.GetMassSpectrum(testScan, out var mzVals, out var intensities);

            int[] testScans = { 999, 1000, 1001 };
            reader.GetMassSpectrum(testScans, out var mzValsSummed, out var intensitiesSummed);

            const int testIndex = 118966;

            Assert.AreEqual(713.6588m, (decimal)mzVals[testIndex]);
            Assert.AreEqual(4970526m, (decimal)intensities[testIndex]);

            Assert.AreEqual(713.6588m, (decimal)mzValsSummed[testIndex]);
            Assert.AreEqual(16332480m, (decimal)intensitiesSummed[testIndex]);

            //713.658752441406	4970525.5

        }

        [Test]
        public void GetMassSpectrum_Bruker15T_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker15TFile1)
            {
                Parameters =
                {
                    ML1 = 230337466.449292,                // ML1
                    ML2 = 7.00924491472374,                // ML2
                    SampleRate = 576923.0769230769 * 2,     // SW_h
                    NumValuesInScan = 524288                // TD
                }
            };

            Assert.AreEqual(18, reader.GetNumMSScans());

            const int testScan = 4;
            reader.GetMassSpectrum(testScan, out var mzVals, out var intensities);
            Assert.AreEqual(789.9679m, (decimal)mzVals[129658]);

            TestUtilities.DisplayXYValues(mzVals, intensities, 0, 714, 717);

            TestUtilities.DisplayXYValues(mzVals, intensities, 0, 857, 858);
        }

        [Test]
        public void GetMassSpectrum_Bruker15T_Test2()
        {
            var reader = new DataReader(FileRefs.Bruker15TFile2)
            {
                Parameters =
                {
                    ML1 = 230344000.2436387,                // ML1
                    ML2 = 8.3640041691183,                  // ML2
                    SampleRate = 2500000 * 2,                // SW_h
                    NumValuesInScan = 4194304                // TD
                }
            };

            Assert.AreEqual(21, reader.GetNumMSScans());

            const int testScan = 10;
            reader.GetMassSpectrum(testScan, out var mzVals, out var intensities);
            Assert.AreEqual(375.6136m, (decimal)mzVals[129658]);

            TestUtilities.DisplayXYValues(mzVals, intensities, 0, 428, 429);
        }
        [Test]
        public void GetMassSpectrum_Bruker15T_Test3()
        {
            var reader = new DataReader(FileRefs.Bruker15T_FID_File1)
            {
                // The parameters come from file apexAcquisition.method
                Parameters =
                {
                    ML1 = 230343708.8145794,                // ML1
                    ML2 = -14.171275734034667,              // ML2
                    SampleRate = 288461.53846153844 * 2,     // SW_h
                    NumValuesInScan = 524288                 // TD
                }
            };

            Assert.AreEqual(1, reader.GetNumMSScans());

            const int testScan = 0;
            reader.GetMassSpectrum(testScan, out var mzVals, out var intensities);
            Assert.AreEqual(1580.17m, (decimal)mzVals[129658]);

            TestUtilities.DisplayXYValues(mzVals, intensities, 0, 850, 865);
        }

        [Test]
        public void ReadParamFile_Test1()
        {
            var serFile = new FileInfo(FileRefs.Bruker15TFile1);
            var directory = serFile.Directory;

            if (directory == null)
                Assert.Fail("Unable to determine the parent directory of " + serFile.FullName);

            var settingsFilePath = Path.Combine(directory.FullName, "ESI_pos_150_3000.m", "apexAcquisition.method");
            if (!File.Exists(settingsFilePath))
                throw new FileNotFoundException("Settings file not found at " + settingsFilePath);

            var reader = new DataReader(FileRefs.Bruker15TFile1, settingsFilePath);

            Assert.AreEqual((float)230337466.4492918, (float)reader.Parameters.ML1);
            Assert.AreEqual((float)7.009244914723741, (float)reader.Parameters.ML2);
            Assert.AreEqual(524288, reader.Parameters.NumValuesInScan);
            Assert.AreEqual((float)(576923.0769230769 * 2), (float)reader.Parameters.SampleRate);
            Assert.AreEqual((float)399.2, (float)reader.Parameters.AcquiredMZMinimum);
            Assert.AreEqual((float)3000, (float)reader.Parameters.AcquiredMZMaximum);
        }

        [Test]
        public void ReadParamFile_Test2()
        {
            var serFile = new FileInfo(FileRefs.Bruker_12T_ser_File1);
            var directory = serFile.Directory;

            if (directory == null)
                Assert.Fail("Unable to determine the parent directory of " + serFile.FullName);

            var settingsFilePath = Path.Combine(directory.FullName, "ACQUS");
            if (!File.Exists(settingsFilePath))
                throw new FileNotFoundException("Settings file not found at " + settingsFilePath);

            var reader = new DataReader(FileRefs.Bruker15TFile1, settingsFilePath);

            Assert.AreEqual((float)184344245.950571, (float)reader.Parameters.ML1);
            Assert.AreEqual((float)10.0177657215039, (float)reader.Parameters.ML2);
            Assert.AreEqual(1048576, reader.Parameters.NumValuesInScan);
            Assert.AreEqual((float)(434782.60869565216 * 2), (float)reader.Parameters.SampleRate);
        }

        [Test]
        public void ReadParamFile_Test3()
        {
            var serFile = new FileInfo(FileRefs.Bruker_9T_ser_File1);
            var directory = serFile.Directory;

            if (directory == null)
                Assert.Fail("Unable to determine the parent directory of " + serFile.FullName);

            var settingsFilePath = Path.Combine(directory.FullName, "acqus");
            if (!File.Exists(settingsFilePath))
                throw new FileNotFoundException("Settings file not found at " + settingsFilePath);

            var reader = new DataReader(FileRefs.Bruker15TFile1, settingsFilePath);

            Assert.AreEqual((float)144378935.472081, (float)reader.Parameters.ML1);
            Assert.AreEqual((float)20.3413771463121, (float)reader.Parameters.ML2);
            Assert.AreEqual(524288, reader.Parameters.NumValuesInScan);
            Assert.AreEqual((float)(370370.37037037 * 2), (float)reader.Parameters.SampleRate);

            Assert.AreEqual((float)389.79956, (float)reader.Parameters.AcquiredMZMinimum);
            Assert.AreEqual((float)2500, (float)reader.Parameters.AcquiredMZMaximum);
        }

        [Test]
        public void GetMassSpectrum_smallMZRange_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1)
            {
                Parameters =
                {
                    ML1 = 144378935.472081,
                    ML2 = 20.3413771463121,
                    SampleRate = 740740.74074074,
                    NumValuesInScan = 524288
                }
            };

            const int testScan = 1000;

            const float minMZ = 695.5f;
            const float maxMz = 696.9f;

            reader.GetMassSpectrum(testScan, minMZ, maxMz, out _, out _);
        }

        [Test]
        public void GetMassSpectrum_extremeMZRange_test1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1)
            {
                Parameters =
                {
                    ML1 = 144378935.472081,
                    ML2 = 20.3413771463121,
                    SampleRate = 740740.74074074,
                    NumValuesInScan = 524288
                }
            };

            const int testScan = 1000;

            const float minMZ = 1f;
            const float maxMz = 1e7f;

            reader.GetMassSpectrum(testScan, minMZ, maxMz, out var mzVals, out _);
            var arrayLength = mzVals.Length;

            reader.GetMassSpectrum(testScan, out mzVals, out _);
            Assert.AreEqual(mzVals.Length, arrayLength);
        }

        [Test]
        public void SetParametersTest1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1);

            var gp = new GlobalParameters
            {
                ML1 = 144378935.472081,
                ML2 = 20.3413771463121,
                SampleRate = 740740.74074074,
                NumValuesInScan = 524288
            };

            reader.SetParameters(gp);

            Assert.AreEqual(144378935.472081, reader.Parameters.ML1);
            Assert.AreEqual(20.3413771463121, reader.Parameters.ML2);
        }

        [Test]
        public void SetParameters_alternateConstructorTest1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1);
            reader.SetParameters(144378935.472081, 20.3413771463121, 740740.74074074, 524288);
            Assert.AreEqual(144378935.472081, reader.Parameters.ML1);
            Assert.AreEqual(20.3413771463121, reader.Parameters.ML2);
        }

        [Test]
        public void ExceptionTest_inputScanNumTooHigh_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1)
            {
                Parameters =
                {
                    ML1 = 144378935.472081,
                    ML2 = 20.3413771463121,
                    SampleRate = 740740.74074074,
                    NumValuesInScan = 524288
                }
            };

            const int testScan = 5000;

            const float minMZ = 695.5f;
            const float maxMz = 696.9f;

            try
            {
                reader.GetMassSpectrum(testScan, minMZ, maxMz, out _, out _);
            }
            catch (PreconditionException ex)
            {
                Assert.That(ex.Message, Is.StringStarting("Cannot get mass spectrum. Requested scan number (5000) is greater than number of scans in dataset."));
                return;
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected exception", ex);
            }

            throw new Exception("PreconditionException was not thrown due to the out-of-range scan number; test failed");
        }

        [Test]
        public void ExceptionTest_inputScanNumTooHigh_Test2()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1)
            {
                Parameters =
                {
                    ML1 = 144378935.472081,
                    ML2 = 20.3413771463121,
                    SampleRate = 740740.74074074,
                    NumValuesInScan = 524288
                }
            };

            const int testScan = 5000;

            var ex = Assert.Throws<PreconditionException>(() => reader.GetMassSpectrum(testScan, out _, out _));
            Assert.That(ex.Message, Is.StringStarting("Cannot get mass spectrum. Requested scan number (5000) is greater than number of scans in dataset."));
        }
        [Test]
        public void ExceptionTest_parametersNotSet_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1);

            const int testScan = 1000;

            var ex = Assert.Throws<PreconditionException>(() => reader.GetMassSpectrum(testScan, out _, out _));
            Assert.That(ex.Message, Is.EqualTo("Cannot get mass spectrum. Need to first set Parameters."));
        }

        [Test]
        public void ExceptionTest_parametersNotSet_Test2()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1);

            const int testScan = 1000;
            const float minMZ = 695.5f;
            const float maxMz = 696.9f;

            var ex = Assert.Throws<PreconditionException>(() => reader.GetMassSpectrum(testScan, minMZ, maxMz, out _, out _));
            Assert.That(ex.Message, Is.EqualTo("Cannot get mass spectrum. Need to first set Parameters."));
        }

        [Test]
        public void ExceptionTest_maxMZ_smallerThanMinMZ_Test1()
        {
            var reader = new DataReader(FileRefs.Bruker_9T_ser_File1)
            {
                Parameters =
                {
                    ML1 = 144378935.472081,
                    ML2 = 20.3413771463121,
                    SampleRate = 740740.74074074,
                    NumValuesInScan = 524288
                }
            };

            const int testScan = 1000;
            const float minMZ = 700f;
            const float maxMz = 600f;

            var ex = Assert.Throws<PreconditionException>(() => reader.GetMassSpectrum(testScan, minMZ, maxMz, out _, out _));
            Assert.That(ex.Message, Is.EqualTo("Cannot get mass spectrum. MinMZ is greater than MaxMZ - that's impossible."));
        }
    }
}
