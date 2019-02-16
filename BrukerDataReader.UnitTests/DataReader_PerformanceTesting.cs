using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace BrukerDataReader.UnitTests
{
    [TestFixture]
    [Category("LongRunning")]
    public class DataReader_PerformanceTesting
    {
        /// <summary>
        /// the test retrieves the same scan 20 times and ensures it is giving back the same value as the first scan retrieved.
        /// I need this test to make sure I'm moving the byte pointer properly.
        /// </summary>
        [Test]
        public void sameScanGivesSameValuesOverAndOver_Test1()
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

            var timeList = new List<long>();

            var scanNum = 1000;
            reader.GetMassSpectrum(scanNum, out var mzVals, out var intensities);

            var testIndex = 100000;

            var testXVal = mzVals[testIndex];
            var testYVal = intensities[testIndex];


            var sw = new Stopwatch();
            for (var i = 0; i < 20; i++)
            {
                sw.Start();

                Console.Write("scan= " + scanNum);


                reader.GetMassSpectrum(scanNum, out mzVals, out intensities);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();

                Assert.AreEqual(testXVal, mzVals[testIndex]);
                Assert.AreEqual(testYVal, intensities[testIndex]);

            }

            Console.WriteLine("Average time = " + timeList.Average());



        }

        [Test]
        public void consecutiveScans_Test1()
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

            var timeList = new List<long>();

            var scanNum = 1000;
            reader.GetMassSpectrum(scanNum, out _, out _);

            var sw = new Stopwatch();
            for (var i = 0; i < 100; i++)
            {
                sw.Start();

                var currentScan = scanNum + i;
                Console.Write("scan= " + currentScan);
                reader.GetMassSpectrum(currentScan, out _, out _);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();



            }

            Console.WriteLine("Average time = " + timeList.Average());



        }

        [Test]
        public void consecutiveScans_SlowButSureTest1() // I find this to be about 3 to 10% slower than using a relative byte pointer
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

            var timeList = new List<long>();

            var scanNum = 1000;
            reader.GetMassSpectrum(scanNum, out _, out _);

            var sw = new Stopwatch();
            for (var i = 0; i < 100; i++)
            {
                sw.Start();

                var currentScan = scanNum + i;
                Console.Write("scan= " + currentScan);
                reader.GetMassSpectrum(currentScan, out _, out _);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();
            }

            Console.WriteLine("Average time = " + timeList.Average());



        }

        [Test]
        public void sumAcrossThreeScansTest1()
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

            var timeList = new List<long>();

            var scanNum = 1000;
            reader.GetMassSpectrum(scanNum, out _, out _);

            var sw = new Stopwatch();
            for (var i = 0; i < 100; i++)
            {
                sw.Start();

                var currentScan = scanNum + i;

                int[] scansToBeSummed = { currentScan - 1, currentScan, currentScan + 1 };

                Console.Write("scan= " + currentScan);
                reader.GetMassSpectrum(scansToBeSummed, out _, out _);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();



            }

            Console.WriteLine("Average time = " + timeList.Average());

        }




        [Test]
        public void consecutiveScans_smallMZRangeTest1()     // result:  smaller m/z range doesn't make it quicker.
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

            var timeList = new List<long>();

            var scanNum = 1000;
            var minMZ = 695.5f;
            var maxMZ = 696.9f;


            reader.GetMassSpectrum(scanNum, minMZ, maxMZ, out _, out _);

            var sw = new Stopwatch();
            for (var i = 0; i < 100; i++)
            {
                sw.Start();

                var currentScan = scanNum + i;
                Console.Write("scan= " + currentScan);
                reader.GetMassSpectrum(currentScan, minMZ, maxMZ, out _, out _);
                sw.Stop();
                Console.WriteLine("; time= " + sw.ElapsedMilliseconds);
                timeList.Add(sw.ElapsedMilliseconds);
                sw.Reset();



            }

            Console.WriteLine("Average time = " + timeList.Average());

        }
    }
}
