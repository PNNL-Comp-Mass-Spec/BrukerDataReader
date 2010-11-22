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

        double m_sampleRate = 909090.90909090906;
        double m_numPointsInScan = 524288;
        double m_calA = 184345587.303392;
        double m_calB = 5.78258691122419;



        [Test]
        public void openFileAndDisplaySomeContents()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_12T_FID_File1);

         

        }


        [Test]
        public void GetNumMSScans_Test1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_12T_ser_File1);
            reader.Parameters = new GlobalParameters();
            reader.Parameters.NumPointsInScan = 524288;
            Assert.AreEqual(8, reader.GetNumMSScans());
        }


        [Test]
        public void GetNumMSScans_Test2()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_12T_FID_File1);
            reader.Parameters = new GlobalParameters();
            reader.Parameters.NumPointsInScan = 524288;
            Assert.AreEqual(1, reader.GetNumMSScans());
        }

        [Test]
        public void GetMassSpectrumTest1()
        {
            DataReader reader = new DataReader(FileRefs.Bruker_12T_FID_File1);
            reader.Parameters.SampleRate = 909090.90909090906;
            reader.Parameters.NumPointsInScan = 524288;
            reader.Parameters.CalA = 184345587.303392;
            reader.Parameters.CalB= 5.78258691122419;

            float[]mzvals = null;
            float[]intensities=null;

            reader.GetMassSpectrum(0, ref mzvals, ref intensities);

            Assert.NotNull(mzvals);
            Assert.AreNotEqual(0, mzvals.Length);

            TestUtilities.DisplayXYValues(mzvals, intensities);

        }




      




        [Test]
        public void openAndReadBinaryTest1()
        {
            BinaryReader reader = new BinaryReader(File.Open(FileRefs.Bruker_12T_FID_File1, FileMode.Open));

            StringBuilder sb = new StringBuilder();

            long length = reader.BaseStream.Length;

            Console.WriteLine("Length is " + length);

            Console.WriteLine("number of 8 byte chunks = " + (double)length / 8d);

            int counter = 0;
            while (true)
            {
                counter++;
                if (counter > 1000) break;
                int testVal = reader.ReadInt32();
                //long testVal = reader.ReadInt64();
                sb.Append(testVal);
                sb.Append(Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());

            reader.Close();
        }


        [Test]
        public void openReadAndWriteOutToTextFileTest1()
        {
            BinaryReader reader = new BinaryReader(File.Open(FileRefs.Bruker_12T_FID_File1, FileMode.Open));
            long length = reader.BaseStream.Length;

            long numberOfPointsToRead = length / sizeof(Int32);

            int counter = 0;

            Console.WriteLine("Length is " + length);
            Console.WriteLine("number of 4 byte chunks = " + (double)length / 4d);
            using (StreamWriter sw = new StreamWriter(FileRefs.Bruker_12T_FID_File1 + ".txt"))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (counter < numberOfPointsToRead)
                {
                    counter++;
                    Int32 value = reader.ReadInt32();
                    sw.WriteLine(value);
                }
                stopwatch.Stop();
                Console.WriteLine("time= " + stopwatch.ElapsedMilliseconds);
            }

            reader.Close();
        }

        [Test]
        public void openReadandTryFFT_Test1()
        {
            BinaryReader reader = new BinaryReader(File.Open(FileRefs.Bruker_12T_FID_File1, FileMode.Open));
            long length = reader.BaseStream.Length;

            long numberOfPointsToRead = length / sizeof(Int32);

            int counter = 0;

            Console.WriteLine("Length is " + length);
            Console.WriteLine("number of 4 byte chunks = " + (double)length / 4d);

            List<long> data = new List<long>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float[] vals = new float[numberOfPointsToRead];

            for (int i = 0; i < numberOfPointsToRead; i++)
            {
                vals[i] = reader.ReadInt32();
            }

            int lengthOfMZAndIntensityArray = (int)numberOfPointsToRead / 2;

            double[] mzVals = new double[lengthOfMZAndIntensityArray];
            double[] intensities = new double[lengthOfMZAndIntensityArray];

            DeconEngine.Utils.FourierTransform(ref vals);

            for (int i = 0; i < lengthOfMZAndIntensityArray; i++)
            {
                double mz = getMZ(i);
                double intensity = Math.Sqrt(vals[2 * i + 1] * vals[2 * i + 1] + vals[2 * i] * vals[2 * i]);

                int indexForReverseInsertion = (lengthOfMZAndIntensityArray - i - 1);
                mzVals[indexForReverseInsertion] = mz;
                intensities[indexForReverseInsertion] = i;
            }


            stopwatch.Stop();

            using (StreamWriter sw = new StreamWriter(FileRefs.Bruker_12T_FID_File1 + "_ScanData.txt"))
            {

                for (int i = 0; i < mzVals.Length; i++)
                {
                    sw.Write(mzVals[i]);
                    sw.Write('\t');
                    sw.Write(intensities[i]);
                    sw.WriteLine();

                }

            }

            Console.WriteLine("time = " + stopwatch.ElapsedMilliseconds);


        }

        private double getMZ(int i)
        {
            double freq = i * m_sampleRate / m_numPointsInScan;

            double mass = 0;
            if (freq + m_calB != 0)
                mass = m_calA / (freq + m_calB);
            else if (freq - m_calB <= 0)
                mass = m_calA;
            else
                mass = 0;
            return mass;

        }





    }
}
