using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BrukerDataReader
{

    //TODO:  add apodization ability
    //TODO:  remove all dependence on DeconEngine  (FFT, apodization, etc).

    public class DataReader : IDisposable
    {
        string m_fileName = "";
        int m_numMSScans = -1;
        int m_lastScanOpened = 0;
        BinaryReader m_reader;
        long m_previousStartPosition = 0;
        long m_bytesAdvanced = 0;
        long m_lengthOfDataFileInBytes = 0;

        /// <summary>
        /// Constructor for the DataReader class
        /// </summary>
        /// <param name="fileName">Refers to the binary file containing the mass spectra data. For Bruker data, 
        /// this is a 'ser' or a 'fid' file</param>
        public DataReader(string fileName)
        {
            this.Parameters = new GlobalParameters();

            bool fileExists = File.Exists(fileName);

            if (File.Exists(fileName))
            {
                m_fileName = fileName;
            }
            else
            {
                throw new FileNotFoundException("Dataset could not be opened. File not found.");
            }


            using (BinaryReader reader = new BinaryReader(File.Open(m_fileName, FileMode.Open)))
            {
                m_lengthOfDataFileInBytes = reader.BaseStream.Length;
            }



        }


        #region Properties

        public GlobalParameters Parameters { get; set; }

        public string FileName
        {
            get { return m_fileName; }
        }


        #endregion

        #region Public Methods

        public void SetParameters(double calA, double calB, double sampleRate, int numValuesInScan)
        {
            GlobalParameters parameters = new GlobalParameters();
            parameters = new GlobalParameters();
            parameters.CalA = calA;
            parameters.CalB = calB;
            parameters.SampleRate = sampleRate;
            parameters.NumValuesInScan = numValuesInScan;

            this.Parameters = parameters;
        }

        public void SetParameters(GlobalParameters gp)
        {
            this.Parameters = gp;
        }

        public int GetNumMSScans()
        {

            //determine if the numMSScans was already stored or not. If not, open file and figure it out.
            bool numScansNotYetDetermined = (m_numMSScans == -1);

            if (numScansNotYetDetermined)
            {
                Check.Require(this.Parameters != null && this.Parameters.NumValuesInScan > 0, "Cannot determine number of MS Scans. Parameter for number of points in Scan has not been set.");

                using (BinaryReader reader = new BinaryReader(File.Open(m_fileName, FileMode.Open)))
                {
                    long fileLength = reader.BaseStream.Length;
                    long totalNumberOfValues = fileLength / sizeof(Int32);

                    m_numMSScans = (int)(totalNumberOfValues / Parameters.NumValuesInScan);
                }
            }
            return m_numMSScans;
        }

        /// <summary>
        /// Gets the mass spectrum.  Opens the BinaryReader and doesn't close it. Then finds the correct scan
        /// by using a relative position within the reader.  It turns out to be only ~3-4% faster. 
        /// </summary>
        /// <param name="scanNum">Zero-based scan number</param>
        public void GetMassSpectrumUsingSupposedlyFasterBinaryReader(int scanNum, ref float[] mzValues, ref float[] intensities)
        {
            Check.Require(Parameters != null, "Cannot get mass spectrum. Need to first set Parameters.");
            Check.Require(scanNum < GetNumMSScans(), "Cannot get mass spectrum. Requested scan num is greater than number of scans in dataset.");

            if (m_reader == null)
            {
                m_reader = new BinaryReader(File.Open(m_fileName, FileMode.Open));
            }

            float[] vals = new float[Parameters.NumValuesInScan];
            int diffBetweenCurrentAndPreviousScan = scanNum - m_lastScanOpened;

            long byteOffset = (long)diffBetweenCurrentAndPreviousScan * (long)Parameters.NumValuesInScan * (long)sizeof(Int32) - m_bytesAdvanced;

            if (byteOffset != 0)
            {
                m_reader.BaseStream.Seek(byteOffset, SeekOrigin.Current);

            }

            m_previousStartPosition = m_reader.BaseStream.Position;
            for (int i = 0; i < Parameters.NumValuesInScan; i++)
            {
                vals[i] = m_reader.ReadInt32();
            }
            m_bytesAdvanced = m_reader.BaseStream.Position - m_previousStartPosition;

            int lengthOfMZAndIntensityArray = Parameters.NumValuesInScan / 2;
            float[] mzValuesFullRange = new float[lengthOfMZAndIntensityArray];
            float[] intensitiesFullRange = new float[lengthOfMZAndIntensityArray];

            DeconEngine.Utils.FourierTransform(ref vals);

            for (int i = 0; i < lengthOfMZAndIntensityArray; i++)
            {
                float mz = (float)getMZ(i);
                float intensity = (float)(Math.Sqrt(vals[2 * i + 1] * vals[2 * i + 1] + vals[2 * i] * vals[2 * i]));

                int indexForReverseInsertion = (lengthOfMZAndIntensityArray - i - 1);
                mzValuesFullRange[indexForReverseInsertion] = mz;
                intensitiesFullRange[indexForReverseInsertion] = intensity;
            }

            //trim off m/z values according to parameters
            int indexOfLowMZ = getIndexForMZ(Parameters.MinMZ, lengthOfMZAndIntensityArray);
            int indexOfHighMZ = getIndexForMZ(Parameters.MaxMZ, lengthOfMZAndIntensityArray);



            mzValues = new float[indexOfHighMZ - indexOfLowMZ];
            intensities = new float[indexOfHighMZ - indexOfLowMZ];

            for (int i = indexOfLowMZ; i < indexOfHighMZ; i++)
            {
                mzValues[i - indexOfLowMZ] = mzValuesFullRange[i];
                intensities[i - indexOfLowMZ] = intensitiesFullRange[i];
            }

            m_lastScanOpened = scanNum;

        }

        /// <summary>
        /// Gets the mass spectrum. Main difference with 'GetMassSpectrumUsingSupposedlyFasterBinaryReader' is that a new BinaryReader is created
        /// everytime here. This is advantageous in terms of making sure the file is opened and closed properly.
        /// Unit tests show this to be about 3 to 10% slower. Presently (Nov 2010), since there isn't much speed gain, I favor this one.
        /// </summary>
        /// <param name="scanNum">Zero-based scan number</param>
        /// <param name="mzValues">m/z values are returned here</param>
        /// <param name="intensities">intensity values are returned here</param>
        public void GetMassSpectrum(int scanNum, ref float[] mzValues, ref float[] intensities)
        {
            Check.Require(Parameters != null && Parameters.CalA != -1, "Cannot get mass spectrum. Need to first set Parameters.");
            Check.Require(scanNum < GetNumMSScans(), "Cannot get mass spectrum. Requested scan num is greater than number of scans in dataset.");

            float[] vals = new float[Parameters.NumValuesInScan];

            using (BinaryReader reader = new BinaryReader(File.Open(m_fileName, FileMode.Open)))
            {
                long bytePosition = (long)scanNum * (long)Parameters.NumValuesInScan * (long)sizeof(Int32);

                reader.BaseStream.Seek(bytePosition, SeekOrigin.Begin);
                for (int i = 0; i < Parameters.NumValuesInScan; i++)
                {
                    vals[i] = reader.ReadInt32();
                }
            }

            int lengthOfMZAndIntensityArray = Parameters.NumValuesInScan / 2;
            float[] mzValuesFullRange = new float[lengthOfMZAndIntensityArray];
            float[] intensitiesFullRange = new float[lengthOfMZAndIntensityArray];

            DeconEngine.Utils.FourierTransform(ref vals);

            for (int i = 0; i < lengthOfMZAndIntensityArray; i++)
            {
                float mz = (float)getMZ(i);
                float intensity = (float)(Math.Sqrt(vals[2 * i + 1] * vals[2 * i + 1] + vals[2 * i] * vals[2 * i]));

                int indexForReverseInsertion = (lengthOfMZAndIntensityArray - i - 1);
                mzValuesFullRange[indexForReverseInsertion] = mz;
                intensitiesFullRange[indexForReverseInsertion] = intensity;
            }

            //trim off m/z values according to parameters
            int indexOfLowMZ = getIndexForMZ(Parameters.MinMZ, lengthOfMZAndIntensityArray);
            int indexOfHighMZ = getIndexForMZ(Parameters.MaxMZ, lengthOfMZAndIntensityArray);



            mzValues = new float[indexOfHighMZ - indexOfLowMZ];
            intensities = new float[indexOfHighMZ - indexOfLowMZ];

            for (int i = indexOfLowMZ; i < indexOfHighMZ; i++)
            {
                mzValues[i - indexOfLowMZ] = mzValuesFullRange[i];
                intensities[i - indexOfLowMZ] = intensitiesFullRange[i];
            }


        }


        public void GetMassSpectrum(int scanNum, float minMZ, float maxMZ, ref float[] mzValues, ref float[] intensities)
        {
            Check.Require(Parameters != null && Parameters.CalA != -1, "Cannot get mass spectrum. Need to first set Parameters.");
            Check.Require(scanNum < GetNumMSScans(), "Cannot get mass spectrum. Requested scan num is greater than number of scans in dataset.");
            Check.Require(maxMZ >= minMZ, "Cannot get mass spectrum. MinMZ is greater than MaxMZ - that's impossible.");


            Parameters.MinMZ = minMZ;
            Parameters.MaxMZ = maxMZ;

            GetMassSpectrum(scanNum, ref mzValues, ref intensities);
        }





        #endregion

        #region Private Methods

        private int getIndexForMZ(float targetMZ, int arrayLength)
        {
            int index = (int)((Parameters.NumValuesInScan / Parameters.SampleRate) * (Parameters.CalA / targetMZ - Parameters.CalB));
            index = arrayLength - index;

            if (index < 0)
            {
                return 0;
            }
            else if (index > arrayLength - 1)
            {
                return arrayLength - 1;
            }
            else
            {
                return index;
            }

        }

        private double getMZ(int i)
        {
            double freq = i * Parameters.SampleRate / Parameters.NumValuesInScan;

            double mass = 0;
            if (freq + Parameters.CalB != 0)
            {
                mass = Parameters.CalA / (freq + Parameters.CalB);
            }
            else if (freq - Parameters.CalB <= 0)
            {
                mass = Parameters.CalA;
            }
            else
            {
                mass = 0;
            }
            return mass;
        }
        internal long GetLengthOfDataFileInBytes()
        {
            return m_lengthOfDataFileInBytes;
        }


        #endregion



        #region IDisposable Members

        public void Dispose()
        {
            try
            {

                if (m_reader != null)
                {
                    using (BinaryReader br = m_reader)
                    {
                        br.Close();
                    }
                }

            }
            catch (Exception)
            {

                Console.WriteLine("BrukerDataReader had problems closing the binary reader. Note this.");
            }

        }

        #endregion
    }
}
